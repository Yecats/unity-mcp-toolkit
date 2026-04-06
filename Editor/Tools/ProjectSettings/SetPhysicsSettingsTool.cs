using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies Unity Physics (3D) and Physics2D settings.
    /// Only supplied (non-null) parameters are applied — omitted values are left unchanged.
    /// </summary>
    public static class SetPhysicsSettingsTool
    {
        const string ToolName = "McpToolkit.SetPhysicsSettings";

        const string Description =
            "Modifies Unity 3D and 2D physics settings. Only supplied parameters are changed. " +
            "Supports gravity, contact offset, bounce/sleep thresholds, solver iterations, " +
            "max angular speed, query behavior (hit triggers/backfaces), and auto sync transforms.";

        const string Title = "Modify Physics Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand(SetPhysicsSettingsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var changes = new List<string>();

                // --- Physics 3D ---
                if (parameters.GravityX.HasValue || parameters.GravityY.HasValue || parameters.GravityZ.HasValue)
                {
                    var g = Physics.gravity;
                    if (parameters.GravityX.HasValue) g.x = parameters.GravityX.Value;
                    if (parameters.GravityY.HasValue) g.y = parameters.GravityY.Value;
                    if (parameters.GravityZ.HasValue) g.z = parameters.GravityZ.Value;
                    Physics.gravity = g;
                    changes.Add($"gravity3D = ({g.x}, {g.y}, {g.z})");
                }
                if (parameters.DefaultContactOffset.HasValue)
                {
                    Physics.defaultContactOffset = parameters.DefaultContactOffset.Value;
                    changes.Add($"defaultContactOffset = {parameters.DefaultContactOffset.Value}");
                }
                if (parameters.BounceThreshold.HasValue)
                {
                    Physics.bounceThreshold = parameters.BounceThreshold.Value;
                    changes.Add($"bounceThreshold = {parameters.BounceThreshold.Value}");
                }
                if (parameters.SleepThreshold.HasValue)
                {
                    Physics.sleepThreshold = parameters.SleepThreshold.Value;
                    changes.Add($"sleepThreshold = {parameters.SleepThreshold.Value}");
                }
                if (parameters.DefaultSolverIterations.HasValue)
                {
                    Physics.defaultSolverIterations = parameters.DefaultSolverIterations.Value;
                    changes.Add($"defaultSolverIterations = {parameters.DefaultSolverIterations.Value}");
                }
                if (parameters.DefaultSolverVelocityIterations.HasValue)
                {
                    Physics.defaultSolverVelocityIterations = parameters.DefaultSolverVelocityIterations.Value;
                    changes.Add($"defaultSolverVelocityIterations = {parameters.DefaultSolverVelocityIterations.Value}");
                }
                if (parameters.DefaultMaxAngularSpeed.HasValue)
                {
                    Physics.defaultMaxAngularSpeed = parameters.DefaultMaxAngularSpeed.Value;
                    changes.Add($"defaultMaxAngularSpeed = {parameters.DefaultMaxAngularSpeed.Value}");
                }
                if (parameters.QueriesHitTriggers.HasValue)
                {
                    Physics.queriesHitTriggers = parameters.QueriesHitTriggers.Value;
                    changes.Add($"queriesHitTriggers = {parameters.QueriesHitTriggers.Value}");
                }
                if (parameters.QueriesHitBackfaces.HasValue)
                {
                    Physics.queriesHitBackfaces = parameters.QueriesHitBackfaces.Value;
                    changes.Add($"queriesHitBackfaces = {parameters.QueriesHitBackfaces.Value}");
                }
                if (parameters.AutoSyncTransforms.HasValue)
                {
                    Physics.autoSyncTransforms = parameters.AutoSyncTransforms.Value;
                    changes.Add($"autoSyncTransforms3D = {parameters.AutoSyncTransforms.Value}");
                }

                // --- Physics 2D ---
                if (parameters.Gravity2DX.HasValue || parameters.Gravity2DY.HasValue)
                {
                    var g2 = Physics2D.gravity;
                    if (parameters.Gravity2DX.HasValue) g2.x = parameters.Gravity2DX.Value;
                    if (parameters.Gravity2DY.HasValue) g2.y = parameters.Gravity2DY.Value;
                    Physics2D.gravity = g2;
                    changes.Add($"gravity2D = ({g2.x}, {g2.y})");
                }
                if (parameters.DefaultContactOffset2D.HasValue)
                {
                    Physics2D.defaultContactOffset = parameters.DefaultContactOffset2D.Value;
                    changes.Add($"defaultContactOffset2D = {parameters.DefaultContactOffset2D.Value}");
                }
                if (parameters.VelocityIterations2D.HasValue)
                {
                    Physics2D.velocityIterations = parameters.VelocityIterations2D.Value;
                    changes.Add($"velocityIterations2D = {parameters.VelocityIterations2D.Value}");
                }
                if (parameters.PositionIterations2D.HasValue)
                {
                    Physics2D.positionIterations = parameters.PositionIterations2D.Value;
                    changes.Add($"positionIterations2D = {parameters.PositionIterations2D.Value}");
                }
                if (parameters.QueriesHitTriggers2D.HasValue)
                {
                    Physics2D.queriesHitTriggers = parameters.QueriesHitTriggers2D.Value;
                    changes.Add($"queriesHitTriggers2D = {parameters.QueriesHitTriggers2D.Value}");
                }
                if (parameters.QueriesStartInColliders2D.HasValue)
                {
                    Physics2D.queriesStartInColliders = parameters.QueriesStartInColliders2D.Value;
                    changes.Add($"queriesStartInColliders2D = {parameters.QueriesStartInColliders2D.Value}");
                }
                if (parameters.AutoSyncTransforms2D.HasValue)
                {
                    Physics2D.autoSyncTransforms = parameters.AutoSyncTransforms2D.Value;
                    changes.Add($"autoSyncTransforms2D = {parameters.AutoSyncTransforms2D.Value}");
                }

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.", new { changesApplied = 0 });

                return Response.Success(
                    $"Physics settings updated: {changes.Count} value(s) changed.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_PHYSICS_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
