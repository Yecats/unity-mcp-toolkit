using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetPhysicsSettings MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// </summary>
    public record SetPhysicsSettingsParams
    {
        // --- Physics 3D ---

        [McpDescription("3D gravity X component.", Required = false)]
        public float? GravityX { get; set; }

        [McpDescription("3D gravity Y component (default: -9.81).", Required = false)]
        public float? GravityY { get; set; }

        [McpDescription("3D gravity Z component.", Required = false)]
        public float? GravityZ { get; set; }

        [McpDescription("3D default contact offset.", Required = false)]
        public float? DefaultContactOffset { get; set; }

        [McpDescription("3D bounce threshold. Collisions below this velocity won't bounce.", Required = false)]
        public float? BounceThreshold { get; set; }

        [McpDescription("3D sleep threshold. Rigidbodies below this energy will sleep.", Required = false)]
        public float? SleepThreshold { get; set; }

        [McpDescription("3D default solver position iterations.", Required = false)]
        public int? DefaultSolverIterations { get; set; }

        [McpDescription("3D default solver velocity iterations.", Required = false)]
        public int? DefaultSolverVelocityIterations { get; set; }

        [McpDescription("3D maximum angular speed for rigidbodies (radians/sec).", Required = false)]
        public float? DefaultMaxAngularSpeed { get; set; }

        [McpDescription("Whether 3D raycasts/overlap queries hit triggers.", Required = false)]
        public bool? QueriesHitTriggers { get; set; }

        [McpDescription("Whether 3D raycasts hit backface triangles.", Required = false)]
        public bool? QueriesHitBackfaces { get; set; }

        [McpDescription("Whether to auto-sync transforms before 3D physics queries.", Required = false)]
        public bool? AutoSyncTransforms { get; set; }

        // --- Physics 2D ---

        [McpDescription("2D gravity X component.", Required = false)]
        public float? Gravity2DX { get; set; }

        [McpDescription("2D gravity Y component (default: -9.81).", Required = false)]
        public float? Gravity2DY { get; set; }

        [McpDescription("2D default contact offset.", Required = false)]
        public float? DefaultContactOffset2D { get; set; }

        [McpDescription("2D velocity solver iterations.", Required = false)]
        public int? VelocityIterations2D { get; set; }

        [McpDescription("2D position solver iterations.", Required = false)]
        public int? PositionIterations2D { get; set; }

        [McpDescription("Whether 2D raycasts/overlap queries hit triggers.", Required = false)]
        public bool? QueriesHitTriggers2D { get; set; }

        [McpDescription("Whether 2D queries detect colliders at the query start point.", Required = false)]
        public bool? QueriesStartInColliders2D { get; set; }

        [McpDescription("Whether to auto-sync transforms before 2D physics queries.", Required = false)]
        public bool? AutoSyncTransforms2D { get; set; }
    }
}
