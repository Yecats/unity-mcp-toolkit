# Unity MCP Toolkit

Custom MCP tools that extend Unity's official MCP integration with additional editor automation capabilities.

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

| Tool | Description | Parameters |
|---|---|---|
| `Custom.GameViewCapture` | Captures the Game View (the final rendered frame the player sees, including UI overlays and post-processing) and returns it as a base64-encoded PNG. Automatically accounts for OS display scaling (e.g. 125%, 150%, Retina). Works in Edit and Play mode. | `SuperSize` (int, 1-4, default 1) — Resolution multiplier. A value of 2 produces a screenshot 2x the native Game View size. |
| `Custom.ForceDomainRefresh` | Forces a domain reload even when the Unity Editor is not in the foreground. Use after modifying scripts externally to trigger recompilation without switching to Unity. | None |

## Contributing

Contributions are welcome. Please keep the following in mind:

- **One file per tool.** Each tool and its parameter class should live in a single `.cs` file.
- **AI-assisted contributions are welcome**, but a human must review the code and PR description before submitting. PRs that are 100% vibe-coded without human review will be sent back.

## License

This project is dedicated to the public domain under the [CC0 1.0 Universal](LICENSE) license. You can copy, modify, and distribute it without asking permission or giving credit.
