# 物品调试辅助补丁

## 功能说明

`ItemSelectionDebugPatch` 是一个调试辅助补丁，用于在玩家选中物品时自动输出该物品的详细信息到日志中。

## 用途

这个补丁主要用于：
- 🔍 **调试物品系统** - 了解物品的内部数据结构
- 📊 **查看物品属性** - 查看完整的物品统计数据
- 🧪 **测试物品配置** - 验证物品配置是否正确
- 📝 **记录物品信息** - 为模组开发收集物品数据

## 使用方法

1. **构建并部署模组**
   ```bash
   dotnet build -c Release
   ```

2. **启动游戏并查看日志**
   ```bash
   ./scripts/rlog.sh
   ```

3. **在游戏中选中物品**
   - 在背包界面中点击任意物品
   - 查看控制台输出的详细信息

## 输出信息包括

### 基础信息
- 显示名称（Display Name）- 本地化后的名称
- 原始名称键（Display Name Raw Key）- 本地化键名
- 描述（Description）- 本地化后的描述
- 原始描述键（Description Raw Key）- 本地化键名
- 类型ID（Type ID）- 物品的唯一标识符
- 排序（Order）- 物品排序值
- 实例ID（Instance ID）- Unity实例ID
- 声音键（Sound Key）- 物品声音标识

### 价值与品质
- 价值（Value）
- 品质（Quality）
- 显示品质（Display Quality）
- 总原始价值（Total Raw Value）

### 重量
- 单位自重（Unit Self Weight）
- 自重（Self Weight）
- 总重量（Total Weight）

### 堆叠信息
- 是否可堆叠（Stackable）
- 最大堆叠数量（Max Stack Count）
- 当前堆叠数量（Current Stack Count）

### 耐久度（如果物品使用耐久度）
- 当前耐久度（Current Durability）
- 最大耐久度（Max Durability）
- 耐久度损失（Durability Loss）
- 损失后的最大耐久度（Max Durability With Loss）
- 是否可修理（Repairable）

### 标志
- 是否可出售（Can Be Sold）
- 是否可丢弃（Can Drop）
- 是否粘性（Sticky）
- 是否为角色（Is Character）
- 是否有手持代理（Has Hand Held Agent）
- 是否可使用（Is Usable）
- 使用时间（Use Time）
- 已检查（Inspected）
- 正在检查（Inspecting）
- 需要检查（Need Inspection）

### 标签（Tags）
所有附加到物品上的标签：
- **原始标签名**（tag.name）
- 显示名称（仅当与原始名不同时显示）

### 属性（Stats）
所有物品属性，以原始key为主：
- 显示状态（[Displayed] 或 [Hidden]）
- **原始键名**（stat.Key）- 如 "Damage", "FireRate"
- **原始数值**（stat.Value）- 精确到小数点后2位
- 显示名称（仅当与键名不同时显示）

### 插槽（Slots）
物品上的所有插槽：
- **原始键名**（slot.Key）
- 插槽内容及其TypeID
- 显示名称（仅当与键名不同时显示）

### 修饰符（Modifiers）
所有修饰符以原始数据为主：
- **原始键名**（modifier.Key）
- **修饰符类型**（Type）- Add, PercentageAdd, PercentageMultiply
- **原始数值**（Value）- 精确浮点数
- 显示值字符串（格式化后的值）
- Order和Override信息
- 显示名称（仅当与键名不同时显示）

### 变量（Variables）
所有自定义变量以原始值为主：
- **原始键名**（variable.Key）
- **数据类型**（DataType）- Float, Int, Bool, String
- **原始值**：
  - Float: 精确到小数点后6位
  - Int: 整数值
  - Bool: True/False
  - String: 带引号的字符串
- 显示值（仅当variable.Display为true且与键名不同时显示）

### 常量（Constants）
所有常量以原始值为主：
- **原始键名**（constant.Key）
- **数据类型**（DataType）
- **原始值**（格式同变量）
- 显示值（仅当constant.Display为true且与键名不同时显示）

### 效果（Effects）
所有效果组件及其激活状态

### 背包（Inventory）
如果物品有背包：
- 容量
- 物品数量
- 总重量

### 层级关系（Hierarchy）
- 父物品
- 所在背包
- 插入的插槽

## 日志示例

```
========== ITEM DETAILS ==========
Display Name: AK-74M
Display Name (Raw Key): Item_AK74M
Description: 俄制5.45x39mm突击步枪
Description (Raw Key): Item_AK74M_Desc
Type ID: 10001
Order: 100
Instance ID: 12345
Sound Key: rifle

--- Value & Quality ---
Value: 25000
Quality: 80
Display Quality: Common
Total Raw Value: 28000

--- Weight ---
Unit Self Weight: 3.400000
Self Weight: 3.400000
Total Weight: 4.200000

--- Stats (15) ---
  [Displayed] Damage: 45.00
    Display Name: 伤害
  [Displayed] FireRate: 650.00
    Display Name: 射速
  [Displayed] Recoil: 85.00
    Display Name: 后坐力
  [Hidden] BaseAccuracy: 0.95
  ...

--- Tags (3) ---
  - Weapon
    Display: 武器
  - Gun
    Display: 枪支
  - Rifle
    Display: 步枪

--- Slots (5) ---
  - Magazine: <empty>
    Display Name: 弹匣
  - Sight: Red Dot Sight (TypeID: 20001)
    Display Name: 瞄具
  ...

--- Modifiers (2) ---
  - Key: Accuracy
    Type: PercentageAdd, Value: 0.15 (+15%)
    Order: 200, Override: False
    Display Name: 精准度
  - Key: Recoil
    Type: Add, Value: -5.00 (-5.00)
    Order: 100, Override: False
    Display Name: 后坐力

--- Variables (3) ---
  - Count (Int): 1
  - Durability (Float): 87.500000
    Display: 87.5 (耐久度)
  - Inspected (Bool): True

--- Constants (2) ---
  - MaxDurability (Float): 100.000000
    Display: 100 (最大耐久度)
  - CaliberKey (String): "5.45x39"
==================================
```

## 注意事项

⚠️ **性能影响**：此补丁会在每次选中物品时输出大量日志，建议仅在开发调试时使用。

⚠️ **日志文件大小**：长时间使用会产生大量日志，注意清理日志文件。

## 禁用方法

如果不需要此功能，有两种方式禁用：

### 方法1：删除补丁文件
删除 `Patches/ItemSelectionDebugPatch.cs` 文件后重新编译

### 方法2：注释补丁类
在类声明前添加注释：
```csharp
// [HarmonyPatch]
public class ItemSelectionDebugPatch
```

## 相关文件

- 补丁实现：`Patches/ItemSelectionDebugPatch.cs`
- 物品类定义：`extracted_assets/Scripts/ItemStatsSystem/ItemStatsSystem/Item.cs`
- UI工具类：`extracted_assets/Scripts/TeamSoda.Duckov.Core/Duckov/UI/ItemUIUtilities.cs`

