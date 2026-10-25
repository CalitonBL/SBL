param(
    [string]$Folder = ""
)
# 检查 SBL 的 sbl-secrets.json 是否可用（不会输出密钥本身）
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($Folder)) {
    $Folder = Split-Path -Parent $PSScriptRoot
    $Folder = Join-Path $Folder "dist\SBL"
}
$path = Join-Path $Folder "sbl-secrets.json"
if (-not (Test-Path $path)) {
    Write-Host "找不到 $path"
    Write-Host "请先把 sbl-secrets.json.example 复制为 sbl-secrets.json 并填写。"
    exit 1
}
$cfg = Get-Content $path -Raw -Encoding UTF8 | ConvertFrom-Json

if ($cfg.MS_CLIENT_ID) {
    $body = "client_id=$($cfg.MS_CLIENT_ID)&scope=XboxLive.signin%20offline_access"
    $code = curl.exe -sS -m 30 -o NUL -w "%{http_code}" -X POST `
        -H "Content-Type: application/x-www-form-urlencoded" `
        --data $body "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode"
    if ($code -eq "200") { Write-Host "[OK] 微软 Client ID 有效（devicecode 返回 200）" }
    else { Write-Host "[FAIL] 微软 Client ID 无效或未启用公共客户端流（HTTP $code）" }
} else {
    Write-Host "[SKIP] 未填写 MS_CLIENT_ID"
}

if ($cfg.CURSEFORGE_API_KEY) {
    $code = curl.exe -sS -m 30 -o NUL -w "%{http_code}" `
        -H "x-api-key: $($cfg.CURSEFORGE_API_KEY)" `
        "https://api.curseforge.com/v1/mods/search?gameId=432&pageSize=1"
    if ($code -eq "200") { Write-Host "[OK] CurseForge 密钥可以搜索 Minecraft 模组" }
    elseif ($code -eq "403") { Write-Host "[FAIL] CurseForge 返回 403：这不是 Organization key，或申请没有搜索权限" }
    else { Write-Host "[FAIL] CurseForge 返回 HTTP $code" }
} else {
    Write-Host "[SKIP] 未填写 CURSEFORGE_API_KEY"
}
