using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads the Scene View camera state including pivot, rotation,
    /// zoom, projection mode, 2D mode, gizmos, lighting, grid, and camera mode.
    /// Operates on the last active Scene View window.
    /// </summary>
    public static class GetSceneViewCameraTool
    {
        const string ToolName = "McpToolkit.GetSceneViewCamera";

        const string Description =
            "Reads the Scene View camera state including pivot position, rotation, zoom (size), " +
            "camera distance, orthographic/perspective mode, 2D mode, gizmos visibility, scene " +
            "lighting, grid visibility, draw mode, and camera settings (FOV, clip planes, speed). " +
            "Returns data from the last active Scene View window.";

        const string Title = "Read Scene View Camera";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Scene View" })]
        public static object HandleCommand()
        {
            try
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null)
                    return Response.Error("NO_SCENE_VIEW: No Scene View window is open.");

                var rot = sceneView.rotation;
                var euler = rot.eulerAngles;
                var camSettings = sceneView.cameraSettings;
                var viewState = sceneView.sceneViewState;

                var data = new
                {
                    // Camera transform
                    pivot = new { x = sceneView.pivot.x, y = sceneView.pivot.y, z = sceneView.pivot.z },
                    rotation = new { x = euler.x, y = euler.y, z = euler.z },
                    size = sceneView.size,
                    cameraDistance = sceneView.cameraDistance,

                    // Projection
                    orthographic = sceneView.orthographic,
                    in2DMode = sceneView.in2DMode,

                    // Visibility toggles
                    drawGizmos = sceneView.drawGizmos,
                    sceneLighting = sceneView.sceneLighting,
                    showGrid = sceneView.showGrid,
                    isRotationLocked = sceneView.isRotationLocked,

                    // Draw mode
                    cameraMode = new
                    {
                        drawMode = sceneView.cameraMode.drawMode.ToString(),
                        name = sceneView.cameraMode.name,
                        section = sceneView.cameraMode.section
                    },

                    // Camera settings
                    cameraSettings = new
                    {
                        fieldOfView = camSettings.fieldOfView,
                        nearClip = camSettings.nearClip,
                        farClip = camSettings.farClip,
                        dynamicClip = camSettings.dynamicClip,
                        occlusionCulling = camSettings.occlusionCulling,
                        speed = camSettings.speed,
                        speedMin = camSettings.speedMin,
                        speedMax = camSettings.speedMax
                    },

                    // Rendering state
                    sceneViewState = new
                    {
                        fogEnabled = viewState.fogEnabled,
                        skyboxEnabled = viewState.skyboxEnabled,
                        flaresEnabled = viewState.flaresEnabled,
                        imageEffectsEnabled = viewState.imageEffectsEnabled,
                        particleSystemsEnabled = viewState.particleSystemsEnabled,
                        alwaysRefresh = viewState.alwaysRefresh
                    }
                };

                return Response.Success("Scene View camera state retrieved.", data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_SCENE_VIEW_CAMERA_FAILED: {ex.Message}");
            }
        }
    }
}
