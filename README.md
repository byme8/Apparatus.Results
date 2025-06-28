# Apparatus.Results

A robust Result&lt;T&gt; pattern implementation for functional error handling in C#. Provides type-safe error handling with async support and comprehensive extension methods.

## Features

- **Type-safe error handling** - No more exceptions for expected failures
- **Async/await support** - Full integration with async operations
- **Functional programming patterns** - Map, Bind, Tap operations
- **LINQ query syntax** - Use familiar LINQ operators
- **Pattern matching support** - Deconstruction and implicit conversions
- **Zero dependencies** - Lightweight and self-contained
- **Multi-targeting** - Supports .NET 6, 7, 8, and 9

## Quick Start

### Installation

```bash
dotnet add package Apparatus.Results
```

### Basic Usage

```csharp
using Apparatus.Results;

// Define your domain errors
public record UserNotFound(int UserId) 
    : Error("UserNotFound", $"User with ID {UserId} was not found");

public record InvalidEmail(string Email) 
    : Error("InvalidEmail", $"Email '{Email}' is not valid");

// Methods return Result<T> instead of throwing exceptions
public Result<User> GetUser(int id)
{
    if (id <= 0) 
        return new InvalidUserId(id);
    
    // Success case - implicit conversion
    return new User(id, "John Doe");
}

// Handle results with pattern matching
var (user, error) = GetUser(userId);
if (error)
{
    Console.WriteLine($"Error: {error.Code} - {error.Message}");
    return;
}

Console.WriteLine($"Welcome, {user.Name}!");
```

## Core Concepts

### Result&lt;T&gt;

The `Result<T>` type represents the outcome of an operation that can either succeed with a value of type `T` or fail with an `Error`.

```csharp
// Creating results
Result<string> success = Result<string>.Success("Hello World");
Result<string> failure = new ValidationError("Invalid input");

// Or use implicit conversions
Result<string> success = "Hello World";
Result<string> failure = new ValidationError("Invalid input");
```

### Error Types

All errors inherit from the abstract `Error` record:

```csharp
public record ValidationError(string Field, string Message) 
    : Error("ValidationError", $"Validation failed for {Field}: {Message}");

public record NotFoundError(string Resource, string Id) 
    : Error("NotFound", $"{Resource} with ID '{Id}' was not found");
```

### Pattern Matching

Multiple ways to handle results:

```csharp
// 1. Deconstruction
var (value, error) = GetUser(id);
if (error) 
{
    // Handle error
}

// 2. Property access
var result = GetUser(id);
if (result.IsSuccess)
{
    Console.WriteLine(result.Value.Name);
}
else
{
    Console.WriteLine(result.Error.Message);
}

// 3. Pattern matching with switch
var message = GetUser(id) switch
{
    { IsSuccess: true } r => $"Hello, {r.Value.Name}",
    { Error: UserNotFound e } => $"User {e.UserId} not found",
    { Error: var e } => $"Error: {e.Message}"
};
```

## Functional Operations

### Map - Transform Success Values

```csharp
Result<User> userResult = GetUser(id);
Result<string> nameResult = userResult.Map(user => user.Name.ToUpper());
```

### Bind - Chain Operations

```csharp
Result<User> GetUser(int id) { /* ... */ }
Result<Profile> GetProfile(User user) { /* ... */ }

var profileResult = GetUser(id)
    .Bind(user => GetProfile(user));
```

### Tap - Side Effects

```csharp
var result = GetUser(id)
    .Tap(user => logger.LogInformation($"Retrieved user: {user.Name}"))
    .TapError(error => logger.LogError($"Failed to get user: {error.Message}"));
```

## Async Support

Full async/await integration with extension methods:

```csharp
// Async operations
async Task<Result<User>> GetUserAsync(int id)
{
    await Task.Delay(100); // Simulate async work
    return new User(id, "Async User");
}

// Chain async operations
var result = await GetUserAsync(id)
    .Map(user => user.Name.ToUpper())
    .Tap(async name => await LogNameAsync(name));

// LINQ syntax
var upperName = await GetUserAsync(id)
    .Select(user => user.Name.ToUpper());
```

### Async Method Reference

| Method | Description |
|--------|-------------|
| `Map<TOut>(Func<T, TOut>)` | Transform success value synchronously |
| `Map<TOut>(Func<T, Task<TOut>>)` | Transform success value asynchronously |
| `Bind<TOut>(Func<T, Result<TOut>>)` | Chain sync operation |
| `Bind<TOut>(Func<T, Task<Result<TOut>>>)` | Chain async operation |
| `Tap(Func<T, Task>)` | Side effect on success |
| `TapError(Func<Error, Task>)` | Side effect on error |
| `Select<TOut>(Func<T, TOut>)` | LINQ map operation |

## Advanced Scenarios

### Custom Error Hierarchies

```csharp
// Base domain error
public abstract record DomainError(string Code, string Message) : Error(Code, Message);

// Specific error types
public record ValidationError(string Field, string Reason) 
    : DomainError("Validation", $"Field '{Field}' is invalid: {Reason}");

public record BusinessRuleError(string Rule) 
    : DomainError("BusinessRule", $"Business rule violated: {Rule}");

// Handle by error type
var (user, error) = CreateUser(userData);
if (error)
{
    var message = error switch
    {
        ValidationError validation => $"Fix validation: {validation.Reason}",
        BusinessRuleError business => $"Business rule: {business.Rule}",
        _ => "Unknown error occurred"
    };
}
```

### Error Aggregation

```csharp
public record AggregateError(IReadOnlyList<Error> Errors) 
    : Error("Aggregate", $"Multiple errors occurred: {string.Join(", ", Errors.Select(e => e.Code))}");

// Collect multiple validation errors
var errors = new List<Error>();
if (string.IsNullOrEmpty(name)) errors.Add(new ValidationError("Name", "Required"));
if (age < 0) errors.Add(new ValidationError("Age", "Must be positive"));

return errors.Any() ? new AggregateError(errors) : new User(name, age);
```

### Integration with Existing Code

```csharp
// Convert exceptions to Results
public static Result<T> Try<T>(Func<T> operation)
{
    try
    {
        return operation();
    }
    catch (Exception ex)
    {
        return new ExceptionError(ex.GetType().Name, ex.Message);
    }
}

// Usage
var result = Try(() => JsonSerializer.Deserialize<User>(json));
```

## Best Practices

### 1. Design Meaningful Errors

```csharp
// ❌ Generic errors
public record Error(string Message) : Error("Error", Message);

// ✅ Specific, actionable errors
public record InvalidEmailFormat(string Email) 
    : Error("InvalidEmailFormat", $"Email '{Email}' is not in valid format");
```

### 2. Use Implicit Conversions

```csharp
// ❌ Verbose
return Result<User>.Success(user);

// ✅ Concise
return user;
```

### 3. Chain Operations

```csharp
// ❌ Nested if statements
var userResult = GetUser(id);
if (userResult.IsSuccess)
{
    var profileResult = GetProfile(userResult.Value);
    if (profileResult.IsSuccess)
    {
        // ... more nesting
    }
}

// ✅ Functional chaining
var result = GetUser(id)
    .Bind(user => GetProfile(user))
    .Bind(profile => UpdateProfile(profile))
    .Tap(profile => logger.LogInfo($"Updated profile for {profile.UserId}"));
```

### 4. Handle All Error Cases

```csharp
var (user, error) = GetUser(id);
if (error)
{
    return error switch
    {
        UserNotFound => Results.NotFound(),
        ValidationError => Results.BadRequest(error.Message),
        _ => Results.Problem("Internal server error")
    };
}
```

## API Reference

### Result&lt;T&gt; Methods

| Method | Description |
|--------|-------------|
| `Success(T value)` | Create successful result |
| `Unwrap()` | Get (value, error) tuple |
| `Map<TOut>(Func<T, TOut>)` | Transform success value |
| `Bind<TOut>(Func<T, Result<TOut>>)` | Chain operation |
| `Tap(Action<T>)` | Execute side effect on success |
| `TapError(Action<Error>)` | Execute side effect on error |

### Result&lt;T&gt; Properties

| Property | Description |
|----------|-------------|
| `IsSuccess` | True if result contains a value |
| `IsError` | True if result contains an error |
| `Value` | Get success value (throws if error) |
| `Error` | Get error (throws if success) |

## Building and Testing

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run examples
dotnet run --project src/Examples
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Inspiration

This library is inspired by:
- [F# Result type](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/results)
- [Rust Result enum](https://doc.rust-lang.org/std/result/enum.Result.html)
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)