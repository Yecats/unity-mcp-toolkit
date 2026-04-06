# Unity MCP Toolkit

A community-driven collection of custom MCP tools that extend Unity's official MCP integration. Each tool adds editor automation capabilities that AI coding agents can use to interact with the Unity Editor.

## Requirements

- **Unity 6+** (`6000.0` or later)
- **[Unity AI Assistant](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/index.html)** (`com.unity.ai.assistant` 2.4.0-pre.1 or later) — this is a pre-release package

### Optional Dependencies

- **[Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)** (`com.unity.inputsystem` 1.0.0 or later) — required for Input System tools. Tools for optional packages appear automatically when the package is installed and are completely hidden when it is not — no errors, no configuration needed.

## Installation

Add the following to your project's `Packages/manifest.json`:

```json
{
    "dependencies": {
        "com.whatupgames.unity-mcp-toolkit": "https://github.com/yecats/unity-mcp-toolkit.git"
    }
}
```

Or use **Window > Package Manager > + > Add package from git URL** and enter:

```
https://github.com/yecats/unity-mcp-toolkit.git
```

## Tools

### General

| Tool | Type | Default | Description |
|---|---|---|---|
| `McpToolkit.GameViewCapture` | Read | On | Captures the Game View as a base64-encoded PNG. Supports a resolution multiplier (1-4x). Accounts for OS display scaling. Works in Edit and Play mode. |
| `McpToolkit.ForceDomainRefresh` | Action | On | Forces a domain reload even when the Unity Editor is not in the foreground. Use after modifying scripts externally to trigger recompilation without switching to Unity. |

### Project Settings

| Tool | Type | Default | Description |
|---|---|---|---|
| `McpToolkit.GetPlayerSettings` | Read | On | Reads project identity, rendering, scripting configuration, and platform settings. |
| `McpToolkit.SetPlayerSettings` | Write | Off | Modifies player settings (company name, color space, scripting backend, defines, etc.). |
| `McpToolkit.GetQualitySettings` | Read | On | Reads all quality levels with shadow, AA, VSync, LOD, and rendering properties. |
| `McpToolkit.SetQualitySettings` | Write | Off | Modifies quality settings for a specific or active quality level. |
| `McpToolkit.GetPhysicsSettings` | Read | On | Reads 3D and 2D physics settings (gravity, solver iterations, queries, etc.). |
| `McpToolkit.SetPhysicsSettings` | Write | Off | Modifies 3D and 2D physics settings. |
| `McpToolkit.GetTimeAndAudioSettings` | Read | On | Reads time (fixed timestep, time scale) and audio (sample rate, voices) settings. |
| `McpToolkit.SetTimeAndAudioSettings` | Write | Off | Modifies time and audio settings. |
| `McpToolkit.GetScriptExecutionOrder` | Read | On | Reads scripts with custom execution order. |
| `McpToolkit.SetScriptExecutionOrder` | Write | Off | Sets execution order for a MonoScript by class name. |

### Build

| Tool | Type | Default | Description |
|---|---|---|---|
| `McpToolkit.GetBuildSettings` | Read | On | Reads build target, development/debug/profiler flags, scenes in build, installed platforms, and compilation defines. |
| `McpToolkit.SetBuildSettings` | Write | Off | Toggles dev build, debugging, profiler flags. Manages scenes in build (add, remove, enable, disable, reorder). Excludes platform switching and build execution for safety. |

### Scene View

| Tool | Type | Default | Description |
|---|---|---|---|
| `McpToolkit.GetSceneViewCamera` | Read | On | Reads Scene View camera pivot, rotation, zoom, projection, 2D mode, gizmos, lighting, grid, draw mode, and camera settings. |
| `McpToolkit.SetSceneViewCamera` | Write | Off | Modifies Scene View camera properties. Supports LookAt (world point) and FrameGameObject (frame by name) actions. |

### Input System

*Requires `com.unity.inputsystem` 1.0.0+. Tools appear automatically when the package is installed.*

| Tool | Type | Default | Description |
|---|---|---|---|
| `McpToolkit.GetInputActions` | Read | On | Lists all Input Action Assets with action maps, actions, bindings, and control schemes. Supports summary or detailed view per asset. |
| `McpToolkit.SetInputActions` | Write | Off | Modifies Input Action Assets: add/remove action maps, actions, and bindings. Supports wholesale JSON replacement. |

## Managing Tools

You can toggle individual MCP Toolkit tools on and off in **Edit > Project Settings > AI > Unity MCP > Tools**. Tools are organized into collapsible groups:

- **MCP Toolkit** — General tools (GameViewCapture, ForceDomainRefresh)
- **MCP Toolkit - Project Settings** — All project settings read/write tools
- **MCP Toolkit - Build** — Build configuration tools
- **MCP Toolkit - Scene View** — Scene View camera tools
- **MCP Toolkit - Input System** — Input Action tools

Write tools are **disabled by default** and must be explicitly enabled by the user.

![MCP Toolkit tools in Project Settings](Images~/projectSettings.png)

## Contributing

This toolkit is meant to grow over time with tools the community finds useful. If you have an idea for a tool that would make your workflow better, chances are others would benefit from it too. Contributions are welcome and encouraged!

Please keep the following in mind:

- **Follow the Unity convention for file structure.** Tool logic goes in `Editor/Tools/<Subsystem>/`, parameter records are co-located with their tools. One file per tool, one file per params record.
- **AI-assisted contributions are welcome**, but a human must review the code and PR description before submitting. PRs that appear to be 100% vibe-coded without human review will be sent back.

## License

This project is dedicated to the public domain under the [CC0 1.0 Universal](LICENSE) license. You can copy, modify, and distribute it without asking permission or giving credit.
