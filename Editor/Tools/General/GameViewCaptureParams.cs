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

        /// <summary>
        /// Maximum width or height in pixels. If the captured image exceeds this
        /// in either dimension it is downscaled (aspect-ratio preserved) before
        /// PNG encoding. Set to 0 to disable and return the full-resolution capture.
        /// </summary>
        [McpDescription(
            "Maximum width or height in pixels. Images larger than this are downscaled " +
            "(aspect-ratio preserved) before encoding. Set to 0 to disable. Default: 1920.",
            Required = false, Default = 1920)]
        public int MaxDimension { get; set; } = 1920;
    }
}
