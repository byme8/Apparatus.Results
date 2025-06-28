namespace Apparatus.Results.Tests;

public class AsyncResultTests
{
    private record TestError(string Code, string Message) : Error(Code, Message);
    private record ValidationError(string Field, string Reason) : Error("Validation", $"Field '{Field}' is invalid: {Reason}");

    [Fact]
    public async Task Select_SuccessfulAsyncResult_ShouldTransformValue()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)5);

        // Act
        var result = await task.Select(x => x * 2);

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task Select_FailedAsyncResult_ShouldPreserveError()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<int>)error);

        // Act
        var result = await task.Select(x => x * 2);

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task Select_WithAsyncSelector_ShouldTransformValue()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)5);

        // Act
        var result = await task.Select(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task Select_WithAsyncSelectorOnFailedResult_ShouldPreserveError()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<int>)error);

        // Act
        var result = await task.Select(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task SelectMany_SuccessfulAsyncResult_ShouldChainOperation()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)5);

        // Act
        var result = await task.SelectMany(x => (Result<string>)$"Value: {x}");

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task SelectMany_FailedAsyncResult_ShouldPreserveError()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<int>)error);

        // Act
        var result = await task.SelectMany(x => (Result<string>)$"Value: {x}");

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task SelectMany_WithAsyncResultSelector_ShouldChainOperation()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)5);

        // Act
        var result = await task.SelectMany(async x =>
        {
            await Task.Delay(1);
            return (Result<string>)$"Value: {x}";
        });

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task SelectMany_ChainedFailure_ShouldReturnChainedError()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)5);
        var chainedError = new ValidationError("Name", "Required");

        // Act
        var result = await task.SelectMany(x => (Result<string>)chainedError);

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task Do_SuccessfulAsyncResult_ShouldExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)42);
        var executed = false;
        var capturedValue = 0;

        // Act
        var result = await task.Do(async value =>
        {
            await Task.Delay(1);
            executed = true;
            capturedValue = value;
        });

        // Assert
        await Verify(new { executed, capturedValue, result });
    }

    [Fact]
    public async Task Do_FailedAsyncResult_ShouldNotExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<int>)error);
        var executed = false;

        // Act
        var result = await task.Do(async value =>
        {
            await Task.Delay(1);
            executed = true;
        });

        // Assert
        await Verify(new { executed, result });
    }

    [Fact]
    public async Task DoOnError_SuccessfulAsyncResult_ShouldNotExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)42);
        var executed = false;

        // Act
        var result = await task.DoOnError(async error =>
        {
            await Task.Delay(1);
            executed = true;
        });

        // Assert
        await Verify(new { executed, result });
    }

    [Fact]
    public async Task DoOnError_FailedAsyncResult_ShouldExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var testError = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<int>)testError);
        var executed = false;
        Error? capturedError = null;

        // Act
        var result = await task.DoOnError(async error =>
        {
            await Task.Delay(1);
            executed = true;
            capturedError = error;
        });

        // Assert
        await Verify(new { executed, capturedError, result });
    }

    [Fact]
    public async Task Unwrap_SuccessfulAsyncResult_ShouldReturnValueAndNullError()
    {
        // Arrange
        var task = Task.FromResult((Result<string>)"test");

        // Act
        var unwrapped = await task.Unwrap();

        // Assert
        await Verify(unwrapped);
    }

    [Fact]
    public async Task Unwrap_FailedAsyncResult_ShouldReturnDefaultValueAndError()
    {
        // Arrange
        var testError = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<string>)testError);

        // Act
        var unwrapped = await task.Unwrap();

        // Assert
        await Verify(unwrapped);
    }

    [Fact]
    public async Task Deconstruct_SuccessfulAsyncResult_ShouldReturnValueAndNullError()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)42);

        // Act
        var (value, error) = task;

        // Assert
        await Verify(new { value, error });
    }

    [Fact]
    public async Task Deconstruct_FailedAsyncResult_ShouldReturnDefaultValueAndError()
    {
        // Arrange
        var testError = new TestError("TEST", "Test error");
        var task = Task.FromResult((Result<int>)testError);

        // Act
        var (value, error) = task;

        // Assert
        await Verify(new { value, error });
    }

    [Fact]
    public async Task ChainedAsyncOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var task = Task.FromResult((Result<int>)5);
        var sideEffectExecuted = false;

        // Act
        var result = await task
            .Select(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            })
            .SelectMany(async x =>
            {
                await Task.Delay(1);
                return x > 5 ? (Result<string>)$"Large: {x}" : new ValidationError("Value", "Too small");
            })
            .Do(async value =>
            {
                await Task.Delay(1);
                sideEffectExecuted = true;
            });

        // Assert
        await Verify(new { result, sideEffectExecuted });
    }

    [Fact]
    public async Task ChainedAsyncOperations_WithFailure_ShouldStopAtFirstError()
    {
        // Arrange
        var initialError = new TestError("INITIAL", "Initial error");
        var task = Task.FromResult((Result<int>)initialError);
        var sideEffectExecuted = false;

        // Act
        var result = await task
            .Select(async x =>
            {
                await Task.Delay(1);
                return x * 2;
            })
            .SelectMany(async x =>
            {
                await Task.Delay(1);
                return (Result<string>)$"Value: {x}";
            })
            .Do(async value =>
            {
                await Task.Delay(1);
                sideEffectExecuted = true;
            });

        // Assert
        await Verify(new { result, sideEffectExecuted });
    }

    [Fact]
    public async Task RealWorldAsyncScenario_DatabaseAndApiCalls_ShouldWorkCorrectly()
    {
        // Arrange
        async Task<Result<User>> GetUserFromDatabaseAsync(int id)
        {
            await Task.Delay(1); // Simulate DB call
            return id > 0 ? new User(id, $"User{id}") : new ValidationError("UserId", "Must be positive");
        }

        async Task<Result<UserProfile>> GetUserProfileAsync(User user)
        {
            await Task.Delay(1); // Simulate API call
            return user.Id != 999 ? new UserProfile(user.Id, $"{user.Name}@example.com") : new TestError("API", "Profile not found");
        }

        // Act
        var result = await GetUserFromDatabaseAsync(123)
            .SelectMany(user => GetUserProfileAsync(user))
            .Do(async profile =>
            {
                await Task.Delay(1); // Simulate logging
            });

        // Assert
        await Verify(result);
    }

    private record User(int Id, string Name);
    private record UserProfile(int UserId, string Email);
}