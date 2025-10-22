# 投掷物轮盘菜单使用指南

## 功能简介

投掷物轮盘菜单是一个专门用于快速切换投掷物（手雷等）的径向菜单，可以让你在战斗中快速选择和装备投掷物。

## 使用方法

### 打开和使用
1. **按住** 配置的热键（默认 **`G` 键**，可在设置中修改）打开菜单
2. **保持按住** 热键的同时，移动鼠标选择投掷物
3. **释放** 热键，自动装备当前选中的投掷物

> 💡 **提示**：这是一个"按住-选择-释放"的操作流程，类似于 GTA5 的武器轮盘

### 选择投掷物
- 按住热键后，移动鼠标指向你想要使用的投掷物
- 鼠标移出中心死区后，会高亮显示当前指向的投掷物扇区
- 高亮的扇区会放大并变亮，表示已选中

### 取消操作
- 游戏暂停时会自动取消菜单
- 打开其他界面（背包、地图等）时会自动取消菜单
- 在死区内释放热键不会触发任何物品

## 界面说明

投掷物轮盘菜单显示你背包中所有的投掷物（手雷等技能物品）:

```
       [手雷A]
    [手雷B]   [闪光弹]
         ●
    [烟雾弹]   [燃烧弹]
        [...]
```

- 中心的 **●** 是死区，鼠标在这里不会选中任何物品
- 每个扇区显示一种投掷物的图标
- 右下角显示该投掷物的总数量（叠加计算）
- 相同类型的投掷物会自动堆叠显示

## 功能特性

✅ **实时同步**：显示当前背包中实际的投掷物  
✅ **视觉反馈**：悬停时扇区高亮，清晰指示当前选择  
✅ **智能关闭**：打开背包、地图等界面时自动取消  
✅ **快速操作**：单手操作，无需离开游戏视角  
✅ **输入隔离**：菜单打开时自动屏蔽射击、移动、视角旋转等输入，防止误操作  
✅ **可自定义**：支持在设置中修改热键和菜单缩放  
✅ **自动堆叠**：相同类型的投掷物自动合并显示，显示总数量  

## 注意事项

1. **投掷物类型检测**：
   - ✅ 检测所有技能类型物品（`IsSkill` = true）
   - ✅ 必须是投掷技能（`Skill_Grenade` 类型）
   - ❌ 非投掷技能不会显示

2. **死区设计**：中心有一个小死区
   - 鼠标在死区内不会选中任何物品
   - 在死区内释放热键不会触发任何操作
   - 可以安全地移动鼠标而不误触

3. **输入屏蔽**：菜单打开时会自动屏蔽以下操作
   - ❌ 鼠标移动影响视角（镜头旋转）
   - ❌ 射击/开火输入
   - ✅ 键盘移动仍然可用（WASD）

4. **与其他界面的交互**：
   - 如果有其他 UI 窗口打开（背包、地图、暂停菜单等），轮盘菜单无法打开
   - 菜单打开时如果游戏暂停或打开其他界面，会自动取消菜单

5. **物品堆叠**：
   - 相同 TypeID 的投掷物会自动堆叠
   - 显示总数量（包括所有相同类型物品的 StackCount 总和）
   - 使用时会优先使用列表中的第一个物品

## 设置选项

可以在游戏设置界面中调整以下参数：

| 设置项 | 默认值 | 说明 |
|--------|--------|------|
| 投掷物轮盘启用 | ✓ 启用 | 是否启用投掷物轮盘菜单功能 |
| 投掷物轮盘热键 | `G` (KeyCode.G) | 打开轮盘菜单的快捷键，可自定义 |
| 轮盘缩放 | 1.0 | 菜单的整体大小，范围 0.5 - 2.0（与物品轮盘共用） |

## 技术实现

### 核心组件
- **`PieMenuComponent`** ([Utils/UI/Components/PieMenuComponent.cs](mdc:Utils/UI/Components/PieMenuComponent.cs)) - 通用饼状菜单组件
- **`ThrowableWheelMenu`** ([Features/ThrowableWheelMenu.cs](mdc:Features/ThrowableWheelMenu.cs)) - 投掷物轮盘功能实现
- **`ThrowableWheelMenuPatch`** ([Patches/ThrowableWheelMenuPatch.cs](mdc:Patches/ThrowableWheelMenuPatch.cs)) - Harmony 补丁，处理输入和集成

### 工作流程
1. 用户按下配置的热键（默认 G 键）
2. `ThrowableWheelMenuPatch` 检测按键事件，调用 `ThrowableWheelMenu.Show()`
3. `ThrowableWheelMenu` 扫描角色背包获取所有投掷物数据
   - 使用 `IsThrowableItem()` 检测投掷物（`IsSkill` + `Skill_Grenade`）
   - 按 TypeID 自动堆叠相同类型的投掷物
   - 计算总数量（所有 StackCount 之和）
4. `PieMenuComponent` 渲染饼状菜单并处理鼠标悬停
5. 用户释放热键时，装备选中的投掷物（调用 `Character.ChangeHoldItem()`）
6. 补丁屏蔽游戏的射击、视角旋转输入，防止误操作

### 投掷物检测逻辑
```csharp
private bool IsThrowableItem(Item item)
{
    // 1. 检查是否为技能物品
    if (!item.GetBool("IsSkill")) return false;
    
    // 2. 获取技能设置组件
    ItemSetting_Skill skillSetting = item.GetComponent<ItemSetting_Skill>();
    if (skillSetting == null) return false;
    
    // 3. 检查是否为投掷技能
    return skillSetting.Skill is Skill_Grenade;
}
```

### 投掷物堆叠逻辑
```csharp
// 按 TypeID 分组堆叠
Dictionary<int, ThrowableStack> stacksByTypeID;

// ThrowableStack 结构
class ThrowableStack {
    string TypeID;              // 物品类型ID
    List<Item> Items;           // 该类型的所有物品实例
    Sprite Icon;                // 物品图标
    string DisplayName;         // 显示名称
    int TotalCount;             // 总数量（所有StackCount之和）
}
```

### 输入处理
补丁会拦截以下游戏输入方法：
- `CharacterInputControl.OnPlayerMouseDelta` - 屏蔽视角旋转
- `CharacterInputControl.OnPlayerMouseMove` - 屏蔽瞄准移动
- `CharacterInputControl.OnPlayerTriggerInputUsingMouseKeyboard` - 屏蔽射击输入

### 自动取消机制
通过事件监听自动取消菜单：
- `Duckov.UI.View.OnActiveViewChanged` - 检测界面切换
- `PauseMenu.Show` - 检测游戏暂停
- `GameManager.Paused` - 轮询检测暂停状态

### 输入状态清理
当菜单打开时，会清理积累的输入状态，防止鼠标移动影响镜头：
```csharp
// 清理 mouseDelta 字段（防止镜头突然旋转）
mouseDeltaField.SetValue(inputControl, Vector2.zero);

// 重置 mousePos 字段到当前鼠标位置
mousePosField.SetValue(inputControl, currentMousePosition);
```

## 与物品轮盘的区别

| 特性 | 物品轮盘 | 投掷物轮盘 |
|------|---------|-----------|
| 默认热键 | `~` (Backquote) | `G` (KeyCode.G) |
| 物品来源 | 快捷栏物品（1-6键绑定） | 背包中所有投掷物 |
| 物品类型 | 消耗品、技能、手持物品 | 仅投掷物（Skill_Grenade） |
| 物品数量 | 固定6个位置 | 动态（有多少显示多少） |
| 堆叠行为 | 不堆叠 | 自动按TypeID堆叠 |
| 数量显示 | 单个物品StackCount | 总StackCount（所有同类型之和） |
| 使用行为 | 使用/装备物品 | 装备投掷物（ChangeHoldItem） |
| 菜单缩放 | 共用设置 | 共用设置 |

## 故障排除

### 轮盘菜单打不开？
1. 检查设置中"投掷物轮盘启用"是否开启
2. 确认是否有其他 UI 界面已经打开（背包、地图等）
3. 确认游戏没有暂停
4. 检查热键是否被其他程序占用
5. 查看日志文件：运行 `scripts/rlog.sh` 或查看游戏日志

### 投掷物不显示？
1. 确认背包中确实有投掷物（手雷等技能物品）
2. 检查物品是否为 `Skill_Grenade` 类型
3. 确认物品有 `IsSkill` 属性且为 true
4. 查看日志了解具体检测结果

### 释放按键没反应？
1. 确保鼠标移出了中心死区（否则不会选中任何物品）
2. 确认扇区已经高亮显示（表示已选中）
3. 检查角色是否可以切换物品（可能在其他动作中）
4. 查看日志了解具体错误信息

### 装备投掷物后没有效果？
1. 确认角色装备栏已经切换到投掷物
2. 检查是否需要再次按使用键（游戏原生操作）
3. 查看日志中的 `ChangeHoldItem` 调用结果

---

## 反馈和建议

如果你在使用过程中遇到问题，或有改进建议，可以：
1. 查看模组日志了解详细错误信息
2. 在项目 GitHub 页面提交 Issue
3. 提供复现步骤和日志文件以便排查

祝游戏愉快！ 🎮

