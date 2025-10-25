# 清理 Managed 目录中未使用的 DLL
# 只保留 csproj 中实际引用的 dll

$managedDir = "Managed"

# 定义需要保留的 DLL 模式
$keepPatterns = @(
    "TeamSoda.*",
    "SodaLocalization.dll",
    "ItemStatsSystem.dll",
    "FMODUnity.dll",
    "ECM2.dll",
    "Unity*.dll",
    "UnityEngine*.dll",
    "UniTask*.dll",
    "Eflatun.SceneReference.dll",
    "LeTai.TrueShadow.dll",
    "com.rlabrecque.steamworks.net.dll",
    "DOTween*.dll",
    "0Harmony.dll"  # Harmony 是通过 NuGet 引用的，但可能在 Managed 中
)

# 获取所有 DLL 文件
$allDlls = Get-ChildItem -Path $managedDir -Filter "*.dll"

Write-Host "扫描 Managed 目录中的 DLL..."
Write-Host "总计: $($allDlls.Count) 个文件`n"

$keptDlls = @()
$removedDlls = @()

foreach ($dll in $allDlls) {
    $shouldKeep = $false

    foreach ($pattern in $keepPatterns) {
        # 转换通配符为正则表达式
        $regex = "^" + $pattern.Replace("*", ".*").Replace(".dll", "\.dll") + "$"

        if ($dll.Name -match $regex) {
            $shouldKeep = $true
            break
        }
    }

    if ($shouldKeep) {
        $keptDlls += $dll.Name
    } else {
        $removedDlls += $dll.Name
        Write-Host "[删除] $($dll.Name)" -ForegroundColor Red
        Remove-Item $dll.FullName -Force
    }
}

Write-Host "`n========================================="
Write-Host "清理完成!" -ForegroundColor Green
Write-Host "保留: $($keptDlls.Count) 个 DLL"
Write-Host "删除: $($removedDlls.Count) 个 DLL"
Write-Host "========================================="

Write-Host "`n保留的 DLL 列表:"
$keptDlls | Sort-Object | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }
