# Repository Guidelines

## Project Structure & Module Organization
- Core mod logic lives in `ModBehaviour.cs`, with feature modules under `Features/` and Harmony patches in `Patches/`.
- Shared helpers and settings abstractions are in `Utils/`; UI assets sit in `Utils/UI/` and `Utils/Settings/`.
- Build artefacts publish to `output/`, bundled with `info.ini` and `preview.png` for in-game deployment.
- `extracted_assets/` mirrors Unity data pulled from the game client; treat it as read-only reference material when inspecting stock behaviours or assets.

## Build, Test, and Development Commands
- `dotnet build EfDEnhanced.sln -c Release` — compiles the mod and populates `output/` via post-build copy targets.
- `dotnet build EfDEnhanced.csproj -c Debug /p:SkipCopyModFiles=true` — quick local build without copying into the live mods folder.
- `scripts/deploy.sh` — macOS helper to mirror the `output/` folder into the Steam `Mods/EfDEnhanced/` directory after a successful build.

## Coding Style & Naming Conventions
- C# code uses 4-space indentation, `var` for obvious types, and PascalCase for types/methods; private fields favour camelCase with leading underscores when persistent. Language version is 13
- Extend existing patterns for Unity event handlers (Awake/Update) and Harmony patches (prefix/postfix naming).
- Run `dotnet format` (if available in your toolkit) before submitting to align whitespace and using directives.

## Testing Guidelines
- No automated test suite exists; rely on spawning standalone builds and validating flows inside Escape from Duckov.
- Focus manual checks on raid entry/exit, wheel menus, and localization toggles—especially after touching `Features/` or `Utils/LocalizationHelper.cs`.
- Use the in-game developer console plus `ModLogger` outputs to verify new behaviour; keep logging verbose during development and scale back before submission.

## Commit & Pull Request Guidelines
- Follow Conventional Commits (`feat:`, `fix:`, `docs:`) consistent with the existing history.
- Squash work by feature area, mention affected systems (e.g., “feat: improve quest tracker refresh cadence”).
- PRs should include: summary of changes, testing notes (game version + scenarios exercised), screenshots/GIFs for UI tweaks, and links to any related Steam or issue tracker items.
- Avoid committing binaries from `output/` or modifications to `extracted_assets/`; share reproducible steps instead.

## Game Integration Tips
- Reference `extracted_assets/Scripts/**` for original method signatures before introducing Harmony patches.
- Respect the post-build copy rules: the csproj automatically mirrors DLLs and supporting files into the live mod directory when paths resolve; confirm environment paths in `EfDEnhanced.csproj` before shipping changes.
