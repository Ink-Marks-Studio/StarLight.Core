# Git Commit Message Conventions

To ensure consistency and clarity in commit messages, we follow the commit message conventions outlined below. Please
use the following format for writing commit messages.

## Commit Message Format

A commit message should include three parts: title (required), body (optional), and footer (optional). Please refer to
the following template:

```
<type>: <subject>

<body>

<footer>
```

### 1. Title

The title briefly describes the content of the commit and should not exceed 50 characters.

Format:

```
<type>: <subject>
```

- **type**: Represents the type of the commit, such as (capitalize the first letter for aesthetics):
    - `Feat`: New feature
    - `Fix`: Bug fix
    - `Docs`: Documentation
    - `Style`: Formatting
    - `Refactor`: Refactoring
    - `Test`: Adding tests
    - `Build`: Build-related changes
    - `Ci`: Continuous integration
    - `Chore`: Other changes
    - `Revert`: Rollback
    - `Wip`: Work in progress
- **subject**: A concise description of the commit

Examples:

```
Feat: Add user login functionality
Fix: Correct calculation error in invoice module
Docs: Update installation instructions in README
```

### 2. Body

The body section provides a detailed description of the commit’s content, purpose, and impact. There is no length limit,
and it should be written in paragraphs.

The body should include:

- **Why**: Explain why this change is being made and the motivation behind it
- **How**: Describe how the change was implemented
- **What**: Specify what was changed or added

Examples:

```
Feat: Add user login functionality

- Implemented JWT-based login feature
- Added login form component
- Created authentication service for handling login requests
- Updated user model to include authentication token
```

### 3. Footer

The footer section is used for additional information, such as related tasks or bugs, and any breaking changes.

- **Related Issues**: Use keywords like `Closes`, `Fixes`, `Resolves` to automatically link issues
- **Breaking Changes**: Describe any breaking changes (e.g., migrations, upgrades)

Examples:

```
Closes #123
BREAKING CHANGE: The user model schema has been updated; please run migrations
```

### Commit Types Explanation

- **Feat**: New feature
- **Fix**: Bug fix
- **Docs**: Documentation changes
- **Style**: Code formatting changes that do not affect logic
- **Refactor**: Code refactoring that does not add new features or fix bugs
- **Test**: Adding or modifying tests
- **Build**: Changes related to build system or external dependencies
- **Ci**: Continuous integration-related changes
- **Chore**: Miscellaneous changes
- **Revert**: Rollback
- **Wip**: Work in progress

## Examples

Here are some examples of conforming commit messages:

```
Feat: Add user login functionality

- Implemented JWT-based login feature
- Added login form component
- Created authentication service for handling login requests
- Updated user model to include authentication token

Closes #123
```

```
Fix: Correct calculation error in invoice module

- Adjusted invoice total calculation logic
- Added unit tests to cover new calculation logic

Fixes #456
```

```
Docs: Update installation instructions in README

- Added step-by-step installation instructions
- Included screenshots for clarity
```
