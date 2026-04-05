# Changelog

## [0.1.0] - 2026-04-05

### Added
- `Custom.GameViewCapture` — Captures the Game View as a base64-encoded PNG. Supports a resolution multiplier parameter (1-4x the native Game View size). Automatically accounts for OS display scaling (e.g. 125%, 150%, Retina) so the captured image matches the actual pixel dimensions of the Game View. Works in Edit and Play mode.
- `Custom.ForceDomainRefresh` — Forces a domain reload even when Unity is unfocused.
