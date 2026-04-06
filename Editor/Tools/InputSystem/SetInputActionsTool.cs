#if MCP_TOOLKIT_INPUT_SYSTEM
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies Input Action Assets (.inputactions).
    /// Supports adding/removing action maps, actions, and bindings,
    /// as well as wholesale JSON replacement.
    /// Only available when com.unity.inputsystem is installed.
    /// </summary>
    public static class SetInputActionsTool
    {
        const string ToolName = "McpToolkit.SetInputActions";

        const string Description =
            "Modifies a Unity Input Action Asset. Supports: AddActionMap, RemoveActionMap, " +
            "AddAction, RemoveAction, AddBinding, RemoveBinding, and ReplaceJson (wholesale " +
            "replacement from JSON). Requires the asset path. Changes are saved to disk. " +
            "Cannot modify assets during Play mode.";

        const string Title = "Modify Input Actions";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Input System" })]
        public static object HandleCommand(SetInputActionsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            if (string.IsNullOrEmpty(parameters.AssetPath))
                return Response.Error("INVALID_PARAMETER: AssetPath is required.");

            if (string.IsNullOrEmpty(parameters.Action))
                return Response.Error("INVALID_PARAMETER: Action is required.");

            if (EditorApplication.isPlaying)
                return Response.Error("PLAY_MODE: Cannot modify Input Action Assets during Play mode. " +
                    "Changes would be lost when exiting Play mode.");

            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(parameters.AssetPath);
                if (asset == null)
                    return Response.Error($"ASSET_NOT_FOUND: No Input Action Asset at '{parameters.AssetPath}'.");

                string result;
                switch (parameters.Action.Trim().ToLowerInvariant())
                {
                    case "addactionmap":
                        result = AddActionMap(asset, parameters);
                        break;
                    case "removeactionmap":
                        result = RemoveActionMap(asset, parameters);
                        break;
                    case "addaction":
                        result = AddAction(asset, parameters);
                        break;
                    case "removeaction":
                        result = RemoveAction(asset, parameters);
                        break;
                    case "addbinding":
                        result = AddBinding(asset, parameters);
                        break;
                    case "removebinding":
                        result = RemoveBinding(asset, parameters);
                        break;
                    case "replacejson":
                        result = ReplaceJson(asset, parameters);
                        break;
                    default:
                        return Response.Error(
                            $"UNKNOWN_ACTION: '{parameters.Action}'. Valid actions: " +
                            "AddActionMap, RemoveActionMap, AddAction, RemoveAction, " +
                            "AddBinding, RemoveBinding, ReplaceJson.");
                }

                // Save the asset to disk
                SaveAsset(asset, parameters.AssetPath);

                return Response.Success(result, new { assetPath = parameters.AssetPath });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_INPUT_ACTIONS_FAILED: {ex.Message}");
            }
        }

        static string AddActionMap(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.ActionMapName))
                throw new ArgumentException("ActionMapName is required for AddActionMap.");

            if (asset.FindActionMap(p.ActionMapName) != null)
                throw new ArgumentException($"Action map '{p.ActionMapName}' already exists.");

            asset.AddActionMap(p.ActionMapName);
            return $"Added action map '{p.ActionMapName}'.";
        }

        static string RemoveActionMap(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.ActionMapName))
                throw new ArgumentException("ActionMapName is required for RemoveActionMap.");

            var map = asset.FindActionMap(p.ActionMapName);
            if (map == null)
                throw new ArgumentException($"Action map '{p.ActionMapName}' not found.");

            asset.RemoveActionMap(map);
            return $"Removed action map '{p.ActionMapName}'.";
        }

        static string AddAction(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.ActionMapName))
                throw new ArgumentException("ActionMapName is required for AddAction.");
            if (string.IsNullOrEmpty(p.ActionName))
                throw new ArgumentException("ActionName is required for AddAction.");

            var map = asset.FindActionMap(p.ActionMapName);
            if (map == null)
                throw new ArgumentException($"Action map '{p.ActionMapName}' not found.");

            if (map.FindAction(p.ActionName) != null)
                throw new ArgumentException($"Action '{p.ActionName}' already exists in map '{p.ActionMapName}'.");

            var actionType = InputActionType.Button;
            if (!string.IsNullOrEmpty(p.ActionType))
            {
                if (!Enum.TryParse<InputActionType>(p.ActionType, true, out actionType))
                    throw new ArgumentException(
                        $"Invalid ActionType '{p.ActionType}'. Valid: Value, Button, PassThrough.");
            }

            var action = map.AddAction(p.ActionName, actionType);

            if (!string.IsNullOrEmpty(p.ExpectedControlType))
                action.expectedControlType = p.ExpectedControlType;

            // Optionally add an initial binding
            if (!string.IsNullOrEmpty(p.BindingPath))
                action.AddBinding(p.BindingPath, groups: p.BindingGroups);

            return $"Added action '{p.ActionName}' (type: {actionType}) to map '{p.ActionMapName}'." +
                (!string.IsNullOrEmpty(p.BindingPath)
                    ? $" Initial binding: {p.BindingPath}"
                    : string.Empty);
        }

        static string RemoveAction(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.ActionMapName))
                throw new ArgumentException("ActionMapName is required for RemoveAction.");
            if (string.IsNullOrEmpty(p.ActionName))
                throw new ArgumentException("ActionName is required for RemoveAction.");

            var map = asset.FindActionMap(p.ActionMapName);
            if (map == null)
                throw new ArgumentException($"Action map '{p.ActionMapName}' not found.");

            var action = map.FindAction(p.ActionName);
            if (action == null)
                throw new ArgumentException($"Action '{p.ActionName}' not found in map '{p.ActionMapName}'.");

            action.RemoveAction();
            return $"Removed action '{p.ActionName}' from map '{p.ActionMapName}'.";
        }

        static string AddBinding(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.ActionMapName))
                throw new ArgumentException("ActionMapName is required for AddBinding.");
            if (string.IsNullOrEmpty(p.ActionName))
                throw new ArgumentException("ActionName is required for AddBinding.");
            if (string.IsNullOrEmpty(p.BindingPath))
                throw new ArgumentException("BindingPath is required for AddBinding.");

            var map = asset.FindActionMap(p.ActionMapName);
            if (map == null)
                throw new ArgumentException($"Action map '{p.ActionMapName}' not found.");

            var action = map.FindAction(p.ActionName);
            if (action == null)
                throw new ArgumentException($"Action '{p.ActionName}' not found in map '{p.ActionMapName}'.");

            action.AddBinding(p.BindingPath, groups: p.BindingGroups);
            return $"Added binding '{p.BindingPath}' to action '{p.ActionName}' in map '{p.ActionMapName}'.";
        }

        static string RemoveBinding(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.ActionMapName))
                throw new ArgumentException("ActionMapName is required for RemoveBinding.");
            if (string.IsNullOrEmpty(p.ActionName))
                throw new ArgumentException("ActionName is required for RemoveBinding.");
            if (!p.BindingIndex.HasValue)
                throw new ArgumentException("BindingIndex is required for RemoveBinding.");

            var map = asset.FindActionMap(p.ActionMapName);
            if (map == null)
                throw new ArgumentException($"Action map '{p.ActionMapName}' not found.");

            var action = map.FindAction(p.ActionName);
            if (action == null)
                throw new ArgumentException($"Action '{p.ActionName}' not found in map '{p.ActionMapName}'.");

            int index = p.BindingIndex.Value;
            if (index < 0 || index >= action.bindings.Count)
                throw new ArgumentException(
                    $"BindingIndex {index} is out of range (0-{action.bindings.Count - 1}).");

            var removedPath = action.bindings[index].path;
            action.ChangeBinding(index).Erase();
            return $"Removed binding at index {index} (path: '{removedPath}') from action '{p.ActionName}'.";
        }

        static string ReplaceJson(InputActionAsset asset, SetInputActionsParams p)
        {
            if (string.IsNullOrEmpty(p.JsonContent))
                throw new ArgumentException("JsonContent is required for ReplaceJson.");

            asset.LoadFromJson(p.JsonContent);
            return $"Replaced entire asset content from JSON.";
        }

        static void SaveAsset(InputActionAsset asset, string assetPath)
        {
            var json = asset.ToJson();
            var physicalPath = FileUtil.GetPhysicalPath(assetPath);

            // Make the file writable (version control support)
            AssetDatabase.MakeEditable(assetPath);

            File.WriteAllText(physicalPath, json);
            AssetDatabase.ImportAsset(assetPath);
        }
    }
}
#endif
