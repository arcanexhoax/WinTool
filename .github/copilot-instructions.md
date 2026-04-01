# Copilot Instructions

## General Guidelines
- In every response, explicitly state the active skill as `Skill: <name>`; if no skill applies, write `Skill: none`.
- Always respond in chat in the language of the user request.

## Skills Usage
- Use the `code-review-csharp` skill during code reviews.
- Use the `concise-planning` skill during planning tasks.
- Use the `csharp-xunit` skill during C# testing tasks.
- Use the `github-actions-templates` skill during work with GitHub Actions workflow files.
- Always use the `dotnet-wpf` skill when working with code.

## Build
- When building WinTool, follow [building.md](building.md).

## XAML Style
- When editing XAML, follow [xaml-style.md](xaml-style.md).

## Session Initialization
- At the start of each session, inspect the whole project structure to understand the solution layout and related modules.
- At the start of each session, verify the target .NET version and C# language version from project files, and keep code compatible.

## First Steps Before Coding
- Skip project structure and version checks for each task; rely on session initialization.

## User Style and Naming
- Always write code in the user's existing style.
- Do not rename or "fix" user naming for variables, methods, classes, or files unless explicitly requested.
- If there is a mistake or a better approach, implement the requested change first, then mention it in a short summary.

## .editorconfig Rules
- Follow `.editorconfig` when writing new code.
- Do not refactor unrelated existing code only to match `.editorconfig`; mention notable style mismatches in the summary instead.

## Code Shape
- Keep code concise and clear.
- Avoid unnecessary checks; add only checks that are needed for correctness.

## Change Scope
- Make the minimum amount of changes needed for the requested task.
- Expand the scope only when extra changes are required for correctness, compilation, or runtime behavior.
