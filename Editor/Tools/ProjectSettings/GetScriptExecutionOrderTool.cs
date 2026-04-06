using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads the Unity Script Execution Order.
    /// Lists all scripts with non-default execution order values,
    /// sorted by order (lowest runs first).
    /// </summary>
    public static class GetScriptExecutionOrderTool
    {
        const string ToolName = "McpToolkit.GetScriptExecutionOrder";

        const string Description =
            "Reads the Unity Script Execution Order and returns all scripts that have a " +
            "non-default (non-zero) execution order. Each entry includes the script name, " +
            "fully qualified class name, asset path, and order value. Scripts with negative " +
            "order run before default, positive run after. Sorted by execution order.";

        const string Title = "Read Script Execution Order";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand()
        {
            try
            {
                var allScripts = MonoImporter.GetAllRuntimeMonoScripts();
                var entries = new List<object>();

                foreach (var script in allScripts)
                {
                    if (script == null) continue;

                    int order = MonoImporter.GetExecutionOrder(script);
                    if (order == 0) continue;

                    var scriptClass = script.GetClass();
                    string assetPath = AssetDatabase.GetAssetPath(script);

                    entries.Add(new
                    {
                        scriptName = script.name,
                        className = scriptClass != null ? scriptClass.FullName : "(unknown)",
                        assetPath,
                        executionOrder = order
                    });
                }

                // Sort by execution order (lowest first)
                entries.Sort((a, b) =>
                {
                    // Use reflection-free approach: cast to dynamic or use a helper
                    // Since we're using anonymous types, we need a workaround
                    int orderA = GetOrder(a);
                    int orderB = GetOrder(b);
                    return orderA.CompareTo(orderB);
                });

                var data = new
                {
                    totalScriptsWithCustomOrder = entries.Count,
                    scripts = entries
                };

                return Response.Success(
                    $"Script Execution Order retrieved: {entries.Count} scripts have custom execution order.",
                    data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_SCRIPT_EXECUTION_ORDER_FAILED: {ex.Message}");
            }
        }

        static int GetOrder(object entry)
        {
            // Anonymous types have a property named executionOrder
            var prop = entry.GetType().GetProperty("executionOrder");
            return prop != null ? (int)prop.GetValue(entry) : 0;
        }
    }
}
