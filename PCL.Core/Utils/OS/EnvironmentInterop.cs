using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PCL.Core.Logging;
using PCL.Core.Utils.Exts;

namespace PCL.Core.Utils.OS;

public static partial class EnvironmentInterop
{
    private const string LogModule = "Environment";

    private static Dictionary<string, string>? _fileSecrets;

    /// <summary>
    /// SBL：从 exe 旁的 sbl-secrets.json 或 %APPDATA%\SBL\secrets.json 读取服务密钥。
    /// 用于在不重新编译的情况下启用微软登录与 CurseForge 下载。
    /// </summary>
    private static Dictionary<string, string> FileSecrets
    {
        get
        {
            if (_fileSecrets != null) return _fileSecrets;
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var paths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "sbl-secrets.json"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SBL", "secrets.json")
            };
            foreach (var path in paths)
            {
                try
                {
                    if (!File.Exists(path)) continue;
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    if (doc.RootElement.ValueKind != JsonValueKind.Object) continue;
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (prop.Value.ValueKind != JsonValueKind.String) continue;
                        var v = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(v)) result[prop.Name] = v!;
                    }
                    LogWrapper.Info(LogModule, $"已加载服务密钥文件：{path}（{result.Count} 项）");
                }
                catch (Exception ex)
                {
                    LogWrapper.Warn(LogModule, $"读取服务密钥文件失败：{path}：{ex.Message}");
                }
            }
            _fileSecrets = result;
            return result;
        }
    }

    /// <summary>
    /// 读取环境变量并使用 <see cref="StringExtension.Convert{T}"/> 将其转换为指定类型并写入目标引用。
    /// </summary>
    /// <param name="key">环境变量名</param>
    /// <param name="target">需要写入的目标引用 (不存在该环境变量或转换失败时不会写入)</param>
    /// <param name="detailLog">是否在日志中输出变量值</param>
    /// <typeparam name="TValue">目标引用的类型</typeparam>
    /// <returns>是否成功写入目标引用</returns>
    public static bool ReadVariable<TValue>(string key, ref TValue target, bool detailLog = true)
    {
        var envValue = Environment.GetEnvironmentVariable(key);
        if (envValue == null) return false;
        var valueLog = detailLog ? $" = {envValue}" : string.Empty;
        LogWrapper.Debug(LogModule, $"读取到环境变量 {key}{valueLog}");
        var value = envValue.Convert<TValue>();
        if (value == null)
        {
            LogWrapper.Warn(LogModule, $"环境变量 {key} 类型转换失败");
            return false;
        }
        target = value;
        return true;
    }

    public static string? GetSecret(string key, bool readEnv = true, bool readEnvDebugOnly = false)
    {
        // 1. 编译期注入的密钥
        if (SecretDictionary.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)) return value;
        // 2. SBL 本地密钥文件
        if (FileSecrets.TryGetValue(key, out var fileValue) && !string.IsNullOrEmpty(fileValue))
        {
            LogWrapper.Info(LogModule, $"已从本地密钥文件读取 {key}");
            return fileValue;
        }
        // 3. 环境变量（Release 下仅非 DebugOnly 的键）
        if (readEnv &&
#if !DEBUG
            !readEnvDebugOnly &&
#endif
            ReadVariable($"PCL_{key}", ref value, false)
        ) SecretDictionary[key] = value;
        return value;
    }
    
    /// <summary>
    /// 获取当前操作系统名称。
    /// </summary>
    /// <returns>返回小写的操作系统名称，如 "windows", "linux", "osx"。</returns>
    public static string GetCurrentOsName() {
        if (OperatingSystem.IsWindows())
            return "windows";
        if (OperatingSystem.IsLinux())
            return "linux";
        return OperatingSystem.IsMacOS() 
            ? "osx"
            : "unknown";
    }
}
