using System;
using System.Diagnostics;

namespace Apparatus.Results;

public readonly struct Result
{
    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The success value</param>
    /// <returns>A successful Result containing the value</returns>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    
    public static Result<T> Error<T>(Error error) => Result<T>.Failure(error);
}


/// <summary>
/// Represents a successful result containing a value of type T.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
[DebuggerDisplay("Success: {Value}")]
public record Success<T>(T Value) : Result<T>
{
    public override string ToString() => $"Success: {Value}";
}

public interface IFailure
{
    public Error Error { get; }
}

/// <summary>
/// Represents a failed result containing an Error.
/// </summary>
/// <typeparam name="T">The type that would have been returned on success</typeparam>
[DebuggerDisplay("Error: {Error}")]
public record Failure<T>(Error Error) : Result<T>, IFailure
{
    public override string ToString() => $"Error: {Error}";
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value of type T or fail with an Error.
/// This type provides a functional approach to error handling without throwing exceptions.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
[DebuggerDisplay("{this switch { Success<T> s => $\"Success: {s.Value}\", Failure<T> f => $\"Error: {f.Error}\", _ => \"Unknown\" }}")]
public abstract record Result<T>
{
    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The success value</param>
    /// <returns>A successful Result containing the value</returns>
    public static Result<T> Success(T value) => new Success<T>(value);

    public static Result<T> Failure(Error error) => new Failure<T>(error);

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    /// <param name="value">The value to wrap in a Result</param>
    public static implicit operator Result<T>(T value) => Success(value);
    
    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to wrap in a Result</param>
    public static implicit operator Result<T>(Error error) => new Failure<T>(error);

    /// <summary>
    /// Extracts the value and error from the Result as a tuple.
    /// In the success case, error will be null. In the failure case, value will be default(T).
    /// </summary>
    /// <returns>A tuple containing the value and error</returns>
    public (T Value, Error Error) Unwrap()
    {
        return this switch
        {
            Success<T>(var value) => (value, null!),
            Failure<T>(var error) => (default!, error),
            _ => throw new InvalidOperationException("Unknown result type")
        };
    }
    
    /// <summary>
    /// Deconstructs the Result into separate value and error variables for pattern matching.
    /// </summary>
    /// <param name="value">The success value (default if failed)</param>
    /// <param name="error">The error (null if successful)</param>
    public void Deconstruct(out T value, out Error? error)
    {
        if (this is Success<T> s)
        {
            value = s.Value;
            error = null;
            return;
        }

        if (this is Failure<T> f)
        {
            
            value = default!;
            error = f.Error;
            return;
        }
        
        throw new InvalidOperationException("Unknown result type");
    }

    /// <summary>
    /// Gets a value indicating whether the result represents a successful operation.
    /// </summary>
    public bool IsSuccess => this is Success<T>;
    
    /// <summary>
    /// Gets a value indicating whether the result represents a failed operation.
    /// </summary>
    public bool IsError => this is Failure<T>;

    /// <summary>
    /// Transforms the success value using the provided function while preserving any error.
    /// </summary>
    /// <typeparam name="TOut">The type of the transformed value</typeparam>
    /// <param name="selector">Function to transform the success value</param>
    /// <returns>A new Result with the transformed value or the original error</returns>
    public Result<TOut> Select<TOut>(Func<T, TOut> selector)
    {
        return this switch
        {
            Success<T>(var value) => selector(value),
            Failure<T>(var error) => error,
            _ => throw new InvalidOperationException("Unknown result type")
        };
    }

    /// <summary>
    /// Chains another Result-returning operation if this Result is successful.
    /// </summary>
    /// <typeparam name="TOut">The type of the chained result</typeparam>
    /// <param name="resultSelector">Function that returns a Result</param>
    /// <returns>The result of the resultSelector function or the original error</returns>
    public Result<TOut> SelectMany<TOut>(Func<T, Result<TOut>> resultSelector)
    {
        return this switch
        {
            Success<T>(var value) => resultSelector(value),
            Failure<T>(var error) => error,
            _ => throw new InvalidOperationException("Unknown result type")
        };
    }

    /// <summary>
    /// Executes a side-effect action if the Result is successful, returning the original Result.
    /// </summary>
    /// <param name="action">Action to execute on success</param>
    /// <returns>The original Result unchanged</returns>
    public Result<T> Do(Action<T> action)
    {
        if (this is Success<T>(var value))
        {
            action(value);
        }
        return this;
    }

    /// <summary>
    /// Executes a side-effect action if the Result is a failure, returning the original Result.
    /// </summary>
    /// <param name="action">Action to execute on error</param>
    /// <returns>The original Result unchanged</returns>
    public Result<T> DoOnError(Action<Error> action)
    {
        if (this is Failure<T>(var error))
        {
            action(error);
        }
        return this;
    }

    /// <summary>
    /// Returns a string representation of the Result.
    /// </summary>
    /// <returns>A string indicating success with value or error with details</returns>
    public override string ToString()
    {
        return this switch
        {
            Success<T>(var value) => $"Success: {value}",
            Failure<T>(var error) => $"Error: {error}",
            _ => "Unknown"
        };
    }
}