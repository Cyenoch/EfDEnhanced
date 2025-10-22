# EfDEnhanced 功能说明

## 功能列表

### 1. 任务追踪器 (ActiveQuestTracker)
在 Raid 中显示当前任务进度的 HUD 追踪器。

**使用方式：**
- 自动在进入 Raid 时启用
- 可在设置菜单中启用/禁用
- 可配置是否显示任务描述

**相关文件：**
- `Features/ActiveQuestTracker.cs`
- `Patches/QuestViewDetailsPatch.cs`

---

### 2. 物品轮盘菜单 (Item Wheel Menu)
快速访问物品栏的径向菜单，通过按键 `~` 打开。

**使用方式：**
1. 按 `~` 键（波浪号键，通常在键盘左上角 Esc 下方）打开轮盘菜单
2. 移动鼠标选择要使用的物品（物品栏 3-8 对应的物品）
3. 鼠标移出中心死区后，会高亮显示选中的物品
4. **左键点击**：使用/装备选中的物品
5. **右键点击** 或 **ESC**：取消并关闭菜单

**功能特性：**
- 空心圆饼图设计，分为 6 个扇区
- 对应物品栏快捷键 3-8 的物品
- 实时显示物品名称
- 鼠标悬停时扇区会放大并高亮
- 自动在打开其他界面（背包、地图等）时关闭
- 打开菜单时记住鼠标位置

**配置参数：**
- `WHEEL_RADIUS`: 轮盘外半径（150 像素）
- `INNER_RADIUS`: 轮盘内半径（50 像素）
- `DEAD_ZONE`: 中心死区半径（30 像素）
- `SEGMENT_HOVER_SCALE`: 悬停时扇区缩放（1.1 倍）

**相关文件：**
- `Features/ItemWheelMenu.cs` - 轮盘菜单 UI 组件
- `Patches/ItemWheelMenuPatch.cs` - 输入监听和集成补丁

**技术说明：**
- 使用 Unity Canvas 系统渲染 UI
- 程序化生成圆饼图纹理
- 通过 Harmony Patch 集成到游戏输入系统
- 防止在菜单打开时触发其他输入（移动、射击等）

---

### 3. 设置系统 (ModSettingsContent)
在游戏暂停菜单中添加模组设置选项。

**使用方式：**
- 按 ESC 打开暂停菜单
- 点击 "Mod Settings" 进入设置界面
- 可配置各项功能的开关和参数

**相关文件：**
- `Features/ModSettingsContent.cs`
- `Patches/OptionsPanelPatch.cs`
- `Utils/ModSettings.cs`

---

### 4. 武器对比系统
在物品悬停时显示与当前装备武器的对比信息。

**相关文件：**
- `Patches/ItemHoveringComparisonPatch.cs`
- `Utils/StatPolarityMap.cs`

---

## 开发指南

### 添加新功能
1. 在 `Features/` 目录创建功能类
2. 如需修改游戏行为，在 `Patches/` 目录创建 Harmony 补丁
3. 使用 `Utils/ModLogger.cs` 进行日志记录
4. 更新本文档说明新功能的使用方式

### 调试
- 查看日志：`./scripts/rlog.sh`
- 构建：`dotnet build -c Release`
- 部署：自动部署到游戏目录（构建后触发）

### 注意事项
- 所有补丁必须包含异常处理
- UI 组件应使用 `Utils/UI/` 下的工具类
- 遵循 C# 代码规范（参考 `.cursor/rules/csharp-style.md`）
