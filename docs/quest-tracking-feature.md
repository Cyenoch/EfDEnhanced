# Quest Tracking Feature Implementation

## 概述

为 EfDEnhanced Mod 添加了任务追踪功能，允许玩家在主菜单任务面板中选择要追踪的任务，然后在 Raid 局内右上角显示这些任务的进度。

## 架构设计

### 核心组件

1. **QuestTrackingManager** (`Utils/QuestTrackingManager.cs`)
   - 管理用户追踪的任务ID列表
   - 持久化保存到磁盘（使用 Unity JsonUtility）
   - 提供追踪状态的查询和修改接口
   - 触发追踪状态变化事件

2. **QuestViewDetailsPatch** (`Patches/QuestViewDetailsPatch.cs`)
   - Harmony Patch，注入到游戏的 `QuestViewDetails.Setup()` 方法
   - 在任务详情面板顶部添加"局内追踪"复选框
   - 监听复选框变化，更新追踪状态
   - 使用 Unity UI 组件（Toggle, TextMeshProUGUI）

3. **ActiveQuestTracker** (`Features/ActiveQuestTracker.cs`)
   - 已有组件，新增过滤逻辑
   - 只显示被用户追踪的任务（而非所有活跃任务）
   - 监听追踪状态变化事件，实时更新UI

### 数据流

```
用户勾选复选框
    ↓
QuestViewDetailsPatch.OnTrackToggleChanged()
    ↓
QuestTrackingManager.SetQuestTracked(questId, true)
    ↓
保存到磁盘 + 触发 OnTrackingChanged 事件
    ↓
ActiveQuestTracker.OnQuestTrackingChanged()
    ↓
RefreshQuestList() - 只显示被追踪的任务
```

## 实现细节

### 1. 追踪状态管理

**存储位置**：
```
%AppData%\..\LocalLow\TeamSoda\Duckov\EfDEnhanced\TrackedQuests.json
```

**数据格式**（Unity JsonUtility）：
```json
{
  "TrackedQuestIds": [1001, 1002, 1005]
}
```

**关键方法**：
- `Initialize()` - 启动时从磁盘加载
- `IsQuestTracked(int questId)` - 查询追踪状态
- `SetQuestTracked(int questId, bool tracked)` - 设置追踪状态
- `OnTrackingChanged` - 状态变化事件

### 2. UI注入（Harmony Patch）

**Patch目标**：`Duckov.Quests.UI.QuestViewDetails.Setup(Quest quest)`

**注入位置**：Postfix（方法执行后）

**UI结构**：
```
QuestViewDetails
  └─ Content (父容器)
       ├─ Text (TMP) - 任务标题 displayName
       ├─ TrackQuestButton (新增，显示在标题下方)
       │    ├─ StatusIcon (Image，32x32)
       │    │    ├─ Background (灰色/绿色方框)
       │    │    └─ Checkmark (绿色内填充，勾选时显示)
       │    └─ Label (TextMeshProUGUI - "局内追踪"，20号字体)
       └─ Description - 任务描述
```

**布局参数**：
- 按钮容器：
  - `anchorMin/Max`: (0, 1) - 顶部锚点
  - `sizeDelta`: (0, 40) - 高度40px
  - `SetSiblingIndex(displayNameIndex + 1)` - 显示在标题正下方
- 图标：
  - 尺寸：32x32 像素
  - 使用 LayoutElement 锁定大小，防止布局组拉伸
- 文字：
  - 字号：20
  - 颜色：白色
  - 与图标间距：12px

**关键改进**：
- 使用 Button 而非 Toggle，避免复杂的 Toggle 状态管理
- 通过反射获取游戏原生的 `TaskEntry.unsatisfiedIcon` 和 `satisfiedIcon`
- 使用 Image.sprite 切换来显示勾选状态，而非显示/隐藏子对象
- 为每个 QuestViewDetails 实例（QuestView 和 QuestGiverView）维护独立的按钮

### 3. 任务过滤逻辑

**修改位置**：`ActiveQuestTracker.RefreshQuestList()`

**原逻辑**：
```csharp
var incompleteQuests = activeQuests
    .Where(q => q != null && !q.Complete)
    .ToList();
```

**新逻辑**：
```csharp
var trackedIncompleteQuests = activeQuests
    .Where(q => q != null && !q.Complete && QuestTrackingManager.IsQuestTracked(q.ID))
    .ToList();
```

### 4. 实时更新

**事件订阅**：
- `Enable()` 时订阅 `QuestTrackingManager.OnTrackingChanged`
- `Disable()` 和 `OnDestroy()` 时取消订阅

**回调处理**：
```csharp
private void OnQuestTrackingChanged(int questId, bool isTracked)
{
    if (!_isActive) return;
    RefreshQuestList(); // 重新筛选并刷新显示
}
```

**UI按钮点击处理**：
```csharp
private static void OnTrackButtonClicked(QuestViewDetails instance)
{
    bool currentlyTracked = QuestTrackingManager.IsQuestTracked(questId);
    QuestTrackingManager.SetQuestTracked(questId, !currentlyTracked);
    UpdateButtonState(instance, quest); // 立即更新UI
}
```

## 技术要点

### 1. Unity JsonUtility 与 Newtonsoft.Json 的区别

Unity 的 `JsonUtility` 有限制：
- ✅ 支持：字段（field）、基本类型、List<T>
- ❌ 不支持：属性（property with getter/setter）、Dictionary、复杂嵌套

**解决方案**：将 SaveData 的属性改为字段
```csharp
// 错误 ❌
public List<int> TrackedQuestIds { get; set; }

// 正确 ✅
public List<int> TrackedQuestIds = new List<int>();
```

### 2. 通过反射获取游戏原生资源

获取 TaskEntry 的图标 Sprites：
```csharp
var taskEntryPrefabField = typeof(QuestViewDetails).GetField("taskEntryPrefab", 
    BindingFlags.NonPublic | BindingFlags.Instance);
var taskEntryPrefab = taskEntryPrefabField.GetValue(questViewDetails) as TaskEntry;

var unsatisfiedField = typeof(TaskEntry).GetField("unsatisfiedIcon", 
    BindingFlags.NonPublic | BindingFlags.Instance);
Sprite uncheckedSprite = unsatisfiedField.GetValue(taskEntryPrefab) as Sprite;
```

**优点**：
- 与游戏UI风格完全一致
- 支持游戏的主题切换和本地化
- 无需自己绘制图标

### 3. UI 布局问题解决

**问题**：Toggle 被父级的 LayoutGroup 拉伸变形

**解决方案**：
1. 使用 LayoutElement 锁定图标大小
2. 设置 `childForceExpandHeight = false`
3. 设置 `childControlHeight = false`

```csharp
LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
iconLayout.minWidth = 32;
iconLayout.minHeight = 32;
iconLayout.preferredWidth = 32;
iconLayout.preferredHeight = 32;
iconLayout.flexibleWidth = 0;
iconLayout.flexibleHeight = 0;
```

### 4. 多实例管理

**问题**：游戏中有多个 QuestViewDetails 实例（QuestView 和 QuestGiverView）

**解决方案**：使用 Dictionary 为每个实例维护独立的 UI
```csharp
private static Dictionary<QuestViewDetails, ButtonData> _trackButtons = 
    new Dictionary<QuestViewDetails, ButtonData>();
```

## 测试要点

1. **追踪状态持久化**
   - 勾选任务 → 退出游戏 → 重新启动 → 检查是否仍被追踪

2. **UI显示过滤**
   - 只追踪部分任务 → 进入 Raid → 确认只显示被追踪的任务

3. **实时更新**
   - 在主菜单勾选/取消追踪 → 立即进入 Raid → 检查UI是否正确

4. **多任务追踪**
   - 追踪多个任务 → 确认右上角显示所有被追踪任务

5. **任务完成处理**
   - 追踪任务完成后 → 确认从右上角消失

6. **UI布局**
   - 检查复选框位置是否在任务详情顶部
   - 检查右上角任务列表是否正确排列

## 文件清单

- ✅ `Utils/QuestTrackingManager.cs` - 追踪管理器（新文件）
- ✅ `Patches/QuestViewDetailsPatch.cs` - UI注入补丁（新文件）
- ✅ `Features/ActiveQuestTracker.cs` - 任务显示器（修改）
- ✅ `ModBehaviour.cs` - 初始化追踪管理器（修改）
- ✅ `Features/README.md` - 功能文档（更新）
- ✅ `README.md` - 项目说明（已包含新功能）

## 实现亮点

1. ✅ **使用游戏原生图标**
   - 通过反射获取 TaskEntry 的勾选框图标
   - 与游戏UI风格完美融合

2. ✅ **多实例支持**
   - 正确处理 QuestView 和 QuestGiverView 两个界面
   - 每个实例独立管理UI元素

3. ✅ **防止布局拉伸**
   - 使用 LayoutElement 锁定图标尺寸
   - 避免被父级 LayoutGroup 影响

4. ✅ **简单可靠的交互**
   - 使用 Button 而非 Toggle
   - 直接切换图标sprite而非复杂的状态管理

## 已知限制

1. **布局依赖游戏内部结构**
   - 如果游戏更新改变了 QuestViewDetails 的布局，可能需要调整

2. **不支持任务排序**
   - 局内显示顺序与 `QuestManager.ActiveQuests` 一致

3. **图标回退方案**
   - 如果无法获取游戏原生图标，会使用简单的方块作为替代

## 未来改进方向

1. **功能增强**
   - 支持拖拽排序追踪的任务
   - 追踪任务数量限制（如最多5个）
   - 在任务列表（QuestEntry）中显示追踪图标

2. **UI优化**
   - 添加勾选/取消的动画效果
   - 按钮悬停时的高亮效果

3. **便捷功能**
   - 快捷键快速追踪/取消当前选中任务
   - 批量追踪同一NPC的所有任务

## 相关游戏类

- `Duckov.Quests.QuestManager` - 任务管理器
- `Duckov.Quests.Quest` - 任务数据
- `Duckov.Quests.Task` - 任务目标
- `Duckov.Quests.UI.QuestView` - 任务主面板
- `Duckov.Quests.UI.QuestViewDetails` - 任务详情面板
- `Duckov.Quests.UI.QuestEntry` - 任务列表项
- `Duckov.Quests.UI.TaskEntry` - 任务目标列表项

