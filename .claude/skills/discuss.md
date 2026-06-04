# discuss

Have a conversation about the project without automatic code changes or plan mode.

## Description

This skill enables conversational discussion about the Ant Simulator project:
- Ask about architecture, design decisions, and trade-offs
- Discuss implementation approaches and alternatives
- Explore ideas and brainstorm solutions
- Ask for explanations and analysis
- Get recommendations and guidance

No automatic code generation, no plan mode — just focused discussion.

## Invocation

```
/discuss <your question or topic>
```

Examples:
```
/discuss How should we handle pheromone persistence?
/discuss What's the difference between the two movement approaches?
/discuss Let's think about performance optimizations for the grid system
/discuss Should we use inheritance or composition for ant behaviors?
```

## Behavior

When you invoke `/discuss`, Claude will:

1. **Listen and understand** - Read your question or topic carefully
2. **Analyze in context** - Consider existing code, architecture, and project state
3. **Discuss thoroughly** - Provide analysis, trade-offs, alternatives, and reasoning
4. **Stay conversational** - Ask follow-up questions if needed
5. **No auto-coding** - Won't attempt to implement changes unless you explicitly ask
6. **No plan mode** - Won't enter planning/approval workflow

## What This Is Good For

- Understanding design decisions in the codebase
- Exploring "what if" scenarios
- Getting explanations of how systems work
- Discussing trade-offs and alternatives
- Brainstorming approaches to problems
- Asking for advice or recommendations
- Understanding the "why" behind architectural choices

## What This Is NOT

- Not for making changes (use `/my_commit` for that)
- Not for planning implementations (use plan mode if you want that)
- Not for step-by-step tutorials (ask directly instead)
- Not for code generation (explicit requests override this)

## Notes

- If you decide to implement something during discussion, just ask and Claude will code it
- You can reference specific files and line numbers
- Discussion can span architecture, performance, design patterns, etc.
- This is a thinking/analysis mode, not an action mode
