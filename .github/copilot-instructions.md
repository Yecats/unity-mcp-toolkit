# Unity MCP Toolkit - Copilot Instructions

## Project Overview

This is a **Unity UPM (Unity Package Manager) package** called `unity-mcp-toolkit` (`com.whatupgames.unity-mcp-toolkit`). It provides custom MCP (Model Context Protocol) tools that **extend** Unity's official `com.unity.ai.assistant` MCP integration. It is NOT a standalone MCP server — it piggybacks on Unity's built-in MCP bridge.

The tools allow AI coding agents (GitHub Copilot, OpenCode, etc.) to read and modify Unity Editor state: project settings, build configuration, scene view cameras, input system bindings, and more.

## Key Technical Details

- **Unity minimum**: 6000.0 (Unity 6+)
- **License**: CC0-1.0
- **Author**: Stacey Haffner
- **Assembly definition**: `McpToolkit.Editor` (editor-only)
- **Namespace**: `WhatUpGames.McpToolkit.Editor`
- **Required dependency**: `com.unity.ai.assistant` 2.4.0-pre.1+

## Repository Structure

```
package.json                    # UPM package manifest
Editor/
  McpToolkit.Editor.asmdef      # Assembly definition (references Unity.AI.MCP.Editor)
  Tools/
    General/                    # General-purpose tools (GetToolkitInfo, GameViewCapture, ForceDomainRefresh)
    ProjectSettings/            # Project settings read/write tools
    Build/                      # Build settings tools
    SceneView/                  # Scene view camera tools
    InputSystem/                # Input system tools (conditional compilation)
Images~/                        # Images for README (~ suffix = Unity ignores this folder)
.plans/                         # Internal planning docs (gitignored)
```

## Coding Conventions

### File Organization
- One file per tool, one file per params record
- Tool files: `<Verb><Noun>Tool.cs` (e.g., `GetPlayerSettingsTool.cs`, `SetPlayerSettingsTool.cs`)
- Params files: `<Verb><Noun>Params.cs` (e.g., `SetPlayerSettingsParams.cs`)
- Params are co-located with their tool in the same directory — NOT in a separate Parameters folder
- Each subsystem gets its own subdirectory under `Editor/Tools/`

### Naming
- Tool names: `McpToolkit.<Verb><Noun>` (e.g., `McpToolkit.GetPlayerSettings`)
- Read tools use `Get` prefix; write tools use `Set` prefix
- Classes: `public static class <Verb><Noun>Tool`
- Method: always `public static object HandleCommand(...)` — no variation
- `McpToolkit.GetToolkitInfo` is the self-describing tool — AI agents call it to discover all available toolkit tools, their types, and enabled status

### EnabledByDefault
- **Read tools and general tools**: `EnabledByDefault = true`
- **Write tools**: `EnabledByDefault = false` (user must opt-in via Project Settings)

### Groups
Each subsystem uses its own group string for the Project Settings UI:
- `"MCP Toolkit"` — General tools
- `"MCP Toolkit - Project Settings"` — Project settings tools
- `"MCP Toolkit - Build"` — Build settings tools
- `"MCP Toolkit - Scene View"` — Scene view camera tools
- `"MCP Toolkit - Input System"` — Input system tools

### Response Format
- Success: `Response.Success("Human-readable message.", new { structured, data })`
- Error: `Response.Error("ERROR_CODE: Human-readable details.")`
- Error codes use SCREAMING_SNAKE_CASE prefix (e.g., `INVALID_PARAMETER`, `GET_EXAMPLE_FAILED`)
- Both come from `Unity.AI.MCP.Editor.Helpers`

## Optional Dependencies

Some tools depend on optional Unity packages (Input System, ProBuilder, Timeline, NavMesh). The package must never force projects to install dependencies they don't need.

This is handled with a single asmdef using `versionDefines` + `#if` conditional compilation:

1. The optional assembly is listed in `references` in `McpToolkit.Editor.asmdef` (e.g., `Unity.InputSystem`)
2. A `versionDefines` entry defines a preprocessor symbol when the package is installed (e.g., `MCP_TOOLKIT_INPUT_SYSTEM`)
3. Every `.cs` file that uses types from the optional package wraps its entire contents in `#if MCP_TOOLKIT_INPUT_SYSTEM` / `#endif`

When the package is absent, Unity silently ignores the missing assembly reference and the `#if` blocks are excluded from compilation. The tools are completely invisible — they don't appear in the MCP settings UI at all.

## UPM Package Requirements

- ALL files in the package (except `Images~/` and `.plans/`) MUST have corresponding `.meta` files
- `.meta` files must be committed to git — without them, Unity cannot import the package
- The `Images~/` folder uses the `~` suffix so Unity ignores it (no `.meta` needed)

## Testing

After making code changes:
1. Force a domain reload (call `McpToolkit.ForceDomainRefresh` or give the Unity Editor focus)
2. Wait ~20-25 seconds for recompilation + MCP reconnection
3. Test the tool via an MCP client call
4. Check Unity Console for compilation errors
