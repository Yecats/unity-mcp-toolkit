#if MCP_TOOLKIT_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.InputSystem;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads all Input Action Assets in the project, including their
    /// action maps, actions, bindings, and control schemes.
    /// Only available when com.unity.inputsystem is installed.
    /// </summary>
    public static class GetInputActionsTool
    {
        const string ToolName = "McpToolkit.GetInputActions";

        const string Description =
            "Reads Unity Input Action Assets (.inputactions) in the project. Returns a list of " +
            "all input action assets with their action maps, actions (name, type, bindings), " +
            "control schemes, and asset paths. Use AssetPath parameter to get details for a " +
            "specific asset, or omit it to list all assets.";

        const string Title = "Read Input Actions";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Input System" })]
        public static object HandleCommand(GetInputActionsParams parameters)
        {
            try
            {
                var guids = AssetDatabase.FindAssets("t:InputActionAsset", new[] { "Assets" });

                if (guids.Length == 0)
                    return Response.Success("No Input Action Assets found in the project.",
                        new { assetCount = 0, assets = Array.Empty<object>() });

                // If a specific asset path is requested, return detailed info for that asset
                if (parameters != null && !string.IsNullOrEmpty(parameters.AssetPath))
                    return GetAssetDetails(parameters.AssetPath);

                // Otherwise, return a summary of all assets
                var assets = new List<object>();
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                    if (asset == null) continue;

                    assets.Add(BuildAssetSummary(asset, path, guid));
                }

                return Response.Success(
                    $"Found {assets.Count} Input Action Asset(s).",
                    new { assetCount = assets.Count, assets });
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_INPUT_ACTIONS_FAILED: {ex.Message}");
            }
        }

        static object GetAssetDetails(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
            if (asset == null)
                return Response.Error($"ASSET_NOT_FOUND: No Input Action Asset found at '{assetPath}'.");

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var detail = BuildAssetDetail(asset, assetPath, guid);

            return Response.Success(
                $"Input Action Asset details for '{asset.name}'.",
                detail);
        }

        static object BuildAssetSummary(InputActionAsset asset, string path, string guid)
        {
            var maps = new List<object>();
            foreach (var map in asset.actionMaps)
            {
                maps.Add(new
                {
                    name = map.name,
                    actionCount = map.actions.Count,
                    bindingCount = map.bindings.Count
                });
            }

            return new
            {
                name = asset.name,
                assetPath = path,
                guid,
                actionMapCount = asset.actionMaps.Count,
                controlSchemeCount = asset.controlSchemes.Count,
                actionMaps = maps
            };
        }

        static object BuildAssetDetail(InputActionAsset asset, string path, string guid)
        {
            // Action maps with full action/binding details
            var maps = new List<object>();
            foreach (var map in asset.actionMaps)
            {
                var actions = new List<object>();
                foreach (var action in map.actions)
                {
                    var bindings = new List<object>();
                    foreach (var binding in action.bindings)
                    {
                        bindings.Add(new
                        {
                            name = binding.name,
                            path = binding.path,
                            interactions = binding.interactions,
                            processors = binding.processors,
                            groups = binding.groups,
                            isComposite = binding.isComposite,
                            isPartOfComposite = binding.isPartOfComposite
                        });
                    }

                    actions.Add(new
                    {
                        name = action.name,
                        id = action.id.ToString(),
                        type = action.type.ToString(),
                        expectedControlType = action.expectedControlType,
                        processors = action.processors,
                        interactions = action.interactions,
                        bindings
                    });
                }

                maps.Add(new
                {
                    name = map.name,
                    id = map.id.ToString(),
                    actions
                });
            }

            // Control schemes
            var schemes = new List<object>();
            foreach (var scheme in asset.controlSchemes)
            {
                var devices = new List<object>();
                foreach (var req in scheme.deviceRequirements)
                {
                    devices.Add(new
                    {
                        controlPath = req.controlPath,
                        isOptional = req.isOptional,
                        isOR = req.isOR
                    });
                }

                schemes.Add(new
                {
                    name = scheme.name,
                    bindingGroup = scheme.bindingGroup,
                    deviceRequirements = devices
                });
            }

            return new
            {
                name = asset.name,
                assetPath = path,
                guid,
                actionMaps = maps,
                controlSchemes = schemes
            };
        }
    }

    /// <summary>
    /// Parameters for the McpToolkit.GetInputActions MCP tool.
    /// </summary>
    public record GetInputActionsParams
    {
        [McpDescription(
            "Path to a specific Input Action Asset (e.g. 'Assets/Settings/PlayerInput.inputactions'). " +
            "If provided, returns full details for that asset. If omitted, returns a summary of all assets.",
            Required = false)]
        public string AssetPath { get; set; }
    }
}
#endif
