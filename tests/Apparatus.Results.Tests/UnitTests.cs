using System;
using Apparatus.Results;
using Xunit;

namespace Apparatus.Results.Tests;

public class UnitTests
{
    [Fact]
    public Task Unit_Value_ShouldReturnUnit()
    {
        var unit = Unit.Value;
        return Verify(unit);
    }

    [Fact]
    public Task Unit_Equals_SameInstance_ShouldReturnTrue()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;
        var areEqual = unit1.Equals(unit2);
        return Verify(areEqual);
    }

    [Fact]
    public Task Unit_Equals_Object_WithUnit_ShouldReturnTrue()
    {
        var unit = Unit.Value;
        object obj = Unit.Value;
        var areEqual = unit.Equals(obj);
        return Verify(areEqual);
    }

    [Fact]
    public Task Unit_Equals_Object_WithNull_ShouldReturnFalse()
    {
        var unit = Unit.Value;
        object? obj = null;
        var areEqual = unit.Equals(obj);
        return Verify(areEqual);
    }

    [Fact]
    public Task Unit_Equals_Object_WithString_ShouldReturnFalse()
    {
        var unit = Unit.Value;
        object obj = "test";
        var areEqual = unit.Equals(obj);
        return Verify(areEqual);
    }

    [Fact]
    public Task Unit_GetHashCode_ShouldReturnZero()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;
        return Verify(new { hash1 = unit1.GetHashCode(), hash2 = unit2.GetHashCode() });
    }

    [Fact]
    public Task Unit_ToString_ShouldReturnUnit()
    {
        var unit = Unit.Value;
        var str = unit.ToString();
        return Verify(str);
    }

    [Fact]
    public Task Unit_EqualityOperator_ShouldReturnTrue()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;
        var areEqual = unit1 == unit2;
        return Verify(areEqual);
    }

    [Fact]
    public Task Unit_InequalityOperator_ShouldReturnFalse()
    {
        var unit1 = Unit.Value;
        var unit2 = Unit.Value;
        var areNotEqual = unit1 != unit2;
        return Verify(areNotEqual);
    }

    [Fact]
    public Task Unit_WithResult_SuccessCase_ShouldWork()
    {
        Result<Unit> result = Unit.Value;
        return Verify(result);
    }

    [Fact]
    public Task Unit_WithResult_ErrorCase_ShouldWork()
    {
        Result<Unit> result = new TestError("UNIT_ERROR", "Unit test error");
        return Verify(result);
    }

    [Fact]
    public Task Unit_WithResult_ChainedOperations_ShouldWork()
    {
        Result<Unit> result = Unit.Value;
        var transformed = result
            .Select(_ => "completed")
            .SelectMany(s => Result<string>.Success($"Operation {s}"));
        
        return Verify(transformed);
    }

    private record TestError(string Code, string Message) : Error(Code, Message);
}