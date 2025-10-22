# 武器比较功能

## 功能说明

**在库存界面中，当选中一把武器后，鼠标悬停在另一把武器上时，会自动显示两把武器的属性对比。**

**支持远程武器（枪支）和近战武器的对比。**

### 特性

- **自动对比**: 选中武器 + 悬停武器 = 自动显示属性差异
- **颜色标识**: 
  - 🟢 绿色：更好的属性值
  - 🔴 红色：更差的属性值
  - ⚪ 白色：中性属性或相同值
- **智能判断**: 自动识别属性的正向性（如伤害越高越好）和负向性（如后坐力越低越好）
- **可配置**: 可以在设置面板中开关，默认启用

### 使用方法

1. 打开库存或装备界面
2. **左键点击**选中一把武器
3. **鼠标悬停**到另一把武器上
4. 查看属性面板中的对比信息

对比信息格式：`[选中武器值] → [悬停武器值]`

### 示例

**枪支对比：**
```
伤害:     45 → 52      (绿色，悬停武器伤害更高)
射速:     600 → 550    (红色，悬停武器射速更低)
后坐力V:  8.5 → 6.2    (绿色，悬停武器后坐力更低，更好)
后坐力H:  4.2 → 4.2    (白色，相同值)
```

**近战武器对比：**
```
伤害:        50 → 65      (绿色，悬停武器伤害更高)
攻击速度:    1.2 → 1.5    (绿色，悬停武器攻击更快)
攻击距离:    2.0 → 2.5    (绿色，悬停武器攻击范围更远)
重量:        3.5 → 2.8    (绿色，悬停武器更轻)
耐久度:      100 → 80     (红色，悬停武器耐久度更低)
```

## 设置选项

在暂停菜单 > EfD Enhanced 设置 > 界面增强：

- **启用武器对比**: 开关此功能（默认：开启）

## 技术实现

### 核心文件

- `Patches/ItemHoveringComparisonPatch.cs` - 主要实现逻辑
- `Utils/StatPolarityMap.cs` - 属性极性映射（正向/负向/中性）
- `Utils/ModSettings.cs` - 设置管理

### 工作原理

1. **检测**: Harmony patch 拦截 `ItemPropertiesDisplay.Setup`
2. **提取**: 从选中武器和悬停武器提取所有可显示属性
3. **匹配**: 按属性键名匹配两把武器的对应属性
4. **对比**: 计算数值差异，根据极性判断优劣
5. **显示**: 修改 UI 文本，添加对比值和颜色标签

### 属性极性

定义在 `StatPolarityMap.cs` 中：

**远程武器属性：**
```csharp
// 正向属性（越高越好）
Damage, ShootSpeed, BulletSpeed, BulletDistance, CritRate, 
ArmorPiercing, Capacity, MoveSpeedMultiplier

// 负向属性（越低越好）  
DefaultScatter, MaxScatter, ScatterGrow, RecoilVMin, RecoilVMax,
RecoilHMin, RecoilHMax, ADSTime, ReloadTime, SoundRange

// 中性属性（不影响判断）
其他所有属性
```

**近战武器属性：**
```csharp
// 正向属性（越高越好）
Damage, AttackSpeed, AttackRange, SwingSpeed, StabSpeed,
HeavyAttackDamage, LightAttackDamage, Durability, 
BlockEfficiency, ParryWindow

// 负向属性（越低越好）
Weight, StaminaCost, RecoveryTime

// 中性属性（不影响判断）
其他所有属性
```

### 颜色定义

```csharp
betterColor = "#66FF66"    // 绿色
worseColor = "#FF6666"     // 红色  
neutralColor = "#FFFFFF"   // 白色
arrowColor = "#CCCCCC"     // 箭头（浅灰）
```

## 历史问题与修复

### 问题 1: 后坐力属性无法显示

**问题描述**: RecoilScaleV 和 RecoilScaleH 在对比UI中缺失

**根本原因**: 循环索引递增逻辑错误

```csharp
// ❌ 错误代码
foreach (var hoverProp in hoverProps)
{
  var entry = entries[entryIndex];
  var selectedProp = selectedProps.FirstOrDefault(p => p.Key == hoverProp.Key);
  
  if (selectedProp != null)
  {
    AddComparisonToEntry(...);
  }
  
  entryIndex++;  // ❌ 无论是否匹配都递增！
}
```

**修复方案**: 仅在成功添加时递增索引

```csharp
// ✅ 修复后
foreach (var hoverProp in hoverProps)
{
  var selectedProp = selectedProps.FirstOrDefault(p => p.Key == hoverProp.Key);
  
  if (selectedProp != null)
  {
    var entry = entries[entryIndex];
    AddComparisonToEntry(...);
    entryIndex++;  // ✅ 仅在成功时递增
  }
}
```

## 已知限制

1. **仅支持武器**: 目前只对标记为 `IsGun` 或 `IsMelee` 的物品生效，不支持护甲等其他装备
2. **必须选中**: 必须先点击选中一把武器，才能与悬停武器对比
3. **同类型对比**: 枪支和近战武器各自对比，不能跨类型对比
4. **数值对比**: 仅对数值型属性有效，文本型属性显示为中性色
5. **UI空间**: 对比文本较长，可能在某些分辨率下显示拥挤

## 未来改进

- [ ] 支持护甲、装备等其他物品类型
- [ ] 添加百分比差异显示（如 +15%）
- [ ] 支持自定义颜色方案
- [ ] 添加快捷键快速对比

## 相关文档

- [ModSettings 文档](../Utils/ModSettings.cs)
- [LocalizationHelper 文档](../Utils/LocalizationHelper.cs)
- [资源管理指南](./assets/assets-guide.md)

