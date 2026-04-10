using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetPlayerSettings MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// </summary>
    public record SetPlayerSettingsParams
    {
        [McpDescription("Company name.", Required = false)]
        public string CompanyName { get; set; }

        [McpDescription("Product name.", Required = false)]
        public string ProductName { get; set; }

        [McpDescription("Bundle version string (e.g. '1.0.0').", Required = false)]
        public string BundleVersion { get; set; }

        [McpDescription("Application identifier / bundle ID (e.g. 'com.company.product').", Required = false)]
        public string ApplicationIdentifier { get; set; }

        [McpDescription("Color space: 'Gamma' or 'Linear'.", Required = false)]
        public string ColorSpace { get; set; }

        [McpDescription("Default screen width in pixels.", Required = false)]
        public int? DefaultScreenWidth { get; set; }

        [McpDescription("Default screen height in pixels.", Required = false)]
        public int? DefaultScreenHeight { get; set; }

        [McpDescription("Fullscreen mode: 'ExclusiveFullScreen', 'FullScreenWindow', 'MaximizedWindow', or 'Windowed'.", Required = false)]
        public string FullScreenMode { get; set; }

        [McpDescription("Whether the application runs in the background.", Required = false)]
        public bool? RunInBackground { get; set; }

        [McpDescription("Scripting backend: 'Mono2x' or 'IL2CPP'.", Required = false)]
        public string ScriptingBackend { get; set; }

        [McpDescription("API compatibility level: 'NET_Standard_2_0' or 'NET_Unity_4_8'.", Required = false)]
        public string ApiCompatibilityLevel { get; set; }

        [McpDescription("IL2CPP compiler configuration: 'Debug', 'Release', or 'Master'.", Required = false)]
        public string Il2CppCompilerConfiguration { get; set; }

        [McpDescription("Managed stripping level: 'Disabled', 'Low', 'Medium', 'High', or 'Minimal'.", Required = false)]
        public string ManagedStrippingLevel { get; set; }

        [McpDescription("Semicolon-separated scripting define symbols (replaces all existing defines).", Required = false)]
        public string ScriptingDefineSymbols { get; set; }

        [McpDescription("Whether to allow unsafe C# code.", Required = false)]
        public bool? AllowUnsafeCode { get; set; }

        [McpDescription("Whether to use incremental garbage collection.", Required = false)]
        public bool? GcIncremental { get; set; }
    }
}
