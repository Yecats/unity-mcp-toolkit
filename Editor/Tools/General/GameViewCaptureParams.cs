using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the Custom.GameViewCapture MCP tool.
    /// </summary>
    public record GameViewCaptureParams
    {
        /// <summary>
        /// Resolution multiplier applied to the screenshot.
        /// A value of 2 produces a screenshot 2x the native Game View size, etc.
        /// </summary>
        [McpDescription("Resolution multiplier (1-4). Higher values produce larger screenshots.", Required = false, Default = 1)]
        public int SuperSize { get; set; } = 1;
    }
}
