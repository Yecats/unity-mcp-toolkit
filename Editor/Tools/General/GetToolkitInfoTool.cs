using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that returns information about the Unity MCP Toolkit plugin,
    /// including its version, description, and a categorized list of all
    /// available tools with their enabled status.
    ///
    /// This tool exists so AI agents can discover what the toolkit provides
    /// without requiring any project-level configuration.
    /// </summary>
    public static class GetToolkitInfoTool
    {
        const string ToolName = "McpToolkit.GetToolkitInfo";

        const string Description =
            "Returns information about the Unity MCP Toolkit community plugin, including " +
            "its version and a categorized list of every tool the toolkit provides with " +
            "each tool's name, description, type (read/write/action), and enabled status. " +
            "Call this tool first to understand what additional capabilities are available " +
            "beyond Unity's built-in MCP tools.";

        const string Title = "Get Toolkit Info";

        const string ToolkitPrefix = "McpToolkit_";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit" })]
        public static object HandleCommand()
        {
            try
            {
                // Read package version from UPM
                var packageInfo = UnityEditor.PackageManager.PackageInfo
                    .FindForAssembly(typeof(GetToolkitInfoTool).Assembly);

                string version = packageInfo?.version ?? "unknown";
                string packageName = packageInfo?.displayName ?? "Unity MCP Toolkit";

                // Discover all McpToolkit.* tools via the registry
                var allTools = McpToolRegistry.GetAllToolsForSettings();

                var toolkitTools = allTools
                    .Where(t => t.Info.name.StartsWith(ToolkitPrefix, StringComparison.Ordinal))
                    .ToList();

                // Group tools by their first group tag
                var groups = new Dictionary<string, List<object>>();

                foreach (var tool in toolkitTools)
                {
                    string group = tool.Groups != null && tool.Groups.Length > 0
                        ? tool.Groups[0]
                        : "MCP Toolkit";

                    if (!groups.ContainsKey(group))
                        groups[group] = new List<object>();

                    // Derive type from naming convention and default state
                    string toolType = ClassifyTool(tool);

                    groups[group].Add(new
                    {
                        name = tool.Info.name,
                        title = tool.Info.title,
                        description = tool.Info.description,
                        type = toolType,
                        enabled = tool.IsEnabled,
                        enabledByDefault = tool.IsDefault
                    });
                }

                // Build ordered group list
                var groupList = groups
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        group = g.Key,
                        tools = g.Value
                    })
                    .ToList();

                return Response.Success(
                    $"{packageName} v{version} — {toolkitTools.Count} tools across {groups.Count} group(s).",
                    new
                    {
                        packageName,
                        version,
                        description = "A community-driven collection of custom MCP tools that extend " +
                            "Unity's official MCP integration. Each tool adds editor automation " +
                            "capabilities that AI coding agents can use to interact with the Unity Editor. " +
                            "Write tools are disabled by default and must be enabled by the user in " +
                            "Edit > Project Settings > AI > Unity MCP > Tools.",
                        repository = "https://github.com/yecats/unity-mcp-toolkit",
                        toolCount = toolkitTools.Count,
                        groups = groupList
                    });
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_TOOLKIT_INFO_FAILED: {ex.Message}");
            }
        }

        /// <summary>
        /// Classifies a tool as Read, Write, or Action based on naming convention
        /// and EnabledByDefault state.
        /// </summary>
        static string ClassifyTool(ToolSettingsEntry tool)
        {
            string name = tool.Info.name;

            if (name.Contains(".Get"))
                return "Read";
            if (name.Contains(".Set"))
                return "Write";

            // Non-Get/Set tools that are enabled by default are actions
            return "Action";
        }
    }
}
