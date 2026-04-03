# AnkleBreaker Studio - Utils UI Basics

[![Sponsor](https://img.shields.io/badge/Sponsor-AnkleBreaker%20Studio-red?logo=github)](https://github.com/sponsors/AnkleBreaker-Studio)
[![Asset Store](https://img.shields.io/badge/Asset%20Store-AnkleBreaker%20Studio-blue)](https://assetstore.unity.com/publishers/101837)

Reusable uGUI button components with state tracking, toggle behavior, and generic button-group management. Part of the AnkleBreaker Utils split.

## Installation

Add via Unity Package Manager using the Git URL:

```
https://github.com/AnkleBreaker-Studio/AnkleBreaker-Utils-UI-Basics.git#Release
```

## Dependencies

| Package | Install URL |
|---------|-------------|
| [Utils Inspector](https://github.com/AnkleBreaker-Studio/AnkleBreaker-Utils-Inspector) | `https://github.com/AnkleBreaker-Studio/AnkleBreaker-Utils-Inspector.git#Release` |

## Runtime Components

### ABButton

Enhanced `UnityEngine.UI.Button` that exposes selection-state transitions via C# events and per-state UnityEvents.

| Feature | Description |
|---------|-------------|
| `OnStateChanged(prev, new)` | C# event fired on every state transition |
| `OnHoveredChanged(bool)` | C# event fired on pointer enter/exit |
| `PrevState` / `CurrentState` | Read-only state tracking |
| `IsHovered` | Independent hover tracking |
| `_onNormal`, `_onHover`, `_onUnhover`, `_onSelected`, `_onDisabled` | Per-state UnityEvents wirable from inspector |

On Reset, auto-configures a transparent `Image` for raycasting and disables navigation/transition.

### IUISwitch

Interface for UI elements with an on/off toggle state.

```csharp
public interface IUISwitch
{
    bool IsOn { get; }
    void SetIsOn(bool on);
}
```

### UISwitchButton

Toggle-style button inheriting `ABButton` and implementing `IUISwitch`.

| Feature | Description |
|---------|-------------|
| `IsOn` | Current toggle state |
| `SwitchOnClick` | Whether clicking toggles the state (default: true) |
| `_objToToggleOnSwitch` | Optional GameObject activated/deactivated with state |
| `OnSwitchValueChanged(bool)` | C# event fired on toggle change |
| `_onSwitchedOn`, `_onSwitchedOff`, `_onSwitchChanged` | UnityEvents for inspector wiring |

On Reset, auto-assigns a child named "Display" as the toggle object.

### UISwitchButtonsDic&lt;K, V&gt;

Generic serialized dictionary (`K : Enum`, `V : UISwitchButton`) for managing mutually-exclusive button groups (tabs, option selectors, panel switches).

| Feature | Description |
|---------|-------------|
| `Init(K defaultKey, bool unselectOnClick)` | Initialize with default selection and optional unselect-on-click |
| `Select(K key)` | Switch selection, turning off all others |
| `ClearSelection()` | Turn off all buttons |
| `Reset()` | Unsubscribe listeners and reset state |
| `UnselectOnClick` | When true, clicking the selected button clears the selection |
| `OnSelectionChanged` | Callback when selection changes |
| `OnSwitchClick` | Callback on any button click |

Handles click listener lifecycle automatically. Safe to call `Init()` multiple times (calls `Reset()` internally).

## Editor

### ABButtonEditor

Custom inspector for `ABButton` and subclasses. Preserves the standard Button inspector while adding support for `[FoldoutGroup]`, `[ShowInInspector]`, `[Button]`, `[SectionHeader]`, `[ToggleButton]`, and other Utils Inspector attributes. Collapses helper components (Image, CanvasRenderer) for a cleaner view.

## Requirements

- Unity 2022.3 LTS or later
- [AnkleBreaker Utils Inspector](https://github.com/AnkleBreaker-Studio/AnkleBreaker-Utils-Inspector)

## License

See [LICENSE.md](LICENSE.md)
