# CI/CD 配置说明

## 概述
本项目使用 GitHub Actions 自动构建和发布 MOD。

## 触发条件

### 自动触发
- **推送到 main 分支**: 自动构建并创建 Release
- **Pull Request**: 仅构建，不创建 Release

### 手动触发
在 GitHub 仓库的 Actions 页面，选择 "Build and Release" workflow，点击 "Run workflow"。

## 版本命名
使用时间戳作为版本号，格式: `YYMMDDHHmm`

例如:
- `2510251226` = 2025年10月25日 12:26

## 构建产物

### Artifact
每次构建都会上传 Artifact，保留 30 天：
- 文件名: `EfDEnhanced-{VERSION}.zip`
- 内容包括:
  - `EfDEnhanced.dll` - 主MOD文件
  - `0Harmony.dll` - Harmony框架
  - `info.ini` - MOD元数据
  - `preview.png` - MOD预览图（如果存在）
  - `Assets/` - 资源文件夹

### Release
推送到 main 分支时会自动创建 Release：
- Tag: `v{VERSION}`
- 附带构建的 ZIP 文件
- 包含安装说明

## 本地开发 vs CI 构建

### 本地开发
- 使用游戏目录中的 DLL: `C:/Program Files (x86)/Steam/steamapps/common/Escape from Duckov/Duckov_Data/Managed/`
- 构建后自动部署到游戏 Mods 目录

### CI 构建
- 使用项目 `Managed/` 目录中的 DLL
- 不部署到游戏目录
- 只创建 `output/` 目录和 ZIP 包

## 环境变量

CI 环境通过以下变量识别：
- `CI=true`
- `GITHUB_ACTIONS=true`

## DLL 管理

### 清理未使用的 DLL
运行清理脚本移除 Managed 目录中未使用的 DLL：

```powershell
.\scripts\cleanup-managed.ps1
```

### 当前保留的 DLL
只保留 csproj 中引用的 DLL：
- TeamSoda.*
- Unity*.dll / UnityEngine*.dll
- SodaLocalization.dll
- ItemStatsSystem.dll
- FMODUnity.dll
- ECM2.dll
- UniTask*.dll
- DOTween*.dll
- Eflatun.SceneReference.dll
- LeTai.TrueShadow.dll
- com.rlabrecque.steamworks.net.dll

## 故障排除

### 构建失败
1. 检查 Managed 目录是否包含所有必需的 DLL
2. 确保 `info.ini` 和 `preview.png` 存在
3. 查看 Actions 日志了解详细错误

### Release 未创建
1. 确保推送到 `main` 分支
2. 检查 GitHub Token 权限
3. 查看 workflow 日志

### 本地构建失败
1. 确保游戏已安装
2. 验证 csproj 中的 `DuckovPath` 路径正确
3. 运行 `dotnet restore` 重新安装依赖

## 工作流文件
- 位置: `.github/workflows/build-release.yml`
- 使用 Windows runner
- .NET SDK: 8.0.x
- PowerShell 脚本打包

## 权限要求
Workflow 需要以下权限（已在 repository settings 中默认启用）：
- `contents: write` - 创建 Release
- `actions: read` - 读取 workflow 状态
