namespace Apparatus.Results.Tests;

public class PatternMatchingTests
{
    private record ValidationError(string Field, string Reason) : Error("Validation", $"Field '{Field}' is invalid: {Reason}");
    private record NotFoundError(string Resource, string Id) : Error("NotFound", $"{Resource} with ID '{Id}' was not found");
    private record BusinessRuleError(string Rule) : Error("BusinessRule", $"Business rule violated: {Rule}");

    [Fact]
    public Task Result_SwitchExpression_ShouldHandleSuccessAndErrorCases()
    {
        // Arrange
        var results = new Result<string>[]
        {
            "Success value",
            new ValidationError("Email", "Invalid format"),
            new NotFoundError("User", "123")
        };

        // Act
        var messages = results.Select(result => result switch
        {
            { IsSuccess: true } r => $"Success: {r.Value}",
            { Error: ValidationError ve } => $"Validation failed for {ve.Field}: {ve.Reason}",
            { Error: NotFoundError nfe } => $"Could not find {nfe.Resource} with ID {nfe.Id}",
            { Error: var e } => $"Error: {e.Code} - {e.Message}"
        }).ToArray();

        // Assert
        return Verify(messages);
    }

    [Fact]
    public Task Result_Deconstruction_ShouldWorkInIfStatements()
    {
        // Arrange
        var results = new[]
        {
            ("Successful case", (Result<int>)42),
            ("Validation error", (Result<int>)new ValidationError("Age", "Must be positive")),
            ("Not found error", (Result<int>)new NotFoundError("User", "999"))
        };

        // Act
        var outcomes = results.Select(test =>
        {
            var (description, result) = test;
            var (value, error) = result;
            
            if (error != null)
            {
                return $"{description}: Error - {error.Code}";
            }
            else
            {
                return $"{description}: Success - {value}";
            }
        }).ToArray();

        // Assert
        return Verify(outcomes);
    }

    [Fact]
    public Task Result_IsSuccessIsError_ShouldWorkInConditionals()
    {
        // Arrange
        var results = new Result<string>[]
        {
            "Success",
            new ValidationError("Name", "Required"),
            "Another success"
        };

        // Act
        var analysis = results.Select((result, index) => new
        {
            Index = index,
            IsSuccess = result.IsSuccess,
            IsError = result.IsError,
            Type = result.IsSuccess ? "Success" : result.Error.GetType().Name
        }).ToArray();

        // Assert
        return Verify(analysis);
    }

    [Fact]
    public Task Result_ImplicitConversions_ShouldWorkSeamlessly()
    {
        // Arrange & Act
        Result<int> successFromValue = 42;
        Result<int> errorFromError = new ValidationError("Number", "Invalid");
        Result<string> successFromString = "Hello";
        Result<string> errorFromValidation = new NotFoundError("Item", "ABC123");

        var conversions = new[]
        {
            new { Type = "Int Success", IsSuccess = successFromValue.IsSuccess, Value = successFromValue.IsSuccess ? successFromValue.Value.ToString() : null },
            new { Type = "Int Error", IsSuccess = errorFromError.IsSuccess, Value = errorFromError.IsError ? errorFromError.Error.Code : null },
            new { Type = "String Success", IsSuccess = successFromString.IsSuccess, Value = successFromString.IsSuccess ? successFromString.Value : null },
            new { Type = "String Error", IsSuccess = errorFromValidation.IsSuccess, Value = errorFromValidation.IsError ? errorFromValidation.Error.Code : null }
        };

        // Assert
        return Verify(conversions);
    }

    [Fact]
    public Task Result_ComplexPatternMatching_ShouldHandleNestedScenarios()
    {
        // Arrange
        var user = new User(1, "John", "john@example.com");
        var results = new[]
        {
            ProcessUser(user),
            ProcessUser(new User(-1, "", "invalid-email")),
            ProcessUser(new User(999, "NotFound", "notfound@example.com"))
        };

        // Act
        var outcomes = results.Select(result => result switch
        {
            { IsSuccess: true } r => $"Processed user: {r.Value.Name}",
            { Error: ValidationError { Field: "Id" } } => "Invalid user ID provided",
            { Error: ValidationError { Field: "Email" } } => "Invalid email format",
            { Error: ValidationError ve } => $"Validation error in {ve.Field}",
            { Error: NotFoundError } => "User not found in system",
            { Error: BusinessRuleError bre } => $"Business rule violation: {bre.Rule}",
            { Error: var e } => $"Unexpected error: {e.Code}"
        }).ToArray();

        // Assert
        return Verify(outcomes);
    }

    private static Result<ProcessedUser> ProcessUser(User user)
    {
        // Validate user
        if (user.Id <= 0)
            return new ValidationError("Id", "Must be positive");
        
        if (string.IsNullOrEmpty(user.Email) || !user.Email.Contains("@"))
            return new ValidationError("Email", "Invalid format");
        
        if (string.IsNullOrEmpty(user.Name))
            return new ValidationError("Name", "Required");
        
        // Business rules
        if (user.Id == 999)
            return new NotFoundError("User", user.Id.ToString());
        
        if (user.Name.Length < 2)
            return new BusinessRuleError("Name must be at least 2 characters");
        
        // Success
        return new ProcessedUser(user.Id, user.Name.ToUpper(), user.Email.ToLower());
    }

    private record User(int Id, string Name, string Email);
    private record ProcessedUser(int Id, string Name, string Email);
}