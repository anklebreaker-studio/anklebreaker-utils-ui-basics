# Changelog

## [0.3.0] - 2026-04-01

### Changed (Breaking)
- Rename Tab vocabulary to generic Selection vocabulary in UISwitchButtonsDic:
  - `DefaultTab` → `DefaultKey`
  - `CurrentTab` → `SelectedKey`
  - `AllTabClosed` → `NoneSelected`
  - `SelectTab(K)` → `Select(K)`
  - `CloseAllTab()` → `ClearSelection()`

### Added
- `Reset()` method to properly unsubscribe click delegates and reset state
- `IsInitialized` public read-only property
- `_clickDelegates` dictionary for safe targeted listener removal (no more RemoveAllListeners)

### Fixed
- `Init()` now properly initializes `SelectedKey` and `NoneSelected`
- `Init()` calls `Reset()` internally when called multiple times (prevents listener accumulation)
- Null-checks in `Select()` and `ClearSelection()` (consistent with `Init()`)

## [0.2.0] - 2026-04-01

### Added
- UISwitchButtonsDic<K, V> — generic switch-button dictionary for UISwitchButton collections (Init, SelectTab, CloseAllTab, OnSelectionChanged, OnSwitchClick)
- Unit tests for UISwitchButtonsDic (7 tests covering Init, SelectTab, CloseAllTab, callbacks)
- Test assembly definition (AnkleBreaker.Utils.UIBasics.Tests)

## [0.1.0] - 2026-03-30

### Added
- Initial package setup — structure, assembly definitions, and metadata
