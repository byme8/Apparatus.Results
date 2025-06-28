# Commit and Push Command

This command verifies that everything looks good, then commits and pushes the changes.

## Description
Run tests, check build status, create a commit with proper message, and push to remote repository.

## Instructions
1. Run `dotnet test` to ensure all tests pass
2. Run `dotnet build` to ensure the solution builds successfully  
3. Check git status to see what files have changed
4. Create a commit with an appropriate message following the project's commit message style
5. Push the changes to the remote repository

## Usage
When you're ready to commit and push your changes, use this command to ensure everything is working correctly before pushing to the repository.

## Safety Checks
- All tests must pass
- Build must succeed
- Only commit staged/modified files, not untracked files unless they're part of the intended changes