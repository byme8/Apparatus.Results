using System;
using System.Threading.Tasks;

namespace Apparatus.Results;

/// <summary>
/// Extension methods for working with Task&lt;Result&lt;T&gt;&gt; to enable async/await patterns with Result types.
/// </summary>
public static class AsyncResult
{
    /// <summary>
    /// Transforms the success value of an async Result using the provided synchronous function.
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="task">The async Result to transform</param>
    /// <param name="selector">Function to transform the success value</param>
    /// <returns>A new async Result with the transformed value or the original error</returns>
    public static async Task<Result<TOut>> Select<TIn, TOut>(this Task<Result<TIn>> task, Func<TIn, TOut> selector)
    {
        var result = await task;
        return result.Select(selector);
    }

    /// <summary>
    /// Transforms the success value of an async Result using the provided asynchronous function.
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="task">The async Result to transform</param>
    /// <param name="selector">Async function to transform the success value</param>
    /// <returns>A new async Result with the transformed value or the original error</returns>
    public static async Task<Result<TOut>> Select<TIn, TOut>(this Task<Result<TIn>> task, Func<TIn, Task<TOut>> selector)
    {
        var result = await task;
        return result.IsSuccess ? await selector(result.Value) : result.Error;
    }

    /// <summary>
    /// Chains another Result-returning operation if the async Result is successful.
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="task">The async Result to chain from</param>
    /// <param name="resultSelector">Function that returns a Result</param>
    /// <returns>The result of the resultSelector function or the original error</returns>
    public static async Task<Result<TOut>> SelectMany<TIn, TOut>(this Task<Result<TIn>> task, Func<TIn, Result<TOut>> resultSelector)
    {
        var result = await task;
        return result.SelectMany(resultSelector);
    }

    /// <summary>
    /// Chains another async Result-returning operation if the async Result is successful.
    /// </summary>
    /// <typeparam name="TIn">The input value type</typeparam>
    /// <typeparam name="TOut">The output value type</typeparam>
    /// <param name="task">The async Result to chain from</param>
    /// <param name="resultSelector">Async function that returns a Result</param>
    /// <returns>The result of the resultSelector function or the original error</returns>
    public static async Task<Result<TOut>> SelectMany<TIn, TOut>(this Task<Result<TIn>> task, Func<TIn, Task<Result<TOut>>> resultSelector)
    {
        var result = await task;
        return result.IsSuccess ? await resultSelector(result.Value) : result.Error;
    }

    /// <summary>
    /// Executes an async side-effect action if the async Result is successful.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="task">The async Result</param>
    /// <param name="action">Async action to execute on success</param>
    /// <returns>The original Result unchanged</returns>
    public static async Task<Result<T>> Do<T>(this Task<Result<T>> task, Func<T, Task> action)
    {
        var result = await task;
        if (result.IsSuccess)
        {
            await action(result.Value);
        }
        return result;
    }

    /// <summary>
    /// Executes an async side-effect action if the async Result is a failure.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="task">The async Result</param>
    /// <param name="action">Async action to execute on error</param>
    /// <returns>The original Result unchanged</returns>
    public static async Task<Result<T>> DoOnError<T>(this Task<Result<T>> task, Func<Error, Task> action)
    {
        var result = await task;
        if (result.IsError)
        {
            await action(result.Error);
        }
        return result;
    }

    /// <summary>
    /// Extracts the value and error from an async Result as a tuple.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="task">The async Result to unwrap</param>
    /// <returns>A tuple containing the value and error</returns>
    public static async Task<(T value, Error error)> Unwrap<T>(this Task<Result<T>> task)
    {
        var result = await task;
        return result.Unwrap();
    }

    /// <summary>
    /// Allows deconstruction of async Results for pattern matching.
    /// Note: This method blocks and should be used carefully.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="task">The async Result to deconstruct</param>
    /// <param name="value">The success value (default if failed)</param>
    /// <param name="error">The error (null if successful)</param>
    public static void Deconstruct<T>(this Task<Result<T>> task, out T value, out Error error)
    {
        var result = task.GetAwaiter().GetResult();
        result.Deconstruct(out value!, out error!);
    }
}