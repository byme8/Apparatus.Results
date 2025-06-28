# Create Release Command

This command creates a new GitHub release with automated version detection, release notes generation, and proper tagging.

## Description
Interactive workflow to create a new GitHub release with automatic version suggestion, commit-based release notes, and proper GitHub Actions integration for NuGet publishing.

## Instructions

### 1. Safety Checks
- Run `dotnet test` to ensure all tests pass
- Run `dotnet build` to ensure the solution builds successfully
- Check for uncommitted changes (warn if any exist)

### 2. Version Detection and Selection
- Get the latest version tag using: `git tag --list "v*" --sort=-version:refname | head -1`
- If no tags exist, suggest starting with `v1.0.0`
- Parse the current version and suggest next versions:
  - **Patch**: Increment patch number (e.g., v1.0.0 → v1.0.1)
  - **Minor**: Increment minor number (e.g., v1.0.0 → v1.1.0)  
  - **Major**: Increment major number (e.g., v1.0.0 → v2.0.0)
- Present options to user and allow custom version input
- Validate version format (must start with 'v' and follow semantic versioning)

### 3. Release Type Selection
- Ask user if this is a preview/prerelease version
- If preview: append `-preview`, `-alpha`, `-beta`, or `-rc` suffix
- Set appropriate flags for GitHub release creation

### 4. Generate Release Notes
- Get commits between latest tag and HEAD using: `git log <latest-tag>..HEAD --format="%h %s (by %an)" --no-merges`
- Parse commit messages and categorize:
  - **Features**: Commits starting with "Add", "Implement", "Create"
  - **Bug Fixes**: Commits starting with "Fix", "Resolve", "Correct"
  - **Improvements**: Commits starting with "Update", "Improve", "Enhance"
  - **Other**: All other commits
- Include contributor information for each commit
- Generate structured release notes in markdown format with contributor credits
- Show generated notes to user for review and editing
- Include a "Contributors" section listing all unique contributors for the release

### 5. User Confirmation
- Display all information:
  - Proposed version tag
  - Release type (stable/prerelease)
  - Generated release notes
  - List of commits to be included
- Ask for final confirmation before proceeding

### 6. Create GitHub Release
- Use `gh release create` with appropriate flags:
  - `--prerelease` if it's a preview version
  - `--generate-notes` as fallback if custom notes are empty
  - Include the generated/edited release notes
- The release creation will trigger GitHub Actions pipeline for:
  - Running tests across all target frameworks
  - Building NuGet packages
  - Publishing to NuGet registry

## Usage
When you're ready to create a new release, use this command to:
1. Ensure code quality with automated testing
2. Get intelligent version suggestions based on semantic versioning
3. Generate comprehensive release notes from commit history
4. Create properly tagged GitHub releases that trigger automated publishing

## Safety Checks
- All tests must pass before release creation
- Build must succeed across all target frameworks
- User confirmation required before creating the release
- Version tags are validated for proper semantic versioning format
- Warns about uncommitted changes that won't be included in the release

## Release Notes Template

Here's an example of how the generated release notes should look:

```markdown
## Release Notes for v1.2.0

### Features
- **Add async result validation methods** (`a1b2c3d`)
  - Added ValidateAsync extension methods for Result<T>
  - Support for custom async validation predicates
  - Improved error handling for validation failures
  - Contributed by: john-doe

- **Implement result caching functionality** (`e4f5g6h`)
  - Added memory-based result caching
  - Configurable cache expiration policies
  - Thread-safe cache implementation
  - Contributed by: jane-smith

### Bug Fixes
- **Fix null reference in error handling** (`i7j8k9l`)
  - Resolved NullReferenceException in Error.ToString()
  - Added defensive null checks
  - Improved error message formatting
  - Contributed by: bob-wilson

### Improvements
- **Update XML documentation for better IntelliSense** (`m1n2o3p`)
  - Enhanced method documentation with examples
  - Added parameter descriptions for all public methods
  - Improved code completion experience
  - Contributed by: alice-brown

---
```

## GitHub Actions Integration
Once the release is created, the existing GitHub Actions workflow will automatically:
- Run comprehensive tests on all target frameworks (.NET 6, 7, 8, 9)
- Build and pack NuGet packages using the `pack.ps1` script
- Publish packages to NuGet registry with the release version
- Update package metadata with release information