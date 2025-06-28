using System;

namespace Apparatus.Results;

/// <summary>
/// Represents the result of an operation that can either succeed with a value of type T or fail with an Error.
/// This type provides a functional approach to error handling without throwing exceptions.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    private Result(T value)
    {
        _value = value;
        _error = null;
        _isSuccess = true;
    }

    private Result(Error error)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">The success value</param>
    /// <returns>A successful Result containing the value</returns>
    public static Result<T> Success(T value) => new(value);

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    /// <param name="value">The value to wrap in a Result</param>
    public static implicit operator Result<T>(T value) => Success(value);
    
    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to wrap in a Result</param>
    public static implicit operator Result<T>(Error error) => new(error);

    /// <summary>
    /// Extracts the value and error from the Result as a tuple.
    /// In the success case, error will be null. In the failure case, value will be default(T).
    /// </summary>
    /// <returns>A tuple containing the value and error</returns>
    public (T value, Error error) Unwrap()
    {
        return _isSuccess ? (_value!, null!) : (default!, _error!);
    }
    
    /// <summary>
    /// Deconstructs the Result into separate value and error variables for pattern matching.
    /// </summary>
    /// <param name="value">The success value (default if failed)</param>
    /// <param name="error">The error (null if successful)</param>
    public void Deconstruct(out T value, out Error? error)
    {
        if (_isSuccess)
        {
            value = _value!;
            error = null;
        }
        else
        {
            value = default!;
            error = _error;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the result represents a successful operation.
    /// </summary>
    public bool IsSuccess => _isSuccess;
    
    /// <summary>
    /// Gets a value indicating whether the result represents a failed operation.
    /// </summary>
    public bool IsError => !_isSuccess;

    /// <summary>
    /// Gets the success value. Throws an InvalidOperationException if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on a failed result</exception>
    public T Value => _isSuccess ? _value! : throw new InvalidOperationException("Cannot access value of failed result");
    
    /// <summary>
    /// Gets the error. Throws an InvalidOperationException if the result is a success.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a successful result</exception>
    public Error Error => _isSuccess ? throw new InvalidOperationException("Cannot access error of successful result") : _error!;

    /// <summary>
    /// Transforms the success value using the provided function while preserving any error.
    /// </summary>
    /// <typeparam name="TOut">The type of the transformed value</typeparam>
    /// <param name="selector">Function to transform the success value</param>
    /// <returns>A new Result with the transformed value or the original error</returns>
    public Result<TOut> Select<TOut>(Func<T, TOut> selector)
    {
        return _isSuccess ? selector(_value!) : _error!;
    }

    /// <summary>
    /// Chains another Result-returning operation if this Result is successful.
    /// </summary>
    /// <typeparam name="TOut">The type of the chained result</typeparam>
    /// <param name="resultSelector">Function that returns a Result</param>
    /// <returns>The result of the resultSelector function or the original error</returns>
    public Result<TOut> SelectMany<TOut>(Func<T, Result<TOut>> resultSelector)
    {
        return _isSuccess ? resultSelector(_value!) : _error!;
    }

    /// <summary>
    /// Executes a side-effect action if the Result is successful, returning the original Result.
    /// </summary>
    /// <param name="action">Action to execute on success</param>
    /// <returns>The original Result unchanged</returns>
    public Result<T> Do(Action<T> action)
    {
        if (_isSuccess)
        {
            action(_value!);
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
        if (_isSuccess == false)
        {
            action(_error!);
        }
        return this;
    }
}