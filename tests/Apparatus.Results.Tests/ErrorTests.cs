namespace Apparatus.Results.Tests;

public class ErrorTests
{
    private record TestError(string Code, string Message) : Error(Code, Message);
    private record ValidationError(string Field, string Reason) : Error("Validation", $"Field '{Field}' is invalid: {Reason}");
    private record NotFoundError(string Resource, string Id) : Error("NotFound", $"{Resource} with ID '{Id}' was not found");

    [Fact]
    public Task Error_Properties_ShouldBeSetCorrectly()
    {
        // Arrange
        var error = new TestError("TEST_CODE", "Test message");

        // Act & Assert
        return Verify(new { error.Code, error.Message });
    }

    [Fact]
    public async Task Error_ImplicitBoolConversion_NullError_ShouldReturnFalse()
    {
        // Arrange
        Error? error = null;

        // Act
        bool result = error;

        // Assert
        await Verify(result);
    }

    [Fact]
    public Task Error_ImplicitBoolConversion_NonNullError_ShouldReturnTrue()
    {
        // Arrange
        Error error = new TestError("TEST", "Test error");

        // Act
        bool result = error;

        // Assert
        return Verify(result);
    }

    [Fact]
    public async Task Error_RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var error1 = new TestError("TEST", "Message");
        var error2 = new TestError("TEST", "Message");

        // Act & Assert
        await Verify(new { AreEqual = error1 == error2, HashCodesEqual = error1.GetHashCode() == error2.GetHashCode() });
    }

    [Fact]
    public Task Error_RecordEquality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new TestError("TEST1", "Message");
        var error2 = new TestError("TEST2", "Message");

        // Act & Assert
        return Verify(new { AreEqual = error1 == error2 });
    }

    [Fact]
    public Task Error_Inheritance_ShouldWorkCorrectly()
    {
        // Arrange
        var validationError = new ValidationError("Email", "Invalid format");
        var notFoundError = new NotFoundError("User", "123");

        // Act & Assert
        return Verify(new 
        { 
            ValidationError = new { validationError.Code, validationError.Message },
            NotFoundError = new { notFoundError.Code, notFoundError.Message }
        });
    }

    [Fact]
    public Task Error_PatternMatching_ShouldWorkWithDifferentErrorTypes()
    {
        // Arrange
        Error[] errors = 
        {
            new ValidationError("Name", "Required"),
            new NotFoundError("User", "456"),
            new TestError("CUSTOM", "Custom error")
        };

        // Act
        var results = errors.Select(error => error switch
        {
            ValidationError ve => $"Validation failed for {ve.Field}: {ve.Reason}",
            NotFoundError nfe => $"Could not find {nfe.Resource} with ID {nfe.Id}",
            TestError te => $"Test error: {te.Message}",
            _ => "Unknown error"
        }).ToArray();

        // Assert
        return Verify(results);
    }

    [Fact]
    public Task Error_ToString_ShouldProvideReadableFormat()
    {
        // Arrange
        var errors = new Error[]
        {
            new TestError("TEST", "Test message"),
            new ValidationError("Email", "Invalid format"),
            new NotFoundError("User", "123")
        };

        // Act
        var stringRepresentations = errors.Select(e => e.ToString()).ToArray();

        // Assert
        return Verify(stringRepresentations);
    }
}