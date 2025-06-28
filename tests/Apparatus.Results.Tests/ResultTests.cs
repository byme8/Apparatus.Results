namespace Apparatus.Results.Tests;

public class ResultTests
{
    private record TestError(string Code, string Message) : Error(Code, Message);
    private record ValidationError(string Field, string Reason) : Error("Validation", $"Field '{Field}' is invalid: {Reason}");

    [Fact]
    public async Task Success_CreateSuccessfulResult_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var result = Result<string>.Success("test value");

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task ImplicitConversion_FromValue_ShouldCreateSuccessfulResult()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");

        // Act
        Result<string> result = error;

        // Assert
        await Verify(result);
    }

    [Fact]
    public async Task Unwrap_SuccessfulResult_ShouldReturnValueAndNullError()
    {
        // Arrange
        Result<string> result = "test";

        // Act
        var unwrapped = result.Unwrap();

        // Assert
        await Verify(unwrapped);
    }

    [Fact]
    public async Task Unwrap_FailedResult_ShouldReturnDefaultValueAndError()
    {
        // Arrange
        var testError = new TestError("TEST", "Test error");
        Result<string> result = testError;

        // Act
        var unwrapped = result.Unwrap();

        // Assert
        await Verify(unwrapped);
    }

    [Fact]
    public async Task Deconstruct_SuccessfulResult_ShouldReturnValueAndNullError()
    {
        // Arrange
        Result<int> result = 42;

        // Act
        var (value, error) = result;

        // Assert
        await Verify(new { value, error });
    }

    [Fact]
    public async Task Deconstruct_FailedResult_ShouldReturnDefaultValueAndError()
    {
        // Arrange
        var testError = new TestError("TEST", "Test error");
        Result<int> result = testError;

        // Act
        var (value, error) = result;

        // Assert
        await Verify(new { value, error });
    }

    [Fact]
    public async Task Select_SuccessfulResult_ShouldTransformValue()
    {
        // Arrange
        Result<int> result = 5;

        // Act
        var transformed = result.Select(x => x * 2);

        // Assert
        await Verify(transformed);
    }

    [Fact]
    public async Task Select_FailedResult_ShouldPreserveError()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        Result<int> result = error;

        // Act
        var transformed = result.Select(x => x * 2);

        // Assert
        await Verify(transformed);
    }

    [Fact]
    public async Task SelectMany_SuccessfulResult_ShouldChainOperation()
    {
        // Arrange
        Result<int> result = 5;

        // Act
        var chained = result.SelectMany(x => Result<string>.Success($"Value: {x}"));

        // Assert
        await Verify(chained);
    }

    [Fact]
    public async Task SelectMany_FailedResult_ShouldPreserveError()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        Result<int> result = error;

        // Act
        var chained = result.SelectMany(x => Result<string>.Success($"Value: {x}"));

        // Assert
        await Verify(chained);
    }

    [Fact]
    public async Task SelectMany_SuccessfulResultButChainedFails_ShouldReturnChainedError()
    {
        // Arrange
        Result<int> result = 5;
        var chainedError = new ValidationError("Name", "Required");

        // Act
        var chained = result.SelectMany(x => (Result<string>)chainedError);

        // Assert
        await Verify(chained);
    }

    [Fact]
    public async Task Do_SuccessfulResult_ShouldExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        Result<int> result = 42;
        var executed = false;
        var capturedValue = 0;

        // Act
        var returned = result.Do(value =>
        {
            executed = true;
            capturedValue = value;
        });

        // Assert
        await Verify(new { executed, capturedValue, result = returned });
    }

    [Fact]
    public async Task Do_FailedResult_ShouldNotExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        Result<int> result = error;
        var executed = false;

        // Act
        var returned = result.Do(value => executed = true);

        // Assert
        await Verify(new { executed, result = returned });
    }

    [Fact]
    public async Task DoOnError_SuccessfulResult_ShouldNotExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        Result<int> result = 42;
        var executed = false;

        // Act
        var returned = result.DoOnError(error => executed = true);

        // Assert
        await Verify(new { executed, result = returned });
    }

    [Fact]
    public async Task DoOnError_FailedResult_ShouldExecuteActionAndReturnOriginalResult()
    {
        // Arrange
        var testError = new TestError("TEST", "Test error");
        Result<int> result = testError;
        var executed = false;
        Error? capturedError = null;

        // Act
        var returned = result.DoOnError(error =>
        {
            executed = true;
            capturedError = error;
        });

        // Assert
        await Verify(new { executed, capturedError, result = returned });
    }

    [Fact]
    public async Task ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        Result<int> result = 5;
        var sideEffectExecuted = false;

        // Act
        var final = result
            .Select(x => x * 2)
            .SelectMany(x => x > 5 ? Result<string>.Success($"Large: {x}") : new ValidationError("Value", "Too small"))
            .Do(value => sideEffectExecuted = true);

        // Assert
        await Verify(new { result = final, sideEffectExecuted });
    }

    [Fact]
    public async Task ChainedOperations_WithFailure_ShouldStopAtFirstError()
    {
        // Arrange
        var initialError = new TestError("INITIAL", "Initial error");
        Result<int> result = initialError;
        var sideEffectExecuted = false;

        // Act
        var final = result
            .Select(x => x * 2)
            .SelectMany(x => Result<string>.Success($"Value: {x}"))
            .Do(value => sideEffectExecuted = true);

        // Assert
        await Verify(new { result = final, sideEffectExecuted });
    }

    [Fact]
    public async Task Value_AccessOnFailedResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = new TestError("TEST", "Test error");
        Result<string> result = error;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        await Verify(new { ExceptionMessage = exception.Message });
    }

    [Fact]
    public async Task Error_AccessOnSuccessfulResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Result<string> result = "test value";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Error);
        await Verify(new { ExceptionMessage = exception.Message });
    }
}