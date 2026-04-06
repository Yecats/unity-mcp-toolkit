using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetQualitySettings MCP tool.
    /// All properties are optional — only supplied values are applied to the target quality level.
    /// </summary>
    public record SetQualitySettingsParams
    {
        [McpDescription("Quality level index to modify (0-based). If omitted, modifies the active level.", Required = false)]
        public int? QualityLevelIndex { get; set; }

        [McpDescription("Set a different quality level as active (0-based index).", Required = false)]
        public int? SetActiveLevel { get; set; }

        [McpDescription("Shadow quality: 'Disable', 'HardOnly', or 'All'.", Required = false)]
        public string Shadows { get; set; }

        [McpDescription("Shadow resolution: 'Low', 'Medium', 'High', or 'VeryHigh'.", Required = false)]
        public string ShadowResolution { get; set; }

        [McpDescription("Maximum shadow rendering distance.", Required = false)]
        public float? ShadowDistance { get; set; }

        [McpDescription("Number of shadow cascades (1, 2, or 4).", Required = false)]
        public int? ShadowCascades { get; set; }

        [McpDescription("Maximum number of pixel lights that can affect a GameObject.", Required = false)]
        public int? PixelLightCount { get; set; }

        [McpDescription("Anti-aliasing sample count (0 = disabled, 2, 4, or 8).", Required = false)]
        public int? AntiAliasing { get; set; }

        [McpDescription("Whether soft particles are enabled.", Required = false)]
        public bool? SoftParticles { get; set; }

        [McpDescription("Anisotropic filtering mode: 'Disable', 'Enable', or 'ForceEnable'.", Required = false)]
        public string AnisotropicFiltering { get; set; }

        [McpDescription("LOD bias multiplier. Higher values favor higher-detail LODs.", Required = false)]
        public float? LodBias { get; set; }

        [McpDescription("Maximum LOD level (0 = highest detail).", Required = false)]
        public int? MaximumLODLevel { get; set; }

        [McpDescription("VSync count (0 = off, 1 = every V blank, 2 = every second V blank).", Required = false)]
        public int? VSyncCount { get; set; }

        [McpDescription("Whether streaming mipmaps are active.", Required = false)]
        public bool? StreamingMipmapsActive { get; set; }

        [McpDescription("Streaming mipmaps memory budget in MB.", Required = false)]
        public float? StreamingMipmapsMemoryBudget { get; set; }
    }
}
