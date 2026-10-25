# Build & configuration

## 构建

### 环境

- Windows 10 / 11 x64
- **.NET 10 SDK**（`PCL.Core` 使用 C# 14，SDK 8/9 编译不过）
- 可访问 NuGet 与 GitHub 的网络

### 命令

```powershell
dotnet publish "Plain Craft Launcher 2/Plain Craft Launcher 2.vbproj" `
  -c Release -p:Platform=x64 -r win-x64 --self-contained true -o dist
```

输出 `dist/SBL.exe`（单文件、自包含 .NET 8）。

### 项目结构

| 路径 | 说明 |
|---|---|
| `Plain Craft Launcher 2/` | VB.NET + WPF 的 PCL 界面与业务模块 |
| `PCL.Core/` | C# 核心库（配置、下载器、Java、启动服务等） |
| `PCL.Core.SourceGenerators/` | 配置 / 密钥源生成器 |
| `docs/CHANGES.patch` | 相对 PCL2-CE 上游的修改补丁 |

## 微软登录（可选）

SBL 使用微软 **设备代码流（device code flow）**。`MS_CLIENT_ID` 是**启动器的应用身份**，
不是账号密码，也不是每个用户各注册一个：注册一次后可以随包分发，所有朋友共用，
每个人在微软官方页面登录**自己的账号**。

### 注册 Azure 应用

1. 打开 <https://portal.azure.com> → **Microsoft Entra ID** → **应用注册** → **新注册**。
2. 名称：`SBL`；账户类型：**仅个人 Microsoft 账户**。
3. 复制 **应用程序(客户端) ID**。
4. **身份验证** → **允许公共客户端流** → **是** → 保存。
   - 设备代码流不需要客户端密码，也不需要重定向 URI。
5. 把 Client ID 写入 `sbl-secrets.json`（和 `SBL.exe` 同目录，或
   `%APPDATA%\SBL\secrets.json`）：

```json
{
  "MS_CLIENT_ID": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

6. 重启 SBL → 主页账号卡片 → 新建档案 → 正版验证 → 按提示打开
   <https://microsoft.com/link> 输入代码。

> 如果个人微软账号在 Azure 中无法创建租户，可尝试工作/学校账号，或改用
> 第三方验证（LittleSkin / authlib-injector）。

## CurseForge（可选）

1. 登录 <https://console.curseforge.com/>，打开左侧 **API keys**
   （<https://console.curseforge.com/#/api-keys>）。
2. 必须是 **Organization key / Partner account key**，并且组织的 API 申请已通过审核；
   **Per-game key** 只能访问你自己创建的游戏，搜索 Minecraft 会返回 403。
3. 申请入口：<https://forms.monday.com/forms/dce5ccb7afda9a1c21dab1a1aa1d84eb?r=use1>
4. 写入 `sbl-secrets.json`：

```json
{
  "CURSEFORGE_API_KEY": "$2a$10$..."
}
```

5. 重启 SBL；CF 模组 / 整合包 / 整合包导入可用。Modrinth 不需要密钥。

> 不要把 CurseForge Key 随包分发给朋友：那是你账号的配额与身份。
> 微软 Client ID 可以随包分发；CF Key 不可以。

## 验证密钥

```powershell
powershell -ExecutionPolicy Bypass -File docs/test-secrets.ps1
```

脚本只打印 `OK/FAIL`，不会输出密钥本身：

- 微软：请求 `devicecode`，200 表示 Client ID 与公共客户端流配置正确；
- CurseForge：请求 `mods/search?gameId=432`，200 表示具备公开模组搜索权限。

## 目录

- 配置：`%APPDATA%\SBL`
- 档案：`%APPDATA%\.sbl`
- 缓存：`%TEMP%\SBL`
- 游戏目录：`SBL.exe` 同目录的 `.minecraft`（或设置中指定）
