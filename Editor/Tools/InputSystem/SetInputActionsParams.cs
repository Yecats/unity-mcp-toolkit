#if MCP_TOOLKIT_INPUT_SYSTEM
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetInputActions MCP tool.
    /// Supports CRUD operations on action maps, actions, and bindings.
    /// </summary>
    public record SetInputActionsParams
    {
        [McpDescription(
            "Path to the Input Action Asset to modify (e.g. 'Assets/Settings/PlayerInput.inputactions'). Required.",
            Required = true)]
        public string AssetPath { get; set; }

        [McpDescription(
            "Action to perform: 'AddActionMap', 'RemoveActionMap', 'AddAction', 'RemoveAction', " +
            "'AddBinding', 'RemoveBinding', 'ReplaceJson'. Required.",
            Required = true)]
        public string Action { get; set; }

        [McpDescription("Name of the action map to add, remove, or target. Required for most actions.", Required = false)]
        public string ActionMapName { get; set; }

        [McpDescription("Name of the action to add or remove.", Required = false)]
        public string ActionName { get; set; }

        [McpDescription(
            "Action type for AddAction: 'Value', 'Button', or 'PassThrough'. Defaults to 'Button'.",
            Required = false)]
        public string ActionType { get; set; }

        [McpDescription(
            "Expected control type for AddAction (e.g. 'Vector2', 'Button', 'Axis').",
            Required = false)]
        public string ExpectedControlType { get; set; }

        [McpDescription(
            "Binding path for AddBinding (e.g. '<Keyboard>/space', '<Gamepad>/buttonSouth').",
            Required = false)]
        public string BindingPath { get; set; }

        [McpDescription("Binding group for AddBinding (e.g. 'Keyboard', 'Gamepad').", Required = false)]
        public string BindingGroups { get; set; }

        [McpDescription("Index of the binding to remove (0-based, within the action). Used for RemoveBinding.", Required = false)]
        public int? BindingIndex { get; set; }

        [McpDescription(
            "Full JSON content for ReplaceJson action. Replaces the entire asset content. " +
            "Use GetInputActions to retrieve the current JSON format.",
            Required = false)]
        public string JsonContent { get; set; }
    }
}
#endif
