# EfDEnhanced UI Component Library

## 概述

基于"Escape from Duckov"游戏原生UI框架提炼的Mod专用UI组件库。利用游戏已加载的组件（UGUI + DOTween + UniTask），提供高效、一致、易用的UI开发体验。

## 核心优势

- ✅ **零额外开销** - 利用游戏已加载的组件和库
- ✅ **完美集成** - 与游戏原生UI风格完全一致
- ✅ **高效开发** - Builder模式减少70%+代码量
- ✅ **自动绑定** - 设置与UI自动双向同步
- ✅ **流畅动画** - 集成DOTween专业动画效果

---

## 目录结构

```
Utils/UI/
├── Core/
│   └── ModPanel.cs              # Mod面板基类（继承UIPanel）
├── Components/
│   ├── ModButton.cs             # 标准化按钮组件
│   ├── ModToggle.cs             # 标准化Toggle组件
│   └── ModSlider.cs             # 标准化Slider组件
├── Builders/
│   └── FormBuilder.cs           # 表单构建器（自动生成设置UI）
├── Animations/
│   └── ModAnimations.cs         # DOTween动画预设
├── Constants/
│   ├── UIConstants.cs           # UI常量定义
│   └── UIStyles.cs              # 样式预设
└── README.md                    # 本文档
```

---

## 快速开始

### 1. 使用FormBuilder创建设置UI

**传统方式** (~600行代码):
```csharp
// 手动创建容器
var container = new GameObject("Container");
// 手动创建布局
var layout = container.AddComponent<VerticalLayoutGroup>();
// 手动配置布局属性...
layout.spacing = 8;
layout.padding = new RectOffset(15, 15, 15, 15);
// ... 重复100次创建每个Toggle/Slider...
```

**使用FormBuilder** (~20行代码):
```csharp
var form = new FormBuilder(parentTransform)
    .AddSection("Settings_Category_General")
    .AddToggle("Settings_EnableFeature", ModSettings.EnableFeature)
    .AddSlider("Settings_Volume", 0f, 1f, ModSettings.Volume)
    .AddSpacer()
    .AddSection("Settings_Category_Advanced")
    .AddToggle("Settings_DebugMode", ModSettings.DebugMode)
    .Build();
```

**优势**:
- 代码量减少 ~70%
- 自动本地化
- 自动双向数据绑定
- 统一样式和布局

### 2. 使用ModButton创建按钮

```csharp
ModButton.Create(parent)
    .SetText("Settings_SaveButton")           // 自动本地化
    .SetStyle(UIStyles.ButtonStyle.Primary)   // 应用预设样式
    .SetSize(250, 50)                         // 自定义大小
    .OnClick(() => SaveSettings())            // 点击回调
    .Build();
```

**可用样式**:
- `Primary` - 蓝色，主要操作
- `Success` - 绿色，确认操作
- `Danger` - 红色，危险操作
- `Secondary` - 灰色，次要操作
- `Ghost` - 透明，最小化样式

### 3. 使用ModToggle创建开关

```csharp
ModToggle.Create(parent)
    .SetLabel("Settings_EnableFeature")      // 自动本地化
    .BindToSetting(ModSettings.EnableFeature) // 自动双向绑定
    .Build();
```

**手动绑定示例**:
```csharp
ModToggle.Create(parent)
    .SetLabelDirect("Manual Label")
    .OnValueChanged((value) => {
        Debug.Log($"Toggle changed to: {value}");
    })
    .Build();
```

### 4. 使用ModSlider创建滑块

```csharp
// Float滑块
ModSlider.Create(parent)
    .SetLabel("Settings_Volume")
    .SetRange(0f, 1f)
    .BindToSetting(ModSettings.Volume)
    .WithValuePreview()  // 显示当前值
    .Build();

// Int滑块
ModSlider.Create(parent)
    .SetLabel("Settings_MaxItems")
    .SetRange(1, 100)
    .BindToSetting(ModSettings.MaxItems)
    .SetWholeNumbers(true)  // 整数模式
    .Build();
```

### 5. 使用ModPanel创建面板

```csharp
public class MyCustomPanel : ModPanel
{
    protected override void OnOpen()
    {
        base.OnOpen();
        // 面板打开时的逻辑
    }

    protected override void OnClose()
    {
        base.OnClose();
        // 面板关闭时的逻辑
    }
}

// 使用
var panel = gameObject.AddComponent<MyCustomPanel>();
panel.OpenWithAnimation();  // 带DOTween动画的打开
```

---

## DOTween动画预设

### 按钮动画

```csharp
// 点击缩放
ModAnimations.ButtonPressScale(buttonTransform);
ModAnimations.ButtonReleaseScale(buttonTransform);

// 悬停放大
ModAnimations.ButtonHoverScale(buttonTransform, 1.05f);
```

### 淡入淡出

```csharp
// CanvasGroup淡入淡出
ModAnimations.FadeIn(canvasGroup);
ModAnimations.FadeOut(canvasGroup);

// Image淡入淡出
ModAnimations.FadeIn(image);
ModAnimations.FadeOut(image);
```

### 弹出窗口动画

```csharp
// 弹出显示（缩放+淡入）
ModAnimations.PopupShow(transform, canvasGroup);

// 弹出隐藏（缩放+淡出）
ModAnimations.PopupHide(transform, canvasGroup)
    .OnComplete(() => DestroyPanel());
```

### 其他动画

```csharp
// 缩放动画
ModAnimations.ScaleIn(transform);      // 弹性放大
ModAnimations.ScaleOut(transform);     // 缩小消失
ModAnimations.Pulse(transform);        // 脉冲循环

// 滑动动画
ModAnimations.SlideInFromRight(rectTransform);
ModAnimations.SlideOutToLeft(rectTransform);

// 颜色动画
ModAnimations.ColorTo(image, Color.red);
ModAnimations.ColorBlink(graphic, Color.yellow);

// 震动动画（强调/错误提示）
ModAnimations.Shake(transform, strength: 20f);
```

---

## UI常量使用

### 尺寸常量

```csharp
// 使用预定义的尺寸常量
rect.sizeDelta = new Vector2(
    UIConstants.QUEST_PANEL_WIDTH,
    Screen.height * UIConstants.QUEST_PANEL_SCREEN_HEIGHT_RATIO
);

// 间距
layout.spacing = UIConstants.QUEST_ENTRY_SPACING;
```

### 颜色常量

```csharp
// 使用预定义的颜色
titleText.color = UIConstants.QUEST_TITLE_COLOR;
descText.color = UIConstants.QUEST_DESC_COLOR;
completeText.color = UIConstants.QUEST_COMPLETE_COLOR;
```

### 字体大小常量

```csharp
titleText.fontSize = UIConstants.QUEST_TITLE_FONT_SIZE;
descText.fontSize = UIConstants.QUEST_DESC_FONT_SIZE;
```

---

## 样式辅助方法

### 配置Canvas

```csharp
var canvas = gameObject.AddComponent<Canvas>();
UIStyles.ConfigureCanvas(canvas, UIConstants.SETTINGS_PANEL_SORT_ORDER);

var scaler = gameObject.AddComponent<CanvasScaler>();
UIStyles.ConfigureCanvasScaler(scaler);
```

### 配置布局组

```csharp
var layout = container.AddComponent<VerticalLayoutGroup>();
UIStyles.ConfigureVerticalLayout(layout, spacing: 10, 
    padding: new RectOffset(15, 15, 15, 15));

var hLayout = container.AddComponent<HorizontalLayoutGroup>();
UIStyles.ConfigureHorizontalLayout(hLayout, spacing: 5);
```

### 应用文本阴影

```csharp
// 标题阴影
UIStyles.ApplyStandardTextShadow(titleObject, isTitle: true);

// 普通文字阴影
UIStyles.ApplyStandardTextShadow(textObject, isTitle: false);
```

---

## 完整示例：创建自定义设置面板

```csharp
using EfDEnhanced.Utils.UI.Builders;
using EfDEnhanced.Utils.UI.Components;
using EfDEnhanced.Utils.UI.Constants;

public class MyModSettingsPanel : UIPanel
{
    private void BuildUI()
    {
        // 1. 创建Canvas
        var canvas = gameObject.AddComponent<Canvas>();
        UIStyles.ConfigureCanvas(canvas, UIConstants.SETTINGS_PANEL_SORT_ORDER);
        
        var scaler = gameObject.AddComponent<CanvasScaler>();
        UIStyles.ConfigureCanvasScaler(scaler);
        
        var canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // 2. 创建ScrollView容器
        // ... (标准ScrollView创建代码)
        
        // 3. 使用FormBuilder构建表单 - 核心部分！
        var form = new FormBuilder(scrollContent.transform)
            .AddSection("General Settings")
            .AddToggle("Enable Mod", MySettings.EnableMod)
            .AddToggle("Show Notifications", MySettings.ShowNotifications)
            .AddSpacer()
            
            .AddSection("Audio Settings")
            .AddSlider("Master Volume", 0f, 1f, MySettings.MasterVolume)
            .AddSlider("SFX Volume", 0f, 1f, MySettings.SfxVolume)
            .AddSpacer()
            
            .AddSection("Advanced")
            .AddToggle("Debug Mode", MySettings.DebugMode)
            .AddDescription("Enable detailed logging for debugging")
            
            .Build();
        
        // 4. 创建按钮
        ModButton.Create(footerPanel.transform)
            .SetText("Save")
            .SetStyle(UIStyles.ButtonStyle.Success)
            .OnClick(SaveSettings)
            .Build();
        
        ModButton.Create(footerPanel.transform)
            .SetText("Close")
            .SetStyle(UIStyles.ButtonStyle.Secondary)
            .OnClick(() => Close())
            .Build();
    }
}
```

---

## 最佳实践

### 1. 使用常量而非魔法数字

❌ **不好**:
```csharp
text.fontSize = 16;
text.color = new Color(1f, 0.9f, 0.4f);
```

✅ **好**:
```csharp
text.fontSize = UIConstants.QUEST_TITLE_FONT_SIZE;
text.color = UIConstants.QUEST_TITLE_COLOR;
```

### 2. 使用Builder模式而非手动创建

❌ **不好** (100+行):
```csharp
var buttonObj = new GameObject("Button");
var rect = buttonObj.AddComponent<RectTransform>();
rect.sizeDelta = new Vector2(220, 46);
var image = buttonObj.AddComponent<Image>();
image.color = new Color(0.3f, 0.3f, 0.3f);
// ... 更多配置代码
```

✅ **好** (3行):
```csharp
ModButton.Create(parent)
    .SetText("My Button")
    .OnClick(DoSomething)
    .Build();
```

### 3. 使用异常辅助类

❌ **不好**:
```csharp
try
{
    DoSomething();
}
catch (Exception ex)
{
    Debug.LogError($"Error: {ex}");
}
```

✅ **好**:
```csharp
ExceptionHelper.SafeExecute(() => DoSomething(), "MyMethod");
```

### 4. 自动数据绑定

❌ **不好** (手动同步):
```csharp
toggle.isOn = ModSettings.EnableFeature.Value;
toggle.onValueChanged.AddListener(value => {
    ModSettings.EnableFeature.Value = value;
});
ModSettings.EnableFeature.ValueChanged += (s, e) => {
    toggle.isOn = e.NewValue;
};
```

✅ **好** (自动绑定):
```csharp
ModToggle.Create(parent)
    .BindToSetting(ModSettings.EnableFeature)
    .Build();
```

---

## 性能提示

1. **复用UI元素** - 使用对象池模式，避免频繁创建/销毁
2. **使用DOTween** - 比自定义协程动画性能更好
3. **批量更新** - 合并多个UI更新到一帧
4. **使用脏标记** - 只在真正需要时才更新UI

---

## 调试工具

### IMGUI调试面板

按 **F8** 键打开/关闭调试面板，可以：
- 查看Mod运行状态
- 实时查看设置值
- 查看已追踪的任务
- 快速重置/重载设置
- 查看FPS和内存使用

```csharp
// 在ModBehaviour.cs中已自动初始化
_debugPanel = ModDebugPanel.Create();
```

---

## API参考

### FormBuilder

#### 方法

| 方法 | 说明 | 示例 |
|-----|------|------|
| `AddSection(titleKey)` | 添加分节标题 | `.AddSection("General Settings")` |
| `AddToggle(labelKey, setting)` | 添加Toggle（自动绑定） | `.AddToggle("Enable", setting)` |
| `AddSlider(labelKey, min, max, setting)` | 添加Slider（自动绑定） | `.AddSlider("Volume", 0f, 1f, setting)` |
| `AddSpacer(height)` | 添加空白间距 | `.AddSpacer(20f)` |
| `AddDescription(textKey)` | 添加描述文本 | `.AddDescription("Help text")` |
| `AddCustomElement(gameObject)` | 添加自定义元素 | `.AddCustomElement(myElement)` |
| `Build()` | 完成构建 | `.Build()` |

### ModButton

#### 方法

| 方法 | 说明 | 示例 |
|-----|------|------|
| `Create(parent)` | 创建按钮实例 | `ModButton.Create(parent)` |
| `SetText(key)` | 设置文本（本地化） | `.SetText("Save")` |
| `SetTextDirect(text)` | 设置文本（直接） | `.SetTextDirect("Click Me")` |
| `SetStyle(style)` | 设置样式 | `.SetStyle(ButtonStyle.Success)` |
| `SetSize(w, h)` | 设置大小 | `.SetSize(200, 40)` |
| `OnClick(action)` | 添加点击事件 | `.OnClick(() => Save())` |
| `SetInteractable(bool)` | 设置可交互性 | `.SetInteractable(false)` |

### ModToggle

#### 方法

| 方法 | 说明 | 示例 |
|-----|------|------|
| `Create(parent)` | 创建Toggle实例 | `ModToggle.Create(parent)` |
| `SetLabel(key)` | 设置标签（本地化） | `.SetLabel("Enable")` |
| `BindToSetting(setting)` | 绑定到设置（自动同步） | `.BindToSetting(setting)` |
| `OnValueChanged(callback)` | 添加值变化回调 | `.OnValueChanged(v => ...)` |
| `SetInteractable(bool)` | 设置可交互性 | `.SetInteractable(true)` |

### ModSlider

#### 方法

| 方法 | 说明 | 示例 |
|-----|------|------|
| `Create(parent)` | 创建Slider实例 | `ModSlider.Create(parent)` |
| `SetLabel(key)` | 设置标签（本地化） | `.SetLabel("Volume")` |
| `SetRange(min, max)` | 设置范围 | `.SetRange(0f, 1f)` |
| `BindToSetting(setting)` | 绑定到Float设置 | `.BindToSetting(floatSetting)` |
| `BindToSetting(setting)` | 绑定到Int设置 | `.BindToSetting(intSetting)` |
| `SetWholeNumbers(bool)` | 设置整数模式 | `.SetWholeNumbers(true)` |
| `SetValueFormat(format)` | 设置值格式 | `.SetValueFormat("F2")` |

### ModAnimations

#### 预设动画

| 动画 | 说明 | 用途 |
|-----|------|------|
| `FadeIn/FadeOut` | 淡入淡出 | 面板显示/隐藏 |
| `ScaleIn/ScaleOut` | 缩放 | 元素出现/消失 |
| `SlideInFromRight` | 滑入 | 侧边面板 |
| `PopupShow/PopupHide` | 弹出 | 对话框、提示 |
| `Pulse` | 脉冲 | 强调元素 |
| `Shake` | 震动 | 错误提示 |
| `ColorBlink` | 颜色闪烁 | 警告、通知 |

---

## 实战案例

### 案例1：ModSettingsPanel重构

**重构前**: 890行代码，手动创建每个UI元素  
**重构后**: 300行代码，使用FormBuilder

**代码对比**:

见 `Features/ModSettingsPanel.cs` vs `Features/ModSettingsPanel.cs.backup`

**成果**:
- 代码量减少 66%
- 维护成本大幅降低
- 添加新设置项只需1行代码
- 自动处理本地化和数据绑定

---

## 扩展组件库

### 添加新组件

1. 在 `Utils/UI/Components/` 创建新组件
2. 继承适当的Unity UI组件
3. 实现Builder模式接口
4. 添加到FormBuilder支持

### 添加新动画

1. 在 `ModAnimations.cs` 添加新的静态方法
2. 使用DOTween API创建动画
3. 设置 `.SetUpdate(true)` 以不受时间缩放影响
4. 返回 `Tween` 或 `Sequence` 对象

---

## 技术细节

### 依赖的游戏组件

- **UGUI** (`UnityEngine.UI`) - Canvas, Button, Slider等
- **DOTween** (`DG.Tweening`) - 补间动画库
- **TextMeshPro** (`TMPro`) - 高级文本渲染
- **TrueShadow** (`LeTai.TrueShadow`) - UI阴影效果
- **UniTask** (`Cysharp.Threading.Tasks`) - 高性能异步

### 项目集成

已添加到 `EfDEnhanced.csproj`:
```xml
<Reference Include="$(DuckovDataPath)\Managed\DOTween.dll" />
<Reference Include="$(DuckovDataPath)\Managed\DOTween.Modules.dll" />
```

---

## 常见问题

**Q: 为什么不使用IMGUI？**  
A: IMGUI适合简单调试工具，但性能和样式定制能力有限。我们使用UGUI保持与游戏一致的外观和性能。

**Q: 为什么不引入外部UI库？**  
A: 游戏已经加载了完善的UI框架。引入外部库会增加Mod体积和兼容性风险。

**Q: FormBuilder性能如何？**  
A: 与手动创建相同，因为底层使用相同的Unity API。但减少了代码量和出错概率。

**Q: 如何添加自定义样式？**  
A: 在 `UIConstants.cs` 添加新颜色常量，在 `UIStyles.cs` 添加新样式方法。

---

## 更新日志

### v1.0 (2024-10-21)
- ✅ 初始版本发布
- ✅ 实现FormBuilder
- ✅ 实现ModButton, ModToggle, ModSlider
- ✅ 集成DOTween动画
- ✅ 重构ModSettingsPanel（减少66%代码）
- ✅ 添加IMGUI调试面板

---

## 贡献指南

欢迎贡献新组件和改进！请遵循：

1. **一致的命名** - 使用 `Mod` 前缀
2. **Builder模式** - 支持链式调用
3. **自动绑定** - 支持绑定到SettingsEntry
4. **异常处理** - 使用ExceptionHelper
5. **文档完整** - 添加XML注释和使用示例

---

## 相关文档

- [CLAUDE.md](../../../CLAUDE.md) - 完整项目文档
- [ModBehaviour.cs](../../../ModBehaviour.cs) - Mod入口点
- [ModSettings.cs](../../ModSettings.cs) - 设置系统

---

Made with ❤️ for EfDEnhanced Mod

