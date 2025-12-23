# Pocket Pickers - Development Guide

## Project Overview

### Core Principles
- **Performance First**: C++ handles all time-critical operations (board representation, move generation, search)
- **Clean Abstraction**: Clear separation between engine mechanics and AI logic
- **UCI Compliance**: Full Universal Chess Interface protocol implementation
- **Extensibility**: Modular design allowing easy integration with multiple chess platforms
- **Testability**: Comprehensive unit testing for all components

### Test Driven Development Principles 
- **Tests First** ALWAYS create tests with expected behavior accounting for edge cases before writing any code. 
- **Negative Tests** - Write tests that are expected to fail as well as ones that are expected to pass. It is not explicit that things that should fair will, and thus always need to be tested.
- **Don't Modify Tests** - Think very hard when writing tests. Treat it entirely as its own phase of the project that needs to be thought about and crafted. Once tests are written and finalized, they should never be changed. (Adding new tests later is fine, but should be treated as a last resort due to a missed edge case, not something to rely upon).
- **100% Coverage** - Always cover 100% of the codebase with tests. Every single function should have a test or multiple tests depending on the situation.

## Kiro System - Adaptation of Amazon's Spec-Driven Development

This project uses an adaptation of Amazon's **Kiro System** for structured feature development. The original Kiro system has been adapted to work with Claude Code through templates and workflow guidance.

### Kiro Workflow (Amazon's 3-Phase Approach)
1. **Requirements** (`requirements.md`) - What needs to be built
2. **Design** (`design.md`) - How it will be built  
3. **Tasks** (`tasks.md`) - Step-by-step implementation plan

### Directory Structure
- `.kiro/specs/{feature-name}/` - Individual feature specifications
- `.kiro/kiro-system-templates/` - Templates and documentation
	- `requirements_template.md` - Template for requirements
	- `design_template.md` - Template for technical design
	- `tasks_template.md` - Template for implementation tasks
	- `how_kiro_works.md` - Detailed Kiro documentation

### How Claude Code Should Work with Kiro

#### When Asked to Create New Features:
1. **Check for existing specs first**: Look in `.kiro/specs/` for any existing feature documentation
2. **Use templates**: Copy templates from `.kiro/kiro-system-templates/` when creating new specs
3. **Follow the 3-phase process**: Requirements → Design → Tasks → Implementation
4. **Require approval**: Each phase needs explicit user approval before proceeding

#### Template Usage:
- **Requirements**: Use `requirements_template.md` to create user stories and EARS acceptance criteria
- **Design**: Use `design_template.md` for technical architecture and component design
- **Tasks**: Use `tasks_template.md` to break down implementation into numbered, actionable tasks

#### During Implementation:
- **Reference requirements**: Always link tasks back to specific requirements
- **Work incrementally**: Implement tasks one at a time, not all at once
- **Validate against specs**: Ensure implementations match the design and requirements
- **Update documentation**: Keep specs updated if changes are needed

#### Key Behaviors:
- **Always suggest using Kiro** when user wants to build new features
- **Guide through templates** if user is unfamiliar with the process
- **Enforce the approval process** - don't skip phases
- **Maintain traceability** from requirements to code

## Claude / Codex Collaboration

You MUST use Codex as a code review agent during task development. Read .claude/CLAUDE_CODEX.md for further instructions on how to utilize CODEX.

## Development Standards

### Code Quality
- **Comprehensive testing**: Unit tests for all components

### Performance Guidelines
- **Profiling**: Regular performance measurement
- **Optimization**: Profile-guided optimization
- **Memory**: Minimize allocations in search
- **Cache-friendly**: Data structure layout optimization

### Documentation
- **Doxygen**: API documentation for all public interfaces
- **README**: Clear setup and usage instructions
- **Architecture**: High-level design documentation
- **Performance**: Benchmarking results and targets

## Development Philosophy

This project prioritizes **elegant code architecture** and **distinctive playing style** over pure engine strength. Every component should be:
- **Well-tested**: Comprehensive unit and integration tests
- **Clearly documented**: Self-explanatory code with good comments. Comment the WHY. NOT the what - unless it is absolutely necessary for human readability
- **Performance-conscious**: Maximum efficiency - as optimized as possible. ALWAYS consider if operations or entire systems could be optimized more, both at the bit and architectural level.
- **Modular**: Easy to extend and modify