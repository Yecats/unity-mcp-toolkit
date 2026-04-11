# Changelog

## [0.2.2] - 2026-04-10

### Added
- **Recorder tools** — `McpToolkit.GetRecorderSettings` reads session config, recorder list with type-specific properties (Movie, Image Sequence, Audio, Animation Clip), and recording state. `McpToolkit.SetRecorderSettings` manages session-level settings and the recorder list (add, remove, enable, disable, modify). `McpToolkit.StartRecording` starts a recording session with auto-enter Play mode support. `McpToolkit.StopRecording` stops any active recording. Requires `com.unity.recorder` 5.0.0+ (auto-detected via `versionDefines`).
- New tool group: **MCP Toolkit - Recorder** in Project Settings UI.

### Changed
- Updated `McpToolkit.Editor.asmdef` to add `Unity.Recorder.Editor` reference and `MCP_TOOLKIT_RECORDER` version define.

## [0.2.1] - 2026-04-10

### Added
- **`McpToolkit.GetToolkitInfo`** — Self-describing tool that returns the package version and a categorized list of all toolkit tools with their type, description, and enabled status. AI agents should call this first to discover what the toolkit provides. Enabled by default.
- Conditional Compilation section in README documenting the optional dependency pattern for contributors.

### Fixed
- **`McpToolkit.GameViewCapture`** — Added `MaxDimension` downscaling (default 1920px) to prevent MCP payload truncation on high-resolution displays.

## [0.2.0] - 2026-04-06

### Added
- **Project Settings tools** — 5 read tools and 5 write tools for PlayerSettings, QualitySettings, PhysicsSettings, TimeAndAudioSettings, and ScriptExecutionOrder.
- **Build Settings tools** — `McpToolkit.GetBuildSettings` reads build target, dev/debug/profiler flags, scenes in build, installed platforms, and compilation defines. `McpToolkit.SetBuildSettings` toggles development build flags and manages the scene list (add, remove, enable, disable, reorder). Platform switching and build execution are excluded for safety.
- **Scene View Camera tools** — `McpToolkit.GetSceneViewCamera` reads camera pivot, rotation, zoom, projection, gizmos, lighting, grid, and draw mode. `McpToolkit.SetSceneViewCamera` modifies camera properties and supports LookAt/FrameGameObject actions.
- **Input System tools** — `McpToolkit.GetInputActions` lists all Input Action Assets with action maps, actions, bindings, and control schemes. `McpToolkit.SetInputActions` supports CRUD operations on action maps, actions, and bindings, plus wholesale JSON replacement. Requires `com.unity.inputsystem` 1.0.0+ (auto-detected via `versionDefines`).
- Tool groups in Project Settings UI: tools are organized under collapsible foldouts (MCP Toolkit, MCP Toolkit - Project Settings, MCP Toolkit - Build, MCP Toolkit - Scene View, MCP Toolkit - Input System).
- Write tools are disabled by default and must be explicitly enabled by the user.

### Changed
- Reorganized tool files from flat `Editor/Tools/` into subsystem folders (`General/`, `ProjectSettings/`, `Build/`, `SceneView/`, `InputSystem/`).
- Moved `GameViewCaptureParams.cs` from `Editor/Tools/Parameters/` to `Editor/Tools/General/` (co-located with its tool).
- Removed empty `Editor/Tools/Parameters/` directory.
- Updated `McpToolkit.Editor.asmdef` to add `Unity.InputSystem` reference and `versionDefines` for optional Input System dependency.

## [0.1.0] - 2026-04-05

### Added
- `McpToolkit.GameViewCapture` — Captures the Game View as a base64-encoded PNG. Supports a resolution multiplier parameter (1-4x the native Game View size). Automatically accounts for OS display scaling (e.g. 125%, 150%, Retina) so the captured image matches the actual pixel dimensions of the Game View. Works in Edit and Play mode.
- `McpToolkit.ForceDomainRefresh` — Forces a domain reload even when Unity is unfocused.
