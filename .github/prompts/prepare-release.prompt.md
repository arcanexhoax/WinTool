---
description: "Generates official release notes for a version"
name: "prepare-release"
argument-hint: "0.11.2"
agent: "agent"
---

Generate official release notes for the version passed as the only argument.

Input:
- Single argument: the version number without the `v` prefix, for example `0.11.2`.

Workflow:
1. Verify that a version argument was provided and that it matches the `major.minor.patch` format.
2. Determine the previous release version from [CHANGELOG.md](../../CHANGELOG.md):
   - if the `## v<target_version>` section already exists, use the next section below it as the previous version;
   - if the section does not exist yet, use the topmost existing section as the previous version;
   - if the previous version cannot be determined, stop and briefly explain the problem instead of inventing release notes.
3. Before generating release notes, run these commands from the repository root to determine the correct git range and inspect the commits:

```powershell
$releaseVersion = "<version from the prompt argument>"
$versions = Select-String -Path CHANGELOG.md -Pattern '^## v(?<version>\d+\.\d+\.\d+)$' | ForEach-Object { $_.Matches[0].Groups['version'].Value }
if (-not $versions) { throw "No release versions found in CHANGELOG.md" }

$targetIndex = [Array]::IndexOf($versions, $releaseVersion)
if ($targetIndex -ge 0) {
    if ($targetIndex + 1 -ge $versions.Count) { throw "Cannot determine previous version for $releaseVersion from CHANGELOG.md" }
    $previousVersion = $versions[$targetIndex + 1]
}
else {
    $previousVersion = $versions[0]
}

$tag = if (git tag --list "v$previousVersion") { "v$previousVersion" } elseif (git tag --list $previousVersion) { $previousVersion } else { throw "Git tag for previous version '$previousVersion' was not found" }

git log --reverse --format='%H%x09%s' "$tag..HEAD"
git log --reverse --stat --format='%H%n%s%n%b%n---' "$tag..HEAD"
```

4. Use only the commits after the previous tag from step 3. Open diffs only for commits that appear user-facing when needed.
5. Write short official release notes in the same style as the existing entries in [CHANGELOG.md](../../CHANGELOG.md):
   - use the `## v<version>` heading format;
   - use a simple `-` bullet list with no extra sections;
   - write in English so it matches the existing changelog;
   - include only changes that matter to ordinary users;
   - exclude internal information, secrets, noise, logs, tests, technical refactoring, or implementation details unless they affect users;
   - merge related commits into one clear bullet and avoid duplication.
6. If the target version section already exists in [CHANGELOG.md](../../CHANGELOG.md), update it; otherwise add a new section at the top of the file.
7. Apply any repository-specific updates from the repository-specific block below.
8. In the final response, briefly state:
   - which version was processed;
   - which git range was used;
   - which files were updated.

Quality rules:
- Do not invent changes that are not present in the commits.
- If the commits are mostly internal or technical work with no user-facing changes, keep the notes very short or honestly state that there are almost no user-visible changes.
- Do not change the format of existing older sections in [CHANGELOG.md](../../CHANGELOG.md).

Repository-specific rules:
- In [README.md](../../README.md), update the version badge `![Version](https://img.shields.io/badge/Version-<version>-%230c7ebf)` if needed.
- In [README.md](../../README.md), update the .NET badge `![NET](https://img.shields.io/badge/.NET%2010-%23512BD4)` if needed. Derive the displayed .NET version from the `TargetFramework` value in [src/WinTool/WinTool.csproj](../../src/WinTool/WinTool.csproj). Preserve URL encoding in the badge text, for example `.NET 10` becomes `.NET%2010`.
- In [src/WinTool/WinTool.csproj](../../src/WinTool/WinTool.csproj), update the `<Version>` value to the target version if needed.