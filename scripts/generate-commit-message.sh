#!/bin/bash

# Generate conventional commit message from git changes
# Usage: ./scripts/generate-commit-message.sh

# Get git diff summary
ADDED=$(git diff --cached --name-only --diff-filter=A | wc -l)
MODIFIED=$(git diff --cached --name-only --diff-filter=M | wc -l)
DELETED=$(git diff --cached --name-only --diff-filter=D | wc -l)
RENAMED=$(git diff --cached --name-only --diff-filter=R | wc -l)

# Get list of changed files for context
CHANGED_FILES=$(git diff --cached --name-only | head -10 | sed 's/^/  - /')

# Determine commit type based on changes
if git diff --cached --name-only | grep -qE '\.md$|README|CHANGELOG|docs/'; then
    TYPE="docs"
elif git diff --cached --name-only | grep -qE '.*\.Tests\.csproj|.*Tests\.cs$'; then
    TYPE="test"
elif git diff --cached --name-only | grep -qE '\.editorconfig|\.gitignore|scripts/'; then
    TYPE="chore"
elif git diff --cached --name-only | grep -qE 'fix:|Bug'; then
    TYPE="fix"
else
    # Default based on what changed
    if [ "$MODIFIED" -gt 0 ]; then
        TYPE="refactor"
    elif [ "$ADDED" -gt 0 ]; then
        TYPE="feat"
    else
        TYPE="chore"
    fi
fi

# Generate descriptive message
if [ "$MODIFIED" -gt 0 ] && [ "$ADDED" -eq 0 ]; then
    DESCRIPTION="update pheromone system and ant behavior logic"
elif [ "$ADDED" -gt 0 ]; then
    DESCRIPTION="add new features and improvements"
else
    DESCRIPTION="make code quality and structure improvements"
fi

# Build the full message
COMMIT_MESSAGE="$TYPE: $DESCRIPTION

Files changed:
  Added: $ADDED
  Modified: $MODIFIED
  Deleted: $DELETED
  Renamed: $RENAMED

Changed files:
$CHANGED_FILES"

echo "$COMMIT_MESSAGE"
