using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetSceneViewCamera MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// Supports direct property setting, LookAt, and Frame operations.
    /// </summary>
    public record SetSceneViewCameraParams
    {
        // --- Camera transform ---

        [McpDescription("Pivot X position in world space.", Required = false)]
        public float? PivotX { get; set; }

        [McpDescription("Pivot Y position in world space.", Required = false)]
        public float? PivotY { get; set; }

        [McpDescription("Pivot Z position in world space.", Required = false)]
        public float? PivotZ { get; set; }

        [McpDescription("Rotation X (euler angle in degrees).", Required = false)]
        public float? RotationX { get; set; }

        [McpDescription("Rotation Y (euler angle in degrees).", Required = false)]
        public float? RotationY { get; set; }

        [McpDescription("Rotation Z (euler angle in degrees).", Required = false)]
        public float? RotationZ { get; set; }

        [McpDescription("View size (zoom level). Smaller values are more zoomed in.", Required = false)]
        public float? Size { get; set; }

        // --- Projection ---

        [McpDescription("Set to true for orthographic, false for perspective.", Required = false)]
        public bool? Orthographic { get; set; }

        [McpDescription("Enable or disable 2D mode.", Required = false)]
        public bool? In2DMode { get; set; }

        // --- Visibility toggles ---

        [McpDescription("Show or hide gizmos.", Required = false)]
        public bool? DrawGizmos { get; set; }

        [McpDescription("Enable or disable scene lighting.", Required = false)]
        public bool? SceneLighting { get; set; }

        [McpDescription("Show or hide the grid.", Required = false)]
        public bool? ShowGrid { get; set; }

        [McpDescription("Lock or unlock camera rotation.", Required = false)]
        public bool? IsRotationLocked { get; set; }

        // --- Camera settings ---

        [McpDescription("Camera field of view in degrees (perspective mode only).", Required = false)]
        public float? FieldOfView { get; set; }

        // --- LookAt action ---

        [McpDescription(
            "World-space X coordinate for LookAt. " +
            "When any LookAt coordinate is set, the camera moves to look at that point instantly.",
            Required = false)]
        public float? LookAtX { get; set; }

        [McpDescription("World-space Y coordinate for LookAt.", Required = false)]
        public float? LookAtY { get; set; }

        [McpDescription("World-space Z coordinate for LookAt.", Required = false)]
        public float? LookAtZ { get; set; }

        // --- Frame action ---

        [McpDescription(
            "Name of a GameObject to frame in the Scene View. " +
            "The camera will move and zoom to center and fit the object.",
            Required = false)]
        public string FrameGameObject { get; set; }
    }
}
