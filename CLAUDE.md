# Claude Development Notes

This document contains learnings and conventions discovered during the development of Apparatus.Results.

## Project Structure

```
Apparatus.Results/
├── src/
│   ├── Apparatus.Results/          # Main library
│   │   ├── Error.cs               # Base error class
│   │   ├── Result.cs              # Result<T> implementation
│   │   └── AsyncResult.cs         # Async extensions
│   └── Examples/                  # Example console app
│       ├── Program.cs
│       └── Results.csproj
├── tests/
│   └── Apparatus.Results.Tests/   # Unit tests
│       ├── ResultTests.cs
│       ├── AsyncResultTests.cs
│       ├── ErrorTests.cs
│       ├── PatternMatchingTests.cs
│       ├── ModuleInitializer.cs   # Verify configuration
│       └── ResultJsonConverter.cs # Custom Verify serializer
├── .github/
│   └── workflows/
│       └── dotnet.yml              # CI/CD pipeline
├── pack.ps1                        # Build and pack script
├── README.md
├── LICENSE
└── Apparatus.Results.sln
```

## Build Commands

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage" --verbosity normal

# Run example
dotnet run --project src/Examples

# Create NuGet package (using pack script)
pwsh ./pack.ps1 -version "1.0.0"

# Create NuGet package (direct command)
dotnet pack src/Apparatus.Results
```

## C# Method Naming Conventions

We chose C#-friendly method names over functional programming names:

| Functional Name | C# Name | Purpose |
|----------------|---------|---------|
| `Map` | `Select` | Transform success values |
| `Bind` | `SelectMany` | Chain operations |
| `Tap` | `Do` | Side effects on success |
| `TapError` | `DoOnError` | Side effects on error |

For async extensions, we use the same names without "Async" suffix:
- `SelectAsync` → `Select` (overloaded for async)
- `SelectManyAsync` → `SelectMany` (overloaded for async)
- `DoAsync` → `Do` (overloaded for async)

## Testing Patterns with Verify

### Use `await Verify(result)` for simple objects
```csharp
[Fact]
public async Task Success_CreateSuccessfulResult_ShouldHaveCorrectProperties()
{
    var result = Result<string>.Success("test value");
    await Verify(result);
}
```

### Use `await Verify(new { ... })` when testing specific properties
```csharp
[Fact]
public async Task Do_SuccessfulResult_ShouldExecuteActionAndReturnOriginalResult()
{
    var executed = false;
    var capturedValue = 0;
    var result = Result<int>.Success(42);
    
    var returned = result.Do(value =>
    {
        executed = true;
        capturedValue = value;
    });

    await Verify(new { executed, capturedValue, returned });
}
```

### Use `return Verify(...)` for non-async tests
```csharp
[Fact]
public Task Error_Properties_ShouldBeSetCorrectly()
{
    var error = new TestError("TEST_CODE", "Test message");
    return Verify(error);
}
```

## Multi-Target Framework Support

The library targets multiple .NET versions for maximum compatibility:
```xml
<TargetFrameworks>net6.0;net7.0;net8.0;net9.0</TargetFrameworks>
```

## Record-based Error Design

Errors inherit from an abstract record:
```csharp
public abstract record Error(string Code, string Message);

// Specific error types
public record ValidationError(string Field, string Reason) 
    : Error("Validation", $"Field '{Field}' is invalid: {Reason}");
```

## Null Safety with NotNullWhen

Use `[NotNullWhen(true)]` for implicit bool conversions:
```csharp
public static implicit operator bool([NotNullWhen(true)]Error? error) => error is not null;
```

## Result Pattern Implementation

Key design decisions:
1. **Struct-based** - `Result<T>` is a readonly struct for performance
2. **Implicit conversions** - Values and errors convert automatically to Results
3. **Pattern matching support** - Deconstruction and switch expressions work
4. **Async-first** - Full support for async/await patterns
5. **LINQ compatibility** - Select/SelectMany enable query syntax

## Common Usage Patterns

### Basic Result Creation
```csharp
// Success cases
Result<User> success = user;
Result<User> success2 = Result<User>.Success(user);

// Error cases  
Result<User> failure = new UserNotFound(id);
```

### Pattern Matching
```csharp
var message = GetUser(id) switch
{
    { IsSuccess: true } r => $"Hello, {r.Value.Name}",
    { Error: UserNotFound e } => $"User {e.Id} not found",
    { Error: var e } => $"Error: {e.Message}"
};
```

### Async Chaining
```csharp
var result = await GetUserAsync(id)
    .Select(user => user.Name.ToUpper())
    .SelectMany(name => ValidateNameAsync(name))
    .Do(async name => await LogAsync($"Processed: {name}"));
```

## Package Configuration

Key NuGet package settings:
- **PackageId**: `Apparatus.Results`
- **Description**: Focus on functional error handling and async support
- **Tags**: `result;error-handling;functional;async;csharp`
- **License**: MIT
- **Documentation**: Generated XML docs included

## Testing Strategy

- **Comprehensive coverage** of all Result<T> methods
- **Async operation testing** with real async/await patterns
- **Error inheritance testing** with multiple error types
- **Pattern matching scenarios** covering real-world usage
- **Snapshot testing** with Verify.Xunit for reliable assertions

## Performance Considerations

- Struct-based Result<T> avoids heap allocations
- Implicit conversions reduce boilerplate
- Async extensions use proper ConfigureAwait patterns
- Error types use records for structural equality

## Verify.Xunit Setup with Custom Serialization

### Custom Result Serializer
Result<T> objects require custom serialization for Verify because accessing Error property on successful results throws exceptions. We created a `ResultJsonConverter`:

```csharp
public class ResultJsonConverter : WriteOnlyJsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override void Write(VerifyJsonWriter writer, object value)
    {
        // Safe serialization that respects Result<T> state
        // Only access Value on success, only access Error on failure
    }
}
```

### Important Notes
- Always verify actual file contents rather than assuming they're correct

### Key Pipeline Features
- **PR Testing**: Tests run automatically on every PR to validate changes
- **Tag Publishing**: NuGet packages are published only when version tags are pushed
- **Multi-target Support**: Tests run against .NET 6, 7, 8, and 9
- **Local Compatibility**: Same `pack.ps1` script works locally and in CI

### PowerShell Build Script
The `pack.ps1` script provides consistent build process:

```powershell
# Clean, restore, build, and pack in one command
pwsh ./pack.ps1 -version "1.0.0"
```

Benefits:
- Consistent between local development and CI/CD
- No test execution (tests handled at pipeline level)
- Creates `./nugets/` directory with packages ready for publishing

## Testing Best Practices Learned

### Test Organization
- Separate test classes for different concerns
- Consistent naming: `[Method]_[Scenario]_Should[ExpectedBehavior]`
- Use of private record types for test-specific errors
- Mix of simple verification and complex scenario testing

### Verification Process
**Critical Learning**: Always actually read verification files!
- Don't assume generated content is correct
- Systematically review all `.verified.txt` files
- Look for empty files, malformed content, or unexpected values
- Verify outputs match test intentions, not just that tests pass

## Common Issues and Solutions

### GitHub Actions Secrets
For NuGet publishing, ensure secrets are properly configured:
- `API_KEY` secret contains your NuGet API key
- Test the entire pipeline with a test package first
- Use `--skip-duplicate` flag to handle reruns gracefully

### Test File Management
When working with Verify snapshot testing:
- Always commit both `.received.txt` and `.verified.txt` files
- Use batch operations to accept multiple verification files
- Review diffs carefully when verification files change

## Test Coverage Analysis

### Running Coverage Reports
Always run test coverage when adding new tests to ensure complete coverage:

```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage" --verbosity normal

# Coverage files are generated in TestResults/{guid}/coverage.cobertura.xml
```

### Coverage Goals
- **Line Coverage**: Target 100%
- **Branch Coverage**: Target 100%
- **Missing Coverage**: Investigate any uncovered code paths

### Coverage Report Analysis
The coverage report shows:
```xml
<coverage line-rate="1" branch-rate="1" version="1.9" timestamp="..." lines-covered="93" lines-valid="93" branches-covered="24" branches-valid="24">
```

Key metrics to check:
- `line-rate="1"` = 100% line coverage ✅
- `branch-rate="1"` = 100% branch coverage ✅
- `lines-covered/lines-valid` = Line coverage ratio
- `branches-covered/branches-valid` = Branch coverage ratio

### Adding Tests for Missing Coverage
When coverage is incomplete:

1. **Identify Missing Paths**: Look at the coverage report to find uncovered lines/branches
2. **Analyze the Code**: Understand what scenarios aren't tested
3. **Add Targeted Tests**: Create tests specifically for uncovered paths
4. **Consider Exception Paths**: Don't forget defensive programming exceptions
5. **Re-run Coverage**: Verify the new tests achieve 100% coverage

### Coverage Best Practices
- Run coverage analysis after adding any new functionality
- Always achieve 100% coverage before considering a feature complete
- Test both success and failure paths
- Include defensive programming exception paths
- Use coverage reports to identify missed edge cases