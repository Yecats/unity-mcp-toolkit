using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies the Unity Script Execution Order for a specific script.
    /// Sets the execution order of the named MonoScript. Use order 0 to reset to default.
    /// </summary>
    public static class SetScriptExecutionOrderTool
    {
        const string ToolName = "McpToolkit.SetScriptExecutionOrder";

        const string Description =
            "Sets the execution order for a specific MonoScript by class name. Negative values " +
            "run before the default group, positive values run after. Set to 0 to reset to " +
            "default execution order. The script must exist in the project.";

        const string Title = "Modify Script Execution Order";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand(SetScriptExecutionOrderParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            if (string.IsNullOrWhiteSpace(parameters.ScriptName))
                return Response.Error("INVALID_PARAMETER: ScriptName is required.");

            try
            {
                // Find the MonoScript by name
                MonoScript targetScript = null;
                var allScripts = MonoImporter.GetAllRuntimeMonoScripts();

                foreach (var script in allScripts)
                {
                    if (script == null) continue;

                    // Match by script name (filename) or by class name
                    if (string.Equals(script.name, parameters.ScriptName, StringComparison.OrdinalIgnoreCase))
                    {
                        targetScript = script;
                        break;
                    }

                    var scriptClass = script.GetClass();
                    if (scriptClass != null &&
                        (string.Equals(scriptClass.Name, parameters.ScriptName, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(scriptClass.FullName, parameters.ScriptName, StringComparison.OrdinalIgnoreCase)))
                    {
                        targetScript = script;
                        break;
                    }
                }

                if (targetScript == null)
                    return Response.Error($"SCRIPT_NOT_FOUND: No MonoScript found matching '{parameters.ScriptName}'.");

                int previousOrder = MonoImporter.GetExecutionOrder(targetScript);
                MonoImporter.SetExecutionOrder(targetScript, parameters.ExecutionOrder);

                var scriptClass2 = targetScript.GetClass();
                string className = scriptClass2 != null ? scriptClass2.FullName : targetScript.name;

                return Response.Success(
                    $"Execution order for '{className}' changed from {previousOrder} to {parameters.ExecutionOrder}.",
                    new
                    {
                        scriptName = targetScript.name,
                        className,
                        assetPath = AssetDatabase.GetAssetPath(targetScript),
                        previousOrder,
                        newOrder = parameters.ExecutionOrder
                    });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_SCRIPT_EXECUTION_ORDER_FAILED: {ex.Message}");
            }
        }
    }
}
