# Apparatus.Results

A modern Result<T> pattern for functional error handling in C#.

**Apparatus.Results** provides type-safe error handling without exceptions. Designed for C# developers who want clean, composable error handling with full async support.

## Why Apparatus.Results?

- ✅ **Type-safe** - Compiler-enforced error handling
- ✅ **No exceptions** - Predictable error flows  
- ✅ **Zero dependencies** - Lightweight and fast

## Installation

```bash
dotnet add package Apparatus.Results
```

## Quick Start

### 1. Define Your Errors

```csharp
using Apparatus.Results;

public record UserNotFound(int Id) 
    : Error("UserNotFound", $"User {Id} not found");

public record InvalidEmail(string Email) 
    : Error("InvalidEmail", $"Invalid email: {Email}");
```

### 2. Return Results Instead of Throwing

```csharp
public Result<User> GetUser(int id)
{
    if (id <= 0) 
        return new UserNotFound(id);
    
    return new User(id, "John Doe"); // Implicit conversion
}
```

### 3. Handle Results Safely

```csharp
var (user, error) = GetUser(123);
if (error)
{
    Console.WriteLine($"Error: {error.Message}");
    return;
}

Console.WriteLine($"Hello, {user.Name}!");
```

## Key Features

### Pattern Matching

```csharp
var message = GetUser(id) switch
{
    { IsSuccess: true } r => $"Hello, {r.Value.Name}",
    { Error: UserNotFound e } => $"User {e.Id} not found",
    { Error: var e } => $"Error: {e.Message}"
};
```

### LINQ Support

```csharp
var result = GetUser(id)
    .Select(user => user.Name.ToUpper())
    .Do(name => Console.WriteLine($"Processing: {name}"));
```

### Async Operations

```csharp
var result = await GetUserAsync(id)
    .Select(user => user.Email)
    .SelectMany(email => SendEmailAsync(email));
```

## Advanced Usage

### Error Chaining

```csharp
var result = ValidateUser(userData)
    .SelectMany(user => SaveToDatabase(user))
    .SelectMany(user => SendWelcomeEmail(user))
    .Do(user => logger.LogInfo($"User {user.Id} created"));
```

### Error Aggregation

```csharp
public record ValidationErrors(IReadOnlyList<Error> Errors) 
    : Error("ValidationErrors", $"{Errors.Count} validation errors");

var errors = new List<Error>();
if (string.IsNullOrEmpty(name)) 
{
    errors.Add(new InvalidName());
}

if (age < 0) 
{
    errors.Add(new InvalidAge());
}

return errors.Any() ? new ValidationErrors(errors) : new User(name, age);
```

## API Reference

### Creating Results

```csharp
// Success
Result<string> success = "Hello World";
Result<string> success = Result<string>.Success("Hello World");

// Error  
Result<string> error = new ValidationError("Invalid input");
```

### Core Methods

| Method | Description |
|--------|-------------|
| `Select<TOut>(Func<T, TOut>)` | Transform success value |
| `SelectMany<TOut>(Func<T, Result<TOut>>)` | Chain operations |
| `Do(Action<T>)` | Side effect on success |
| `DoOnError(Action<Error>)` | Side effect on error |
| `Unwrap()` | Get (value, error) tuple |

### Properties

| Property | Description |
|----------|-------------|
| `IsSuccess` | True if contains value |
| `IsError` | True if contains error |
| `Value` | Get value (throws if error) |
| `Error` | Get error (throws if success) |

## License

MIT License - see [LICENSE](LICENSE) file for details.