# Raid Ready Check Features

## Pre-Raid Check System (Raid前检查系统)

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
- [ ] 支持多语言警告文本
- [ ] 添加装备推荐系统
- [ ] 检查任务的场景要求，只提示当前地图可完成的任务物品
- [ ] 可选支持检查 SubmitItems 类型任务（但需要用户配置，因为这些物品通常在 Raid 中获取）

