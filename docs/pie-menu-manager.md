# PieMenuManager - 全局菜单控制

## 概述

`PieMenuManager` 是一个全局的静态管理器类，用于控制 `PieMenuComponent` 实例的显示。它确保同一时刻只能有一个 `PieMenuComponent` 显示，如果有新菜单要显示，旧菜单会自动被取消。

## 实现原理

### 核心机制

`PieMenuManager` 维护一个静态引用 `_activeMenu` 来跟踪当前活动的菜单实例。

```csharp
private static PieMenuComponent? _activeMenu = null;
public static PieMenuComponent? ActiveMenu => _activeMenu;
```

### 关键方法

#### 1. `RegisterActiveMenu(PieMenuComponent menu)`

当菜单调用 `Show()` 时触发，作用：
- 如果已有其他菜单在显示，自动调用 `Cancel()` 来关闭它
- 将新菜单注册为活动菜单

```csharp
public static void RegisterActiveMenu(PieMenuComponent menu)
{
    if (_activeMenu != null && _activeMenu != menu && _activeMenu.IsOpen)
    {
        _activeMenu.Cancel();  // 取消旧菜单
    }
    _activeMenu = menu;        // 注册新菜单
}
```

#### 2. `UnregisterMenu(PieMenuComponent menu)`

当菜单调用 `Hide()` 或 `Cancel()` 时触发，作用：
- 如果这个菜单是当前活动菜单，则取消注册

```csharp
public static void UnregisterMenu(PieMenuComponent menu)
{
    if (_activeMenu == menu)
    {
        _activeMenu = null;    // 清空活动菜单引用
    }
}
```

#### 3. `CancelActiveMenu()` (可选)

手动取消当前活动的菜单（如果需要的话）

```csharp
public static void CancelActiveMenu()
{
    if (_activeMenu != null)
    {
        _activeMenu.Cancel();
    }
}
```

## 集成方式

### 在 PieMenuComponent 中的集成

**Show() 方法：**
```csharp
public void Show()
{
    if (_isOpen) return;

    // 注册此菜单为活动菜单，自动取消其他菜单
    PieMenuManager.RegisterActiveMenu(this);

    _isOpen = true;
    gameObject.SetActive(true);
    // ... 其他初始化代码 ...
}
```

**Hide() 方法：**
```csharp
public void Hide(bool invokeSelectedItem = false)
{
    if (!_isOpen) return;

    // ... 菜单隐藏逻辑 ...

    _isOpen = false;
    gameObject.SetActive(false);

    // 从全局管理器中注销
    PieMenuManager.UnregisterMenu(this);

    OnMenuHidden?.Invoke();
}
```

**Cancel() 方法：**
```csharp
public void Cancel()
{
    if (!_isOpen) return;

    _isOpen = false;
    gameObject.SetActive(false);

    // 从全局管理器中注销
    PieMenuManager.UnregisterMenu(this);

    OnMenuCancelled?.Invoke();
}
```

## 使用场景

项目中有三个菜单，都继承了这个行为：

1. **ItemWheelMenu** - 物品轮盘菜单（~ 键）
2. **ThrowableWheelMenu** - 投掷物菜单（G 键）  
3. **ContainerWheelMenu** - 容器菜单（从物品菜单打开）

### 自动控制流程

```
用户按 ~ 键
  ↓
ItemWheelMenu.Show() 被调用
  ↓
PieMenuManager.RegisterActiveMenu(itemMenu) 被调用
  ↓
如果 ThrowableWheelMenu 已打开，自动调用 ThrowableWheelMenu.Cancel()
  ↓
ItemWheelMenu 显示
  ↓
用户按 ~ 键释放
  ↓
ItemWheelMenu.Hide() 被调用
  ↓
PieMenuManager.UnregisterMenu(itemMenu) 被调用
  ↓
菜单消失，_activeMenu 重置为 null
```

## 优势

1. **防止菜单重叠** - 确保不会同时显示多个菜单
2. **自动管理** - 无需手动检查其他菜单状态
3. **简洁易用** - 只需调用 `Show()`、`Hide()`、`Cancel()` 即可
4. **可扩展** - 可轻松添加新的菜单类型而无需修改现有逻辑
5. **一致的用户体验** - 用户不会看到菜单重叠或混乱

## 技术细节

- **线程安全性** - 当前实现假设在单线程 Unity 主线程中执行
- **性能** - 极小的性能开销（仅是引用赋值和简单条件检查）
- **可查询** - 可通过 `PieMenuManager.ActiveMenu` 检查当前活动菜单

## 可能的扩展

未来可以添加：
- 菜单优先级系统
- 菜单队列/堆栈管理
- 菜单过渡动画
- 菜单事件回调（如 onMenuOpened、onMenuClosed）
