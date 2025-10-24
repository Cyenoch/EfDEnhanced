# EfDEnhanced - LLM 开发指引

## 项目概览
- **游戏**: Escape from Duckov (独立游戏，非Tarkov)
- **框架**: Unity + `Duckov.Modding.ModBehaviour`
- **语言**: C# (.NET Standard 2.1)
- **补丁**: Harmony 运行时注入

## 核心架构

### 入口点
- `ModBehaviour.cs` - 继承 `Duckov.Modding.ModBehaviour`
- `info.ini` - MOD元数据配置

### 目录结构
```
├── Features/          # 功能模块 (UI/逻辑)
├── Patches/           # Harmony补丁 (游戏修改)
├── Utils/             # 工具类 (日志/设置/UI组件)
├── extracted_assets/  # 游戏资源 (场景/模型/纹理)
└── docs/              # 详细文档
```

## 开发规范

### 代码组织
- 补丁按游戏系统分组放在 `Patches/`
- 功能模块放在 `Features/`
- 工具类放在 `Utils/`
- 一个功能 = 一个文件

### 日志系统
```csharp
ModLogger.Log("消息");                    // 普通日志
ModLogger.LogWarning("警告");             // 警告
ModLogger.LogError("错误");               // 错误
ModLogger.Log("组件名", "消息");          // 带组件标识

TransformTreeLogger.LogTransformTree(transform);
```

### Harmony补丁
```csharp
[HarmonyPatch(typeof(TargetClass), "TargetMethod")]
public static class MyPatch
{
    public static void Postfix()
    {
        try {
            // 补丁逻辑
        } catch (Exception ex) {
            ModLogger.LogError("MyPatch", ex.Message);
        }
    }
}
```

## 关键资源

### 反编译源码 (优先使用)
- 位置: `extracted_assets/` (搜索后读取特定文件)
- 包含完整C#实现，不仅是签名
- 使用 `grep` 搜索后 `read` 具体文件

### 游戏API
- 核心命名空间: `TeamSoda.Duckov.Core`
- 物品系统: `ItemStatsSystem`
- 移动系统: `ECM2`
- 本地化: `SodaLocalization`
- UI系统: `UIPanel`, `UIComponent` (使用 `Utils/UI/` 提供的组件)

### 常用命令
```bash
dotnet build                    # 构建
./scripts/deploy.sh            # 部署到游戏
./scripts/rlog.sh              # 实时查看日志
```

## 重要注意事项

### 禁止操作
- ❌ 不要读取整个目录
- ❌ 不要读取完整API文档文件
- ❌ 不要读取整个extracted_assets

### 推荐操作
- ✅ 使用 `grep` 搜索后读取特定文件
- ✅ 先搜索再读取
- ✅ 查看现有补丁了解模式

## 已实现功能

1. **移动增强** - 优化角色移动响应性
2. **武器对比** - 库存界面属性对比显示
3. **任务追踪** - Raid中实时任务进度HUD
4. **Raid检查** - 进入前自动检查装备/天气
5. **设置系统** - 完整游戏内设置界面
6. **按键绑定** - 自定义快捷键系统
7. **多语言** - 中英日文支持
8. **轮盘菜单** - 物品/投掷物快速选择
9. **Steam集成** - 创意工坊上传优化

## 故障排除

### MOD未加载
- 检查 `info.ini` 中的 `name` 与DLL文件名匹配
- 验证文件在 `Duckov_Data/Mods/EfDEnhanced/`
- 运行 `./scripts/rlog.sh` 查看日志

### 补丁不工作
- 使用 `grep` 验证目标方法存在
- 检查Unity控制台异常
- 在补丁中添加日志验证执行

## 开发流程

1. 搜索目标方法: `grep pattern:"MethodName" path:"extracted_assets"`
2. 创建补丁文件: `Patches/NewFeaturePatch.cs`
3. 实现补丁逻辑 (包裹try-catch)
4. 构建测试: `dotnet build && ./scripts/deploy.sh`
5. 查看日志: `./scripts/rlog.sh`
