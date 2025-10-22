# CLAUDE.md

这是Raid Ready Check项目的AI助手指南，提供项目架构、开发规范和工作流程说明。

## 项目概述

Raid Ready Check是《Escape from Duckov》游戏的Raid前检查MOD，基于游戏的官方Modding框架构建。这是一个.NET Standard 2.1类库项目，通过继承`Duckov.Modding.ModBehaviour`来扩展游戏功能。

**项目目标**: 在玩家进入Raid前自动检查装备、物资、任务物品和天气，避免因忘带装备而导致的尴尬死亡。

**重要说明**: "Escape from Duckov"是一款独立游戏，不是"Escape from Tarkov"。

## 项目架构

### 目录结构

```
EfDEnhanced/
├── ModBehaviour.cs              # MOD入口点（继承游戏ModBehaviour）
├── extracted_assets             # 提取的游戏Unity资源，包含反编译的源代码
├── Utils/                       # 工具类
│   ├── ModLogger.cs             # 统一日志系统
│   ├── RaidCheckUtility.cs      # Raid检查核心逻辑
│   ├── LocalizationHelper.cs    # 多语言本地化支持
│   ├── ModSettings.cs           # 集中式设置管理器
│   ├── QuestTrackingManager.cs  # 任务追踪持久化
│   ├── StatPolarityMap.cs       # 武器属性正负性映射
│   ├── ExceptionHelper.cs       # 异常处理工具
│   ├── TransformTreeLogger.cs   # UI调试工具
│   ├── Settings/                # 设置条目类
│   │   ├── BoolSettingsEntry.cs
│   │   ├── FloatSettingsEntry.cs
│   │   ├── IntSettingsEntry.cs
│   │   ├── StringSettingsEntry.cs
│   │   ├── OptionsSettingsEntry.cs
│   │   └── SettingsEntry.cs
│   └── UI/                      # UI框架组件
│       ├── Core/                # 核心UI组件
│       │   └── ModPanel.cs      # 基础面板类
│       ├── Components/          # 可重用UI组件
│       │   ├── ModButton.cs
│       │   ├── ModSlider.cs
│       │   └── ModToggle.cs
│       ├── Constants/           # UI常量和样式
│       │   ├── UIConstants.cs
│       │   └── UIStyles.cs
│       ├── Builders/            # UI构建器
│       │   └── FormBuilder.cs   # 自动表单生成
│       └── Animations/          # UI动画
│           └── ModAnimations.cs
├── Features/                    # 功能模块
│   ├── ActiveQuestTracker.cs    # Raid中任务追踪HUD
│   ├── ModSettingsContent.cs    # 设置面板内容
│   ├── RaidPreparationView.cs   # Raid准备界面视图
│   └── README.md                # 功能详细说明（中文）
├── Patches/                     # Harmony补丁（按游戏系统分类）
│   ├── OptionsPanelPatch.cs     # 添加设置标签页到游戏设置
│   ├── MovementEnhancementPatch.cs # 移动响应性增强
│   ├── ItemHoveringComparisonPatch.cs # 武器属性对比
│   ├── QuestViewDetailsPatch.cs # 添加追踪复选框到任务详情
│   ├── RaidEntryPatches.cs      # Raid进入拦截补丁
│   ├── SceneTransitionPatches.cs # 场景转换处理
│   ├── InteractionDebugPatches.cs # 交互调试补丁（开发用）
│   └── WorkshopUploadPatch.cs   # 防止创意工坊描述覆盖
├── docs/                        # 文档
│   ├── api/                     # API文档和反编译代码
│   │   ├── decompiled/          # 反编译的游戏源码 ⭐
│   │   └── game_dll_api_documentation.md  # API签名文档
│   ├── game/                    # 游戏分析文档
│   │   └── game-analysis.md     # 游戏系统分析和MOD框架参考
│   ├── harmony/                 # Harmony补丁教程
│   │   ├── quickstart.md        # 快速参考
│   │   └── guide.md             # 完整指南
│   ├── localization-guide.md    # 本地化系统指南
│   └── assets/                  # 资源文档
│       └── assets-guide.md      # 提取资源使用指南
├── scripts/                     # 开发脚本
│   ├── decompile.sh             # 反编译游戏DLL
│   ├── deploy.sh                # 部署到游戏目录
│   └── rlog.sh                  # 实时查看游戏日志
├── output/                      # 构建输出（自动生成）
│   ├── EfDEnhanced.dll          # MOD主程序
│   ├── 0Harmony.dll             # Harmony依赖
│   ├── info.ini                 # MOD信息文件
│   └── preview.png              # 预览图（256x256）
├── info.ini                     # MOD配置文件
├── preview.png                  # 预览图源文件
├── README.md                    # 项目说明（英文，面向用户）
└── EfDEnhanced.csproj           # 项目配置文件
```

### 核心组件

#### ModBehaviour.cs - MOD入口
- 继承自`Duckov.Modding.ModBehaviour`（游戏MOD框架基类）
- 使用单例模式（Instance属性）
- 在`Awake()`中初始化Harmony并应用所有补丁
- 在`OnDisable()`中清理Harmony补丁

#### ModLogger - 统一日志系统
位置: `Utils/ModLogger.cs`

提供统一的日志接口，所有日志带`[EfDEnhanced]`前缀：
```csharp
ModLogger.Log("消息");                    // 普通日志
ModLogger.LogWarning("警告");             // 警告
ModLogger.LogError("错误");               // 错误
ModLogger.Log("组件名", "消息");          // 带组件标识的日志
```

**查看日志**:
- macOS: `~/Library/Logs/TeamSoda/Duckov/Player.log`
- 使用`./scripts/rlog.sh`实时监控日志

#### Patches/ - Harmony补丁目录
- 存放游戏功能修改补丁
- 使用`[HarmonyPatch]`特性标记补丁类
- 补丁在`ModBehaviour.Awake()`中通过`PatchAll()`自动应用
- 按游戏系统分类组织（如QuestPatches.cs, InventoryPatches.cs等）

### 关键依赖

项目引用的程序集来自游戏安装目录（`DuckovDataPath`定义在`.csproj`中）:
- `TeamSoda.*` - 游戏核心框架（Core、Utilities等）
- `SodaLocalization.dll` - 本地化系统
- `ItemStatsSystem.dll` - 物品统计系统
- `ECM2.dll` - Easy Character Movement 2 (移动系统)
- `UniTask.dll` - 异步操作库
- `LeTai.TrueShadow.dll` - 高质量UI阴影
- `DOTween.dll` + `DOTween.Modules.dll` - 动画补间库
- `com.rlabrecque.steamworks.net.dll` - Steam API集成
- `FMODUnity.dll` - 音频系统
- `Eflatun.SceneReference.dll` - 场景引用工具
- `UnityEngine.*` - Unity引擎程序集
- `0Harmony` (NuGet) - 运行时补丁库

游戏数据路径: `@EfDEnhanced.csproj > Project > PropertyGroup > DuckovDataPath`

## 开发命令

### 构建MOD
```bash
dotnet build                    # 开发构建
dotnet build -c Release         # 发布构建
dotnet clean                    # 清理构建产物
dotnet restore                  # 恢复依赖
```

### 开发脚本

#### 反编译游戏程序集（供LLM参考）
```bash
./scripts/decompile.sh
```
将游戏的`TeamSoda.*`、`ItemStatsSystem`、`Assembly-CSharp`等DLL反编译为可读的C#源码，输出到`decompiled/`。游戏更新后应重新运行此脚本。

#### 部署到游戏MOD目录
```bash
./scripts/deploy.sh
```
自动将编译好的DLL和资源文件复制到游戏的MOD目录。

#### 实时查看游戏日志
```bash
./scripts/rlog.sh
```
实时监控Unity Player日志，过滤并高亮显示EfDEnhanced相关日志。

#### 提取Unity资源（场景、Prefab等）
```bash
./scripts/extract-assets.sh
```
生成资源索引文档并提供AssetRipper使用指南。提取后的资源以YAML格式存储，便于LLM阅读和理解游戏场景结构、Prefab组成等。

## 文档资源

所有文档均为英文编写，针对AI辅助开发优化。详见**[docs/README.md](docs/README.md)**获取完整文档索引。

### 游戏分析与API参考

**[docs/game/game-analysis.md](docs/game/game-analysis.md)** - 游戏系统综合分析
- 游戏系统、机制和功能分析
- 物品、任务、建筑系统
- **MOD框架文档**，包含官方MOD仓库示例
- API命名空间组织和关键入口点

**[decompiled/](decompiled/)** - ⭐ **反编译的TeamSoda源码（强烈推荐）**
- **完整C#实现代码**，包含方法体，不仅是签名
- 按命名空间组织，便于导航
- 包含Core、Utilities和MiniLocalizor程序集
- **理解游戏系统实际工作原理的最佳资源**
- 使用`Grep`搜索，然后`Read`具体文件
- 参见[decompiled/README.md](decompiled/README.md)获取使用指南
- **⚠️ 重要**: 先用`Grep`搜索，不要直接读取整个目录

**[game_dll_api_documentation.md](game_dll_api_documentation.md)** - 完整API参考（30,000+行）
- 完整成员签名和继承链（仅签名，无实现）
- **⚠️ 重要**: 不要完整读取此文件！使用`grep`或`Grep`工具进行目标搜索
- 按需搜索特定类、命名空间或方法
- **查看实现请使用反编译源码**

### Harmony补丁文档

- **[docs/harmony/quickstart.md](docs/harmony/quickstart.md)** - 常用Harmony补丁模式快速参考
- **[docs/harmony/guide.md](docs/harmony/guide.md)** - 完整Harmony补丁指南和高级技巧

### Unity资源文档

**[docs/assets/assets-guide.md](docs/assets/assets-guide.md)** - Unity资源系统完整指南
- 资源类型组织和目录结构
- 场景、网格、纹理、精灵资源索引
- Prefab和地形系统说明
- 资源搜索和引用方法

**[docs/scenes/scenes-guide.md](docs/scenes/scenes-guide.md)** - 游戏场景系统指南
- 完整关卡列表和结构
- 场景组织和依赖关系
- 光照和环境设置
- Mod开发中的场景使用

**[extracted_assets/](extracted_assets/)** - 提取的Unity资源
- **场景结构** - 游戏关卡组织和光照设置
- **网格模型** - 2762个GLB格式的3D模型
- **纹理资源** - 3096个PNG纹理文件
- **精灵图集** - 1028个UI和2D图形JSON定义
- **Prefab对象** - 2195个预制件层级定义（GLB格式）
- **地形数据** - 13个地形网格文件
- **Resources目录** - Unity运行时加载的资源
- 示例: `Grep pattern:"LightingSettings" path:"extracted_assets/Assets/Scenes"`
- 详见 [docs/assets/assets-guide.md](docs/assets/assets-guide.md) 获取完整资源指南

### 日志系统

- **[docs/logging-guide.md](docs/logging-guide.md)** - ModLogger使用指南和日志位置

### 有效使用API文档

**推荐做法**:
- **优先使用反编译源码查看实现**: `Grep pattern:"class ModBehaviour" path:"decompiled"`
- **读取特定反编译文件**查看系统工作原理: `Read file_path:"decompiled/TeamSoda.Duckov.Core.cs"`
- **搜索提取的资源**: `Grep pattern:"LightingSettings" path:"extracted_assets/Assets/Scenes"`
- **查找特定资源类型**: `Grep pattern:"TerrainData" path:"extracted_assets"`
- 使用`Grep`工具搜索特定类型: `Grep pattern:"class MyClassName"`
- 按命名空间搜索: `Grep pattern:"namespace Duckov.Buffs"`
- 阅读 [docs/scenes/scenes-guide.md](docs/scenes/scenes-guide.md) 了解游戏关卡结构
- 阅读 [docs/assets/assets-guide.md](docs/assets/assets-guide.md) 了解资源系统
- 阅读 [docs/assets/mod-development-guide.md](docs/assets/mod-development-guide.md) 了解Mod开发最佳实践
- 检查`Patches/`中的现有补丁了解实现模式
- 游戏更新后运行`./scripts/decompile.sh`重新生成反编译代码

**禁止做法**:
- 永远不要对整个目录（反编译、提取资源或其他）使用`Read`工具
- 永远不要对完整的game_dll_api_documentation.md文件使用`Read`工具
- 不要试图将所有API文档加载到上下文
- 不要读取整个extracted_assets目录 - 使用Grep进行目标搜索
- 避免返回数千行的广泛搜索

## 最佳实践

### 代码组织
- **一个功能 = 一个文件/类**
- 补丁按**游戏系统分组**放在`Patches/`目录
- 共享工具放在`Utils/`目录
- 保持代码**模块化**和**松耦合**

### 日志规范
- 始终使用`ModLogger`而非直接使用`Debug.Log`
- 为不同组件使用带组件标识的日志: `ModLogger.Log("Inventory", "message")`
- 开发时用日志记录关键操作
- 生产代码中移除过于详细的调试日志

### Harmony补丁
- 补丁逻辑务必包裹在try-catch块中
- 使用描述性的补丁方法名
- 注释掉禁用/实验性补丁
- 按影响的游戏系统分组补丁
- 使用`[HarmonyPatch]`特性标记补丁

### 错误处理
- 所有补丁方法中捕获并记录异常
- 使用`ModLogger.LogError()`报告错误
- 优雅处理边缘情况，避免破坏游戏

## MOD框架参考

基于官方Escape from Duckov MOD框架:

### MOD加载流程
1. 游戏扫描`Duckov_Data/Mods`文件夹查找`info.ini`文件
2. 从`info.ini`读取`name`参数
3. 加载`{name}.dll`程序集
4. 创建GameObject并实例化`{namespace}.ModBehaviour`
5. 调用Unity生命周期方法（`Awake`, `Start`, `OnEnable`等）

### info.ini结构
```ini
name=EfDEnhanced                  # DLL名称（不含.dll）
displayName=EfD Enhanced          # 显示名称
description=描述文本              # MOD描述
publishedFileId=3590346461        # Steam创意工坊ID（可选）
```

### 发布所需文件
- `EfDEnhanced.dll` - 编译的MOD程序集
- `0Harmony.dll` - Harmony依赖（如果使用）
- `info.ini` - MOD配置
- `preview.png` - 256x256预览图（推荐）

## 重要注意事项

- **平台特定路径**: `.csproj`中的`DuckovDataPath`针对macOS。如果游戏安装在其他位置需要调整。
- **Unity引擎兼容性**: 此MOD针对Unity程序集。注意Unity的生命周期方法和组件系统。
- **可空引用类型**: 项目启用了可空引用类型。始终考虑可空性。
- **MOD加载**: 作为`ModBehaviour`子类，MOD会被游戏的MOD框架自动发现和加载。
- **Harmony补丁**: 在`Awake`中应用，可在`OnDestroy`中移除（可选）
- **日志位置**: macOS: `~/Library/Logs/TeamSoda/Duckov/Player.log`

## 故障排除

### MOD未加载
- 检查`info.ini`中的`name`是否与DLL文件名匹配
- 验证文件位于正确的MOD目录: `Duckov_Data/Mods/EfDEnhanced/`
- 检查Unity控制台是否有加载错误
- 运行`./scripts/rlog.sh`查看详细日志

### Harmony补丁不工作
- 使用`Grep`工具在API文档中验证目标方法是否存在
- 检查Unity控制台是否有异常
- 使用`ModLogger`在补丁方法中记录日志以验证执行
- 确保`[HarmonyPatch]`特性参数正确

### 构建错误
- 运行`dotnet restore`恢复依赖
- 验证游戏程序集路径（检查`.csproj`中的`DuckovDataPath`）
- 确保游戏已安装且路径可访问

## 开发工作流程

### 添加新补丁
1. 在`decompiled/`中搜索目标方法/类
2. 在`Patches/`中创建补丁文件（按系统命名）
3. 使用`[HarmonyPatch]`特性定义补丁
4. 补丁逻辑包裹在try-catch中
5. 使用`ModLogger`记录操作
6. 构建并测试

### 测试流程
1. 构建MOD: `dotnet build`
2. 部署到游戏: `./scripts/deploy.sh`
3. 启动游戏
4. 运行`./scripts/rlog.sh`监控日志
5. 在游戏中测试功能
6. 检查日志中的`[EfDEnhanced]`消息

### 发布流程
1. 构建Release版本: `dotnet build -c Release`
2. 从`output/`目录收集文件:
   - `EfDEnhanced.dll`
   - `0Harmony.dll`
   - `info.ini`
   - `preview.png`
3. 打包并分发

## 已实现功能

### 1. 移动增强系统 (Movement Enhancement System)

**功能**: 优化角色移动响应速度，消除"粘滞感"

**核心组件**:
- `MovementEnhancementPatch.cs` - ECM2移动系统增强补丁
- `ModSettings.cs` - 移动增强级别设置

**特性**:
- 4档位可调增强级别（禁用/轻度/中度/重度）
- 分别优化加速度、制动力和转向响应
- 仅影响玩家角色，不影响NPC
- 保持垂直速度（跳跃、下落）不受影响
- 实时切换无需重启游戏

**技术实现**:
- Patch ECM2.Movement.UpdateNormalMove方法
- 使用反射缓存提升性能
- 预设配置系统支持不同增强级别
- 智能检测玩家角色避免影响NPC
- 支持直接线性插值实现瞬时方向切换

---

### 2. 武器对比系统 (Weapon Comparison System)

**功能**: 在库存界面自动显示武器属性对比

**核心组件**:
- `ItemHoveringComparisonPatch.cs` - 物品悬停对比补丁
- `StatPolarityMap.cs` - 属性正负性映射表

**特性**:
- 支持枪支和近战武器对比
- 智能属性正负性判断（伤害高好，后坐力低好）
- 颜色编码显示差异（绿色更好，红色更差）
- 选中+悬停自动触发对比
- 可在设置中开关功能

**对比显示格式**:
- `[选中武器值] → [悬停武器值]`
- 绿色：悬停武器该属性更好
- 红色：选中武器该属性更好
- 白色：中性属性或数值相同

**技术实现**:
- Patch ItemPropertiesDisplay.Setup方法
- 使用ComparisonData缓存对比信息
- 动态修改TextMeshProUGUI显示文本和颜色
- 自动清理避免内存泄漏

---

### 3. 任务追踪系统 (Quest Tracking System)

**功能**: 在Raid中实时追踪活跃任务进度

**核心组件**:
- `ActiveQuestTracker.cs` - 任务追踪HUD显示
- `QuestTrackingManager.cs` - 任务追踪状态持久化
- `QuestViewDetailsPatch.cs` - 任务详情页面追踪复选框

**特性**:
- 任务详情页面添加"局内追踪"复选框
- 仅显示被追踪且未完成的任务
- 实时更新任务进度（任务完成数/总数）
- 显示每个子任务的完成状态（✓ 完成 / ○ 待完成）
- 菜单打开时自动隐藏，关闭时恢复显示
- 追踪选择持久化保存
- 可配置位置、缩放和是否显示任务描述

**技术实现**:
- 使用Unity Canvas + VerticalLayoutGroup构建UI
- 订阅游戏的任务系统事件（QuestManager.onQuestListsChanged等）
- 使用TrueShadow实现高质量文本阴影
- ContentSizeFitter自动调整UI高度
- 使用设置系统实时调整显示参数

---

### 4. Pre-Raid Check System (Raid前检查系统)

**功能**: 在玩家进入Raid地图前自动检查装备和天气条件

**核心组件**:
- `RaidCheckUtility.cs` - 检查逻辑实现
- `RaidPreparationView.cs` - 警告对话框UI
- `RaidEntryPatches.cs` - 拦截Raid进入点

**检查项目**:
1. 枪支 - 使用`item.GetBool("IsGun")`检查
2. 弹药 - 使用`item.GetBool("IsBullet")`检查
3. 药品 - 检查`UsageUtilities.behaviors`中的`Drug`组件
4. 食物 - 检查`UsageUtilities.behaviors`中的`FoodDrink`组件
5. 天气 - 检测当前是否为风暴天气（Stormy_I/Stormy_II）
6. 风暴预警 - 检测24小时内是否有风暴来临
7. 任务物品 - 根据目标地图检查活跃任务的`RequiredItemID`

**技术实现**:
- Patch `MapSelectionView.NotifyEntryClicked`方法
- 使用UniTask实现异步确认流程
- 智能场景过滤（只检查目标地图相关的任务）
- 失效保护设计（错误时不阻止进入）

---

### 5. 设置系统 (Settings System)

**功能**: 完整的游戏内设置界面和持久化

**核心组件**:
- `ModSettings.cs` - 集中式设置定义和管理
- `ModSettingsContent.cs` - 设置面板内容
- `Settings/` - 类型化设置条目（Bool/Int/Float/String/Options）
- `OptionsPanelPatch.cs` - 将设置标签页添加到游戏设置菜单
- `Utils/UI/` - 完整UI框架支持自动表单生成

**特性**:
- 自动从设置条目构建UI
- 分类组织（Raid检查 / 任务追踪）
- 类型化设置条目（Toggle/Slider/InputField/Dropdown）
- 实时设置变更事件
- ES3持久化保存
- 一键恢复默认设置

**设置项**:
- **Raid检查**: 启用检查系统、单独开关各项检查
- **移动增强**: 4档位移动响应性调节
- **界面增强**: 武器对比功能开关
- **任务追踪**: 启用追踪、调整位置/缩放、显示选项

**技术实现**:
- 使用泛型`SettingsEntry<T>`基类
- ValueChanged事件通知系统
- ES3序列化存储到游戏存档
- UIPanel继承和输入管理集成

---

### 6. 多语言支持 (Localization System)

**功能**: 完整的多语言本地化支持

**核心组件**:
- `LocalizationHelper.cs` - 本地化管理器

**支持语言**:
- 简体中文
- 繁体中文
- English
- 日本語

**技术实现**:
- 集成游戏的`LocalizationManager`系统
- 使用`SetOverrideText`覆盖本地化键
- 订阅`OnSetLanguage`事件自动切换语言
- 所有UI文本使用本地化键

---

### 7. Steam创意工坊集成 (Steam Workshop Integration)

**功能**: 防止上传时覆盖创意工坊描述

**核心组件**:
- `WorkshopUploadPatch.cs` - Patch SteamUGC方法

**特性**:
- 仅对EfDEnhanced生效（通过Workshop ID识别）
- 其他Mod不受影响
- 允许在创意工坊页面编写详细Markdown描述
- info.ini中保留简短描述用于游戏内显示

**技术实现**:
- Patch `SteamUGC.SetItemDescription`
- Patch `SteamWorkshopManager.UploadWorkshopItem`
- 通过publishedFileId识别Mod

---

## 获取帮助

- 查看`docs/`目录获取详细文档
- 检查现有补丁获取实现示例
- 使用`Grep`工具搜索反编译代码
- 运行`./scripts/rlog.sh`实时调试
