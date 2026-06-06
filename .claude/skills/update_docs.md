---
name: update_docs
---

# update_docs

Analyze code changes and update project documentation accordingly.

## Description

This skill reviews recent code changes and updates the relevant documentation files in `/docs`:

- `TECHNICAL_DESIGN.md` - Architecture, design decisions, systems
- `PRD Simulador Multi‑Agente de hormigas.md` - Product requirements and vision
- Other relevant documentation

## Invocation

```
/update_docs
```

## Implementation

When invoked, Claude will:

1. **Analyze staged changes** - Review `git diff --cached` to understand what code changed
2. **Identify impact areas** - Determine which documentation files need updates
3. **Update relevant docs** - Modify `/docs` files to reflect:
   - New features or systems added
   - API/interface changes
   - Architectural decisions
   - Performance implications
   - Configuration changes
4. **Stage changes** - Run `git add docs/` to stage all documentation updates
5. **Summary** - Show what was updated

## Output

The skill will display:
- 📝 Files analyzed
- 🔄 Documentation files updated
- ✅ Changes staged and ready for commit
- 📋 Summary of updates made

## Notes

- This skill analyzes **staged changes** (use `git add` first)
- Documentation updates are staged automatically
- You can review changes with `git diff --cached docs/`
- Run `/my_commit` after to commit both code and docs together
- Or stage additional changes and commit manually

## Usage Flow

Typical workflow:
```
1. Make code changes and stage them: git add .
2. /update_docs       # Updates and stages documentation
3. /my_commit         # Commits everything together
```
