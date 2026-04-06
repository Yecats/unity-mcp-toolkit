using System;
using UnityEngine;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads Unity Physics (3D) and Physics2D settings.
    /// Covers gravity, solver configuration, collision detection, and query behavior.
    /// </summary>
    public static class GetPhysicsSettingsTool
    {
        const string ToolName = "McpToolkit.GetPhysicsSettings";

        const string Description =
            "Reads Unity 3D and 2D physics settings including gravity, default contact offset, " +
            "bounce threshold, sleep threshold, solver iterations, max angular speed, collision " +
            "detection flags (queries hit triggers/backfaces), auto sync transforms, and simulation " +
            "mode. Returns both Physics (3D) and Physics2D settings.";

        const string Title = "Read Physics Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand()
        {
            try
            {
                var data = new
                {
                    physics3D = new
                    {
                        gravity = new { x = Physics.gravity.x, y = Physics.gravity.y, z = Physics.gravity.z },
                        defaultContactOffset = Physics.defaultContactOffset,
                        bounceThreshold = Physics.bounceThreshold,
                        sleepThreshold = Physics.sleepThreshold,
                        defaultSolverIterations = Physics.defaultSolverIterations,
                        defaultSolverVelocityIterations = Physics.defaultSolverVelocityIterations,
                        defaultMaxAngularSpeed = Physics.defaultMaxAngularSpeed,
                        queriesHitTriggers = Physics.queriesHitTriggers,
                        queriesHitBackfaces = Physics.queriesHitBackfaces,
                        autoSyncTransforms = Physics.autoSyncTransforms,
                        simulationMode = Physics.simulationMode.ToString()
                    },
                    physics2D = new
                    {
                        gravity = new { x = Physics2D.gravity.x, y = Physics2D.gravity.y },
                        defaultContactOffset = Physics2D.defaultContactOffset,
                        velocityIterations = Physics2D.velocityIterations,
                        positionIterations = Physics2D.positionIterations,
                        queriesHitTriggers = Physics2D.queriesHitTriggers,
                        queriesStartInColliders = Physics2D.queriesStartInColliders,
                        autoSyncTransforms = Physics2D.autoSyncTransforms,
                        simulationMode = Physics2D.simulationMode.ToString()
                    }
                };

                return Response.Success("Physics settings retrieved for both 3D and 2D.", data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_PHYSICS_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
