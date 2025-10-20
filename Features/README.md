# EfDEnhanced Mod Features

## 1. Active Quest Tracker (活跃任务追踪器)

### 功能概述

在Raid局内右上角显示你正在追踪的任务列表，实时查看任务进度和目标，无需打开任务面板。只有你主动勾选追踪的任务才会显示，避免信息过载。

### 使用方式

#### 第一步：在任务面板中勾选要追踪的任务

1. **打开任务面板**（按 Tab 键）
2. **点击任意任务**查看详情
3. 在任务标题下方，找到 **"局内追踪"** 复选框
4. **点击复选框**进行勾选
   - 未勾选：灰色方框 ☐
   - 已勾选：绿色勾选标记 ☑
5. 你可以同时追踪多个任务

#### 第二步：进入Raid查看追踪

1. **进入Raid**（通过传送台、楼梯等）
2. 被追踪的任务会自动显示在**屏幕右上角**
3. 实时显示任务进度和完成状态

### 任务显示内容

在Raid局内右上角会显示：
- **任务标题**（黄色高亮，加粗）
- **任务进度**（如 2/5，表示完成2个目标，共5个）
- **任务简介**（斜体）
- **各个子任务**及其完成状态：
  - ✓ 绿色勾选 = 已完成
  - ○ 白色圆圈 = 未完成

### 追踪数据保存

- **保存位置**：`%LocalLow%\TeamSoda\Duckov\EfDEnhanced\TrackedQuests.json`
- **自动保存**：勾选/取消追踪时立即保存到磁盘
- **持久化**：即使退出游戏，追踪设置也会保留
- **自动恢复**：下次启动游戏时自动加载

### UI设计

#### 任务面板中的追踪按钮
- **位置**：任务标题正下方
- **样式**：游戏原生的任务图标样式（与任务目标的复选框一致）
- **大小**：32x32 图标 + 20号字体
- **交互**：点击切换勾选状态

#### Raid局内显示
- **位置**：屏幕右上角
- **大小**：自适应任务数量，最大高度为屏幕40%
- **样式**：透明背景，带阴影文字
- **更新**：事件驱动,即时响应任务变化

### 性能优化

- **事件驱动更新**：订阅游戏内置的任务事件,即时响应变化,无需轮询
  - 任务列表变化事件（新任务、任务完成）
  - 任务状态变化事件
  - 子任务完成事件
  - 追踪状态变化事件（手动勾选/取消追踪）
- **UI元素复用**：避免每次更新都销毁重建UI，复用现有元素
- **差异更新**：只在内容实际变化时才更新UI组件
- **零GC压力**：更新过程中几乎不产生垃圾回收
- **精确更新**：只刷新实际变化的任务,不进行全量更新

---

## 2. Pre-Raid Check System (Raid前检查系统)

### 功能概述

在玩家通过传送台、楼梯或下水道进入Raid地图之前，自动检查玩家是否携带必需物资和检查天气条件。如果发现问题，会弹出警告对话框让玩家选择是否继续进入。

### 检查项目

1. **枪支检查** - 确保携带至少一把枪支
2. **弹药检查** - 确保背包中有弹药（包括额外弹匣/子弹）
3. **药品检查** - 确保携带医疗用品
4. **食物检查** - 确保携带食物或饮料
5. **天气检查** - 警告风暴天气（Stormy_I 和 Stormy_II）
6. **任务物品检查** - 检查是否携带了活跃任务所需的物品

### 使用方式

1. MOD会自动运行，无需额外配置
2. 当你尝试通过传送台进入Raid时：
   - 如果一切正常，直接进入
   - 如果缺少物资或天气不佳，会显示警告对话框
3. 在警告对话框中：
   - 按 **确认键（Enter/空格/手柄A）** 继续进入
   - 按 **取消键（ESC/手柄B）** 返回准备

### 输入管理

系统与游戏原生UI系统协同工作：

- **对话框显示时**：
  - 继承MapSelectionView的输入状态（已由View.OnOpen()禁用玩家输入）
  - 只响应UI输入（确认/取消）
  - 不会重复调用InputManager，避免输入状态混乱

- **对话框关闭后**：
  - 自动模拟取消按钮点击，让MapSelectionView正常退出异步流程
  - 输入状态由MapSelectionView在Close时自动恢复
  - 用户可以立即操作，无需额外按键

- **用户确认继续时**：
  - MOD设置绕过标志，用户再次点击地图时直接进入
  - 这样保证了流程的连贯性和用户体验

### 物品识别逻辑

使用游戏官方的物品识别API，确保准确性和兼容性：

#### 枪支识别
- 使用 `item.GetBool("IsGun")` - 游戏官方枪支标志
- 此标志由 `ItemSetting_Gun.SetMarkerParam()` 自动设置
- **精确识别**：只有真正的枪支才有此标志，配件和弹匣不会被误判

#### 弹药识别
- 使用 `item.GetBool("IsBullet")` - 游戏官方弹药标志
- 此标志由 `ItemSetting_Bullet.SetMarkerParam()` 自动设置
- **包含范围**：子弹、弹匣、弹药箱等所有弹药类物品

#### 药品识别
- 检查 `item.UsageUtilities.behaviors` 中是否包含 `Drug` 类型
- 游戏官方的医疗用品组件系统

#### 食物识别
- 检查 `item.UsageUtilities.behaviors` 中是否包含 `FoodDrink` 类型
- 游戏官方的食物饮料组件系统

#### 任务物品识别
- 从 `QuestManager.Instance.ActiveQuests` 获取所有活跃任务
- 检查每个任务的 `RequiredItemID` 和 `RequiredItemCount`（任务要求携带到 Raid 的物品）
- 使用 `ItemUtilities.GetItemCount(itemTypeID)` 检查物品数量
- **只检查活跃任务**：已完成或未激活的任务不会检查
- **全局搜索**：检查玩家背包、仓库等所有位置的物品
- **注意**：这不包括任务中需要交付的物品（SubmitItems），因为那些通常在 Raid 外交付

### 错误处理

系统设计为故障安全（Fail-Safe）：

- 所有检查逻辑都包含try-catch
- 如果检查过程出错，默认允许传送（避免阻止游戏）
- 如果对话框创建失败，不会阻止传送
- 所有错误都会记录到日志中

### 日志

查看日志以了解系统运行状态：

```bash
# macOS
tail -f ~/Library/Logs/TeamSoda/Duckov/Player.log

# Windows
Get-Content "$env:USERPROFILE\AppData\LocalLow\TeamSoda\Duckov\Player.log" -Wait

# 或使用项目脚本
./scripts/rlog.sh
```

日志示例：
```
[EfDEnhanced] [RaidCheck] Starting raid readiness check...
[EfDEnhanced] [RaidCheck] Found weapon: AK-47
[EfDEnhanced] [RaidCheck] Found ammo: 5.45x39 弹药
[EfDEnhanced] [RaidCheck] Found medicine: 急救包
[EfDEnhanced] [RaidCheck] Found food: 罐头
[EfDEnhanced] [RaidCheck] Current weather: Sunny, Safe: True
[EfDEnhanced] [QuestCheck] Found 2 active quests
[EfDEnhanced] [QuestCheck] ✓ Quest: 解救人质, Item: 钥匙卡, Required: 1, Current: 1
[EfDEnhanced] [QuestCheck] ✗ Quest: 破坏设施, Item: C4炸药, Required: 2, Current: 0
[EfDEnhanced] [QuestCheck] Total quest items to check: 2
[EfDEnhanced] [RaidCheck] Check result - Weapon: True, Ammo: True, Medicine: True, Food: True, Weather: True, QuestItems: 2
```

### 已知限制

1. 弹药检查不验证弹药类型是否与枪支匹配
2. 不检查弹药/物品的具体数量，只要有就算通过
3. 任务物品检查只检查 `Quest.RequiredItemID`（任务明确要求携带的物品）
4. 不检查任务的场景要求（即使物品充足，某些任务可能需要在特定地图完成）
5. 不检查任务中需要"找到并交付"的物品（SubmitItems），因为这些物品通常在 Raid 中获取后在基地交付

### 未来改进

- [ ] 添加可配置选项（允许用户自定义检查项）
- [ ] 检查弹药是否与武器匹配
- [ ] 添加最低物资数量要求
- [x] 支持多语言警告文本
- [ ] 添加装备推荐系统
- [ ] 检查任务的场景要求，只提示当前地图可完成的任务物品
- [ ] 可选支持检查 SubmitItems 类型任务（但需要用户配置，因为这些物品通常在 Raid 中获取）

---

## Active Quest Tracker (活跃任务追踪器)

### 功能概述

在 Raid 中实时显示进行中的任务信息，帮助玩家追踪任务目标和完成进度。追踪器显示在屏幕右上角，不会遮挡游戏主要视野。

### 主要特性

1. **实时任务显示** - 自动显示所有未完成的活跃任务
2. **任务进度追踪** - 显示每个任务的子任务完成状态
3. **自动更新** - 每秒自动刷新任务状态
4. **多语言支持** - 支持简体中文、繁体中文、英语、日语
5. **Raid 专属** - 只在 Raid 地图中显示，基地中自动隐藏

### UI 设计

- **位置**：屏幕右上角（距离边缘 10 像素）
- **尺寸**：360px 宽 x 屏幕高度 40%（动态适配分辨率）
- **背景**：完全透明，不遮挡游戏画面
- **层级**：Canvas sortingOrder = 100（在 HUD 之上）
- **文字阴影**：使用 TrueShadow 高质量渲染
- **布局**：垂直列表，无滚动条（所有任务直接显示）

### 任务显示内容

每个任务条目包含：

1. **标题行（水平布局）**
   - 左侧：任务标题（16px，金黄色，粗体）
   - 右侧：进度徽章（15px，灰色，粗体）
   - Space-between 布局，充分利用空间

2. **任务描述**
   - 字体大小：12px
   - 样式：斜体
   - 颜色：浅灰色
   - 支持自动换行

3. **子任务列表**
   - 已完成：✓ 绿色显示
   - 未完成：○ 白色显示
   - 显示任务描述和额外信息
   - 字体大小：13px

4. **分隔线**
   - 每个任务底部有淡色分隔线
   - 无背景色，完全透明

### 工作原理

#### 自动启用/禁用

```csharp
// ModBehaviour 每帧检查玩家状态
void Update()
{
    if (LevelManager.Instance == null) return;
    
    bool isInRaid = LevelManager.Instance.IsRaidMap;
    
    if (isInRaid && !_wasInRaid)
    {
        // 进入 Raid - 启用追踪器
        _questTracker?.Enable();
    }
    else if (!isInRaid && _wasInRaid)
    {
        // 离开 Raid - 禁用追踪器
        _questTracker?.Disable();
    }
}
```

#### 任务数据源

从游戏官方 API 获取：

```csharp
var activeQuests = QuestManager.Instance.ActiveQuests;
var incompleteQuests = activeQuests.Where(q => !q.Complete);
```

#### 更新机制

- **更新频率**：每 1 秒刷新一次
- **性能优化**：只在任务列表变化时重建 UI
- **智能清理**：离开 Raid 时自动清理所有 UI 元素

### 本地化支持

| 键名 | 简体中文 | 繁体中文 | English | 日本語 |
|-----|---------|---------|---------|--------|
| `QuestTracker_Title` | 活跃任务 | 活躍任務 | Active Quests | アクティブクエスト |
| `QuestTracker_Progress` | 进度: {0}/{1} | 進度: {0}/{1} | Progress: {0}/{1} | 進行状況: {0}/{1} |
| `QuestTracker_NoQuests` | 无进行中的任务 | 無進行中的任務 | No active quests | 進行中のクエストなし |

### 使用示例

1. **接取任务**：在基地从 NPC 接取任务
2. **进入 Raid**：通过传送台/楼梯进入 Raid 地图
3. **查看追踪器**：右上角自动显示任务列表
4. **完成任务**：任务进度实时更新，完成的子任务显示 ✓
5. **离开 Raid**：回到基地后追踪器自动隐藏

### 日志示例

```
[EfDEnhanced] [ModBehaviour] Quest tracker initialized
[EfDEnhanced] [QuestTracker] Quest tracker created successfully
[EfDEnhanced] [QuestTracker] UI built successfully
[EfDEnhanced] [ModBehaviour] Entered raid - quest tracker enabled
[EfDEnhanced] [QuestTracker] Tracker enabled
[EfDEnhanced] [QuestTracker] Created entry for quest: 营救人质
[EfDEnhanced] [QuestTracker] Created entry for quest: 破坏敌军设施
[EfDEnhanced] [ModBehaviour] Left raid - quest tracker disabled
[EfDEnhanced] [QuestTracker] Tracker disabled
```

### 技术细节

#### UI 组件层次结构

```
QuestTrackerCanvas (Canvas, Screen Space Overlay)
└── QuestPanel (360 x 40%屏幕高度, 右上角)
    └── QuestListContainer (VerticalLayoutGroup)
        ├── QuestEntry_1 (透明 + 底部边框)
        │   ├── TitleRow (HorizontalLayoutGroup)
        │   │   ├── QuestTitle (16px, 金黄色) ← flex:1
        │   │   └── ProgressBadge (15px, "2/3") ← min:40px
        │   ├── QuestDescription (12px, 斜体)
        │   └── ProgressContainer (VerticalLayoutGroup)
        │       ├── Task_1 (13px, "○ 未完成任务" + TrueShadow)
        │       └── Task_2 (13px, "✓ 已完成任务" + TrueShadow)
        └── QuestEntry_2 (...)
```

#### 关键类

- **`ActiveQuestTracker`** - 主追踪器类，管理 UI 生命周期
- **`QuestEntryUI`** - 单个任务条目的 UI 数据和更新逻辑
- **`LocalizationHelper`** - 提供多语言支持

### 错误处理

- 所有方法都包含 try-catch 错误处理
- 出错时记录详细日志但不影响游戏运行
- QuestManager 不存在时优雅降级
- UI 构建失败时不会崩溃

### 性能考虑

1. **DontDestroyOnLoad** - 追踪器在场景切换时保持存在
2. **智能更新** - 重用 UI 元素，只在任务变化时重建
3. **延迟更新** - 1 秒更新间隔避免过度刷新
4. **动态适配** - 使用 `Screen.height` 动态计算UI尺寸
5. **高质量渲染** - TrueShadow 提供专业级阴影效果

### 已知限制

1. 不显示已完成但未交付的任务
2. 不显示任务奖励信息
3. 任务过多时会超出屏幕高度（显示前几个任务）
4. 不支持点击任务跳转到地图标记（未来功能）
5. 不支持折叠/展开单个任务

### 未来改进

- [ ] 添加滚动条支持显示更多任务
- [ ] 添加任务分类筛选（主线/支线/日常）
- [ ] 支持折叠/展开单个任务
- [ ] 添加任务地点指示（地图标记）
- [ ] 支持自定义 UI 位置、大小和透明度
- [ ] 添加完成任务的音效和动画效果
- [ ] 支持快捷键切换显示/隐藏
- [ ] 添加任务优先级标记

