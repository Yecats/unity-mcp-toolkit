# Unity MCP Toolkit

A community-driven collection of custom MCP tools that extend Unity's official MCP integration. Each tool adds editor automation capabilities that AI coding agents can use to interact with the Unity Editor.

## Requirements

- **Unity 6+** (`6000.0` or later)
- **[Unity AI Assistant](https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/index.html)** (`com.unity.ai.assistant` 2.4.0-pre.1 or later) — this is a pre-release package

## Installation

Add the following to your project's `Packages/manifest.json`:

```json
{
    "dependencies": {
        "com.whatupgames.unity-mcp-toolkit": "https://github.com/yecats/unity-mcp-toolkit.git"
    }
}
```

Or use **Window > Package Manager > + > Add package from git URL** and enter:

```
https://github.com/yecats/unity-mcp-toolkit.git
```

## Tools

| Tool | Description |
|---|---|
| `Custom.GameViewCapture` | Captures the Game View (the final rendered frame the player sees, including UI overlays and post-processing) and returns it as a base64-encoded PNG. Automatically accounts for OS display scaling (e.g. 125%, 150%, Retina). Works in Edit and Play mode. |
| `Custom.ForceDomainRefresh` | Forces a domain reload even when the Unity Editor is not in the foreground. Use after modifying scripts externally to trigger recompilation without switching to Unity. |

## Contributing

This toolkit is meant to grow over time with tools the community finds useful. If you have an idea for a tool that would make your workflow better, chances are others would benefit from it too. Contributions are welcome and encouraged!

Please keep the following in mind:

- **Follow the Unity convention for file structure.** Tool logic goes in `Editor/Tools/`, parameter records go in `Editor/Tools/Parameters/`. One file per tool, one file per params record.
- **AI-assisted contributions are welcome**, but a human must review the code and PR description before submitting. PRs that appear to be 100% vibe-coded without human review will be sent back.

## License

This project is dedicated to the public domain under the [CC0 1.0 Universal](LICENSE) license. You can copy, modify, and distribute it without asking permission or giving credit.
