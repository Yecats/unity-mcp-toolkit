using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetScriptExecutionOrder MCP tool.
    /// </summary>
    public record SetScriptExecutionOrderParams
    {
        [McpDescription("The name of the script (class name, e.g. 'PlayerController'). Required.", Required = true)]
        public string ScriptName { get; set; }

        [McpDescription("The execution order value to set. Negative = before default, 0 = default, positive = after default. Required.", Required = true)]
        public int ExecutionOrder { get; set; }
    }
}
