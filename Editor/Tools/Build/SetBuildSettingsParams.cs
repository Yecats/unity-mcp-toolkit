using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetBuildSettings MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// Scene list operations use SceneAction + ScenePath/SceneIndex.
    /// </summary>
    public record SetBuildSettingsParams
    {
        // --- Development / debug flags ---

        [McpDescription("Enable or disable Development Build.", Required = false)]
        public bool? DevelopmentBuild { get; set; }

        [McpDescription("Enable or disable script debugging (requires Development Build).", Required = false)]
        public bool? AllowDebugging { get; set; }

        [McpDescription("Wait for managed debugger to attach on player start.", Required = false)]
        public bool? WaitForManagedDebugger { get; set; }

        [McpDescription("Auto-connect the Unity Profiler to the build.", Required = false)]
        public bool? ConnectProfiler { get; set; }

        [McpDescription("Enable deep profiling support in the build.", Required = false)]
        public bool? BuildWithDeepProfilingSupport { get; set; }

        [McpDescription("Compress files in the build package.", Required = false)]
        public bool? CompressFilesInPackage { get; set; }

        // --- Scene list operations ---

        [McpDescription(
            "Scene list action to perform: 'Add', 'Remove', 'Enable', 'Disable', or 'Move'. " +
            "Requires ScenePath for Add/Remove/Enable/Disable. " +
            "Requires SceneFromIndex and SceneToIndex for Move.",
            Required = false)]
        public string SceneAction { get; set; }

        [McpDescription(
            "Scene asset path for Add/Remove/Enable/Disable actions (e.g. 'Assets/Scenes/MainMenu.unity').",
            Required = false)]
        public string ScenePath { get; set; }

        [McpDescription("Source index for the Move action (0-based).", Required = false)]
        public int? SceneFromIndex { get; set; }

        [McpDescription("Destination index for the Move action (0-based).", Required = false)]
        public int? SceneToIndex { get; set; }
    }
}
