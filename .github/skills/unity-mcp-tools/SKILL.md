---
name: unity-mcp-tools
description: Templates, patterns, and mechanics for creating MCP tools in the unity-mcp-toolkit package. Covers read tools, write tools, parameter records, .meta files, conditional compilation, and error handling.
---

# MCP Tool Creation — Full Reference

This skill contains everything needed to write correct MCP tools for the `unity-mcp-toolkit` package. It covers the exact code patterns, required attributes, parameter conventions, response format, and supporting files.

## Required Usings

```csharp
// Always required — these provide the MCP attributes and response helpers:
using Unity.AI.MCP.Editor.Helpers;       // Response.Success(), Response.Error()
using Unity.AI.MCP.Editor.ToolRegistry;  // [McpTool], [McpDescription], [McpOutputSchema]

// Commonly needed:
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
```

## How Tool Discovery Works

Unity's `McpToolRegistry` uses `TypeCache.GetMethodsWithAttribute<McpToolAttribute>()` to scan ALL loaded assemblies for `[McpTool]` methods. The method must be `public static` with 0 or 1 parameters. If there is a parameter, the registry auto-generates a JSON schema from the record's `[McpDescription]`-decorated properties.

### Five Gates for Tool Visibility
1. Method is `public static` with 0-1 params; assembly references `Unity.AI.MCP.Editor`
2. `MCPSettings.IsToolEnabled()` checks overrides, then falls back to `EnabledByDefault`
3. `McpToolFilter.Filter` delegate (null = pass-through)
4. MCP bridge must be running
5. Client must reconnect or poll `get_available_tools` to see new tools

## Read Tool Template

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads [what it reads] and returns it as structured data.
    /// </summary>
    public static class Get[Name]Tool
    {
        const string ToolName = "McpToolkit.Get[Name]";

        const string Description =
            "[Detailed description of what the tool reads and returns. This text "  +
            "is shown to the AI agent, so be specific about the data shape.]";

        const string Title = "Read [Name]";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "[group string]" })]
        public static object HandleCommand()
        {
            try
            {
                var data = new
                {
                    // Use camelCase property names
                    // Resolve enums to strings: someEnum.ToString()
                    // Group related properties logically
                };

                return Response.Success(
                    "[Summary of what was retrieved, including context like build target].",
                    data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_[NAME]_FAILED: {ex.Message}");
            }
        }
    }
}
```

### Read Tool Guidelines
- No parameters needed — read tools take no input
- Return anonymous objects with camelCase property names
- Convert enums to strings via `.ToString()` so the AI agent gets readable values
- Include contextual info (e.g., which build target the settings are for)
- The `Description` is critical — it's what the AI agent uses to decide whether to call this tool

## Write Tool Template

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies [what it modifies].
    /// Only supplied (non-null) parameters are applied — omitted values are left unchanged.
    /// </summary>
    public static class Set[Name]Tool
    {
        const string ToolName = "McpToolkit.Set[Name]";

        const string Description =
            "Modifies [what]. Only supplied parameters are changed — omitted " +
            "values are left as-is. Supports [list of settable things].";

        const string Title = "Modify [Name]";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "[group string]" })]
        public static object HandleCommand(Set[Name]Params parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var changes = new List<string>();

                // --- String / enum parameter ---
                if (parameters.SomeEnumValue != null)
                {
                    if (Enum.TryParse<SomeEnum>(parameters.SomeEnumValue, true, out var parsed))
                    {
                        SomeAPI.someProperty = parsed;
                        changes.Add($"someProperty = {parsed}");
                    }
                    else
                        return Response.Error(
                            $"INVALID_PARAMETER: Unknown SomeEnum '{parameters.SomeEnumValue}'. " +
                            "Use 'OptionA', 'OptionB', or 'OptionC'.");
                }

                // --- Nullable value type parameter ---
                if (parameters.SomeNumber.HasValue)
                {
                    SomeAPI.someNumber = parameters.SomeNumber.Value;
                    changes.Add($"someNumber = {parameters.SomeNumber.Value}");
                }

                // --- Nullable bool parameter ---
                if (parameters.SomeBool.HasValue)
                {
                    SomeAPI.someBool = parameters.SomeBool.Value;
                    changes.Add($"someBool = {parameters.SomeBool.Value}");
                }

                if (changes.Count == 0)
                    return Response.Success(
                        "No changes applied — all parameters were null/omitted.",
                        new { changesApplied = 0 });

                return Response.Success(
                    $"[Name] updated: {changes.Count} value(s) changed.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_[NAME]_FAILED: {ex.Message}");
            }
        }
    }
}
```

### Write Tool Guidelines
- Always `EnabledByDefault = false`
- Null-check the `parameters` object first
- Track every change in a `List<string>` so the response tells the agent exactly what happened
- Validate enum strings with `Enum.TryParse` — return `INVALID_PARAMETER` with valid options if it fails
- Return early on first validation error (don't partially apply changes then error)
- If all parameters were null/omitted, return success with "no changes" — not an error

## Parameter Record Template

```csharp
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.Set[Name] MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// </summary>
    public record Set[Name]Params
    {
        [McpDescription("Description including valid values: 'A', 'B', 'C'.", Required = false)]
        public string EnumParam { get; set; }

        [McpDescription("Description of what this number controls.", Required = false)]
        public int? IntParam { get; set; }

        [McpDescription("Description of what this number controls.", Required = false)]
        public float? FloatParam { get; set; }

        [McpDescription("Whether to enable or disable something.", Required = false)]
        public bool? BoolParam { get; set; }
    }
}
```

### Parameter Record Rules
- MUST be a `record`, not a `class` — this matches Unity's 100% consistent pattern
- Every property MUST have `[McpDescription("...", Required = true|false)]`
- Use `string` for enums (validated in the tool with `Enum.TryParse`)
- Use `int?`, `float?`, `bool?` for optional value types — the null check is how you know if the agent supplied it
- List valid enum values in the description text so the agent knows what to pass
- Keep properties flat — no nested objects or arrays
- `Required = true` only for parameters the tool cannot function without (rare for write tools)

## .meta File Format

Every `.cs` file MUST have a corresponding `.cs.meta` file or Unity will not import it. The format:

```yaml
fileFormatVersion: 2
guid: <32-char-lowercase-hex-guid>
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
```

- The GUID must be exactly 32 lowercase hexadecimal characters
- Each file gets a unique GUID — never reuse GUIDs
- Directory `.meta` files use a different format (see existing ones in the repo for reference)

## Error Handling Patterns

### Error Code Conventions
- Prefix with SCREAMING_SNAKE_CASE: `GET_PLAYER_SETTINGS_FAILED`, `INVALID_PARAMETER`, `SET_PHYSICS_FAILED`
- Include the actual error message: `$"ERROR_CODE: {ex.Message}"`
- For validation errors, list valid options: `"Use 'Gamma' or 'Linear'."`

### Error Priority
1. Null parameter check (write tools only) — before try/catch
2. Enum validation — return immediately on first invalid value
3. General exception catch — wraps the entire body

## Conditional Compilation

For tools that depend on optional Unity packages (e.g., Input System, ProBuilder):

### 1. Add versionDefines to the asmdef

```json
{
    "versionDefines": [
        {
            "name": "com.unity.inputsystem",
            "expression": "1.0.0",
            "define": "MCP_TOOLKIT_INPUT_SYSTEM"
        }
    ]
}
```

### 2. Wrap the entire file

```csharp
#if MCP_TOOLKIT_INPUT_SYSTEM
using System;
// ... rest of usings and code ...
namespace WhatUpGames.McpToolkit.Editor
{
    public static class GetInputActionsTool
    {
        // ... tool implementation ...
    }
}
#endif
```

- Wrap the ENTIRE file, not just the class body
- The `.meta` file is still required even though the code might be compiled out
- The asmdef also needs the package added to `references` (alongside the `versionDefines`)

## Real Example: SetQualitySettings RestoreAndError Pattern

When a write tool modifies settings that could throw mid-operation and leave things in a bad state, capture the original value first and restore on error:

```csharp
// Helper to restore the active quality level if an error occurs mid-operation
static object RestoreAndError(int originalLevel, string message)
{
    QualitySettings.SetQualityLevel(originalLevel, true);
    return Response.Error(message);
}
```

Use this pattern when the tool switches context (e.g., changing active quality level to modify a specific level's settings) and needs to guarantee the original state is restored.
