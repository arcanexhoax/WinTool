---
description: "Reviews staged, unstaged, or post-tag code changes with findings-first output"
name: "code-review"
argument-hint: "[staged|unstaged|after-last-tag]"
agent: "agent"
---

Review the requested git changes in this repository.

Input:
- Optional single argument: `staged`, `unstaged`, or `after-last-tag`.
- If no argument is provided, review both staged and unstaged changes.

Argument handling:
1. Normalize the argument to lowercase.
2. If the argument is empty, set the review scope to both staged and unstaged changes.
3. If the argument is `staged`, review only staged changes.
4. If the argument is `unstaged`, review only unstaged changes.
5. If the argument is `after-last-tag`, review the diff from the latest reachable tag to `HEAD`.
6. If the argument has any other value, stop and briefly explain that only `staged`, `unstaged`, and `after-last-tag` are supported.

Workflow:
1. Determine the repository root and inspect the requested git diff scope:
   - staged: `git diff --staged --name-only` and `git diff --staged`
   - unstaged: `git diff --name-only` and `git diff`
   - both: run both staged and unstaged commands and treat them as separate scopes in the review
   - after-last-tag:
     1. Resolve the latest reachable tag with `git describe --tags --abbrev=0`.
     2. If no tag is found, stop and briefly explain that `after-last-tag` requires at least one reachable tag.
     3. Inspect `git diff <latest-tag>..HEAD --name-only` and `git diff <latest-tag>..HEAD`; this represents all changes starting after the latest tag.
2. Review only the changed hunks plus the minimum nearby context needed to judge correctness and regression risk.
3. Prioritize bugs, behavioral regressions, missing validation, null-safety issues, state-management mistakes, performance problems in hot paths, security problems, and missing or weak tests.
4. Keep the response findings-first:
   - list findings ordered by severity with file and line references;
   - for every finding, include a concrete suggestion for how to fix or mitigate the issue;
   - keep any overall summary brief and only after the findings;
   - if there are no findings, say that explicitly and mention residual risks or testing gaps.
5. When both staged and unstaged changes are reviewed, clearly label which scope each finding belongs to. When reviewing `after-last-tag`, mention the resolved tag in the review summary.

Output requirements:
- Write the review in Ukrainian.
- Use the repository review style: findings first, brief summary second.
- Do not propose speculative issues; every finding must be grounded in the actual diff and nearby code.
- Mention when a conclusion is based on an assumption caused by missing context.
- Ignore generated files, lockfiles, and binary assets unless they directly affect correctness or reveal a concrete issue worth reporting.
