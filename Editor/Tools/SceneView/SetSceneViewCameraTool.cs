using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies the Scene View camera state including pivot, rotation,
    /// zoom, projection, visibility toggles, and supports LookAt/Frame actions.
    /// Operates on the last active Scene View window.
    /// </summary>
    public static class SetSceneViewCameraTool
    {
        const string ToolName = "McpToolkit.SetSceneViewCamera";

        const string Description =
            "Modifies the Scene View camera. Set pivot position, rotation, zoom (size), " +
            "orthographic/perspective mode, 2D mode, gizmos, lighting, grid visibility, " +
            "field of view, and rotation lock. Also supports LookAt (move camera to a world " +
            "point) and FrameGameObject (frame a named object). Only supplied parameters are changed.";

        const string Title = "Modify Scene View Camera";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Scene View" })]
        public static object HandleCommand(SetSceneViewCameraParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView == null)
                    return Response.Error("NO_SCENE_VIEW: No Scene View window is open.");

                var changes = new List<string>();

                // --- Direct property assignments ---

                if (parameters.PivotX.HasValue || parameters.PivotY.HasValue || parameters.PivotZ.HasValue)
                {
                    var p = sceneView.pivot;
                    if (parameters.PivotX.HasValue) p.x = parameters.PivotX.Value;
                    if (parameters.PivotY.HasValue) p.y = parameters.PivotY.Value;
                    if (parameters.PivotZ.HasValue) p.z = parameters.PivotZ.Value;
                    sceneView.pivot = p;
                    changes.Add($"pivot = ({p.x}, {p.y}, {p.z})");
                }

                if (parameters.RotationX.HasValue || parameters.RotationY.HasValue || parameters.RotationZ.HasValue)
                {
                    var euler = sceneView.rotation.eulerAngles;
                    if (parameters.RotationX.HasValue) euler.x = parameters.RotationX.Value;
                    if (parameters.RotationY.HasValue) euler.y = parameters.RotationY.Value;
                    if (parameters.RotationZ.HasValue) euler.z = parameters.RotationZ.Value;
                    sceneView.rotation = Quaternion.Euler(euler);
                    changes.Add($"rotation = ({euler.x}, {euler.y}, {euler.z})");
                }

                if (parameters.Size.HasValue)
                {
                    sceneView.size = parameters.Size.Value;
                    changes.Add($"size = {parameters.Size.Value}");
                }

                if (parameters.Orthographic.HasValue)
                {
                    sceneView.orthographic = parameters.Orthographic.Value;
                    changes.Add($"orthographic = {parameters.Orthographic.Value}");
                }

                if (parameters.In2DMode.HasValue)
                {
                    sceneView.in2DMode = parameters.In2DMode.Value;
                    changes.Add($"in2DMode = {parameters.In2DMode.Value}");
                }

                if (parameters.DrawGizmos.HasValue)
                {
                    sceneView.drawGizmos = parameters.DrawGizmos.Value;
                    changes.Add($"drawGizmos = {parameters.DrawGizmos.Value}");
                }

                if (parameters.SceneLighting.HasValue)
                {
                    sceneView.sceneLighting = parameters.SceneLighting.Value;
                    changes.Add($"sceneLighting = {parameters.SceneLighting.Value}");
                }

                if (parameters.ShowGrid.HasValue)
                {
                    sceneView.showGrid = parameters.ShowGrid.Value;
                    changes.Add($"showGrid = {parameters.ShowGrid.Value}");
                }

                if (parameters.IsRotationLocked.HasValue)
                {
                    sceneView.isRotationLocked = parameters.IsRotationLocked.Value;
                    changes.Add($"isRotationLocked = {parameters.IsRotationLocked.Value}");
                }

                if (parameters.FieldOfView.HasValue)
                {
                    sceneView.cameraSettings.fieldOfView = parameters.FieldOfView.Value;
                    changes.Add($"fieldOfView = {parameters.FieldOfView.Value}");
                }

                // --- LookAt action ---
                if (parameters.LookAtX.HasValue || parameters.LookAtY.HasValue || parameters.LookAtZ.HasValue)
                {
                    var point = sceneView.pivot; // default to current pivot
                    if (parameters.LookAtX.HasValue) point.x = parameters.LookAtX.Value;
                    if (parameters.LookAtY.HasValue) point.y = parameters.LookAtY.Value;
                    if (parameters.LookAtZ.HasValue) point.z = parameters.LookAtZ.Value;
                    sceneView.LookAtDirect(point, sceneView.rotation, sceneView.size);
                    changes.Add($"lookAt = ({point.x}, {point.y}, {point.z})");
                }

                // --- Frame GameObject action ---
                if (!string.IsNullOrEmpty(parameters.FrameGameObject))
                {
                    var go = GameObject.Find(parameters.FrameGameObject);
                    if (go == null)
                        return Response.Error($"GAMEOBJECT_NOT_FOUND: No active GameObject named '{parameters.FrameGameObject}' found.");

                    // Calculate bounds including all renderers in children
                    var bounds = new Bounds(go.transform.position, Vector3.zero);
                    var renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                        bounds.Encapsulate(r.bounds);

                    // If no renderers, use a default small bounds around the transform
                    if (renderers.Length == 0)
                        bounds = new Bounds(go.transform.position, Vector3.one);

                    sceneView.Frame(bounds, true);
                    changes.Add($"framedGameObject = '{parameters.FrameGameObject}'");
                }

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.",
                        new { changesApplied = 0 });

                // Repaint to reflect changes immediately
                sceneView.Repaint();

                return Response.Success(
                    $"Scene View camera updated: {changes.Count} change(s) applied.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_SCENE_VIEW_CAMERA_FAILED: {ex.Message}");
            }
        }
    }
}
