namespace Apparatus.Results.Tests;

public class DebugDisplayTests
{
    private record TestError(string Code, string Message) : Error(Code, Message);

    [Fact]
    public async Task Result_ToString_SuccessfulResult_ShouldDisplayValue()
    {
        // Arrange
        Result<string> result = "test value";

        // Act
        var display = result.ToString();

        // Assert
        await Verify(display);
    }

    [Fact]
    public async Task Result_ToString_FailedResult_ShouldDisplayError()
    {
        // Arrange
        var error = new TestError("TEST_CODE", "Test error message");
        Result<string> result = error;

        // Act
        var display = result.ToString();

        // Assert
        await Verify(display);
    }

    [Fact]
    public async Task Error_ToString_ShouldDisplayCodeAndMessage()
    {
        // Arrange
        var error = new TestError("VALIDATION", "Username is required");

        // Act
        var display = error.ToString();

        // Assert
        await Verify(display);
    }

    [Fact]
    public async Task Result_ToString_WithComplexObject_ShouldDisplayCorrectly()
    {
        // Arrange
        var user = new { Id = 123, Name = "John Doe" };
        Result<object> result = user;

        // Act
        var display = result.ToString();

        // Assert
        await Verify(display);
    }

    [Fact]
    public async Task Result_ToString_WithNullValue_ShouldHandleGracefully()
    {
        // Arrange
        Result<string?> result = (string?)null;

        // Act
        var display = result.ToString();

        // Assert
        await Verify(display);
    }
}