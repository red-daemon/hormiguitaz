# my_commit

Automated commit workflow: analyze changes, generate conventional commit message, and execute commit script.

## Description

This skill streamlines the commit process by:
1. Analyzing staged changes with `git status` and `git diff`
2. Generating a conventional commit message using `scripts/generate-commit-message.sh`
3. Executing the automated commit workflow via `scripts/commit.sh`

## Invocation

```
/my_commit
```

## Implementation

When invoked, Claude will:

1. **Analyze staged changes** - Run `git status` to see what's staged
2. **Documentation check** - If there are code changes (not just docs):
   - Check if `/docs` directory was also modified
   - If NOT modified, offer: "Would you like to update documentation? Run `/update_docs` first"
   - User can:
     - Run `/update_docs` to update docs and stage them
     - Or continue with commit (skipping docs for now)
3. **Generate commit message** - Call `./scripts/generate-commit-message.sh` to create a conventional commit message based on file changes
4. **Execute commit workflow** - Run `./scripts/commit.sh "<message>"` which:
   - Validates message format
   - Runs `dotnet format`
   - Builds project with analyzers (Release mode)
   - Runs tests with coverage verification (70% minimum)
   - Creates commit with co-author attribution
   - Pushes to origin

## Output

The skill will display:
- ✅ Green checkmarks for successful steps
- ❌ Red errors if any validation fails
- ⚠️ Yellow warnings for non-critical issues (e.g., missing docs updates)
- 📝 Documentation status (if code changes detected)
- Coverage report with percentage
- Final confirmation when push succeeds

## Documentation Workflow

- If only `/docs` files change → proceed normally
- If code files change but `/docs` unchanged → suggest running `/update_docs`
- If code + docs both staged → proceed with commit
- Can also run `/update_docs` separately anytime to update documentation

## Related Skills

- `/update_docs` - Analyze code changes and update documentation files

## Prerequisites

- `scripts/commit.sh` - Main commit automation script (executable)
- `scripts/generate-commit-message.sh` - Commit message generator (executable)
- `.editorconfig` - Code formatting rules
- `dotnet` - .NET CLI tools
- `git` - Version control

## Notes

- The skill will abort if tests fail or coverage drops below 70%
- All changes must be staged before invoking this skill
- The commit message is automatically generated; no manual input needed
- Push is automatic after successful commit
