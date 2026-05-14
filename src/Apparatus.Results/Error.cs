using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Apparatus.Results;

/// <summary>
/// Abstract base class for all error types in the Result pattern.
/// Provides a consistent structure for error information with code, message, and optional extensions.
/// </summary>
/// <param name="Code">A unique identifier for the error type</param>
/// <param name="Message">A human-readable description of the error</param>
[DebuggerDisplay("{Code}: {Message}")]
public record Error(string Code, string Message)
{
    /// <summary>
    /// Optional structured extension data (e.g. for transport over GraphQL/HTTP).
    /// Populated only via <see cref="ErrorExtensions.With{TError}"/>; not assignable from the outside.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Extensions { get; private set; }

    /// <summary>
    /// Internal entry point used by <see cref="ErrorExtensions.With{TError}"/> to set the backing dictionary.
    /// </summary>
    internal void SetExtensions(Dictionary<string, object?> extensions) => Extensions = extensions;

    /// <summary>
    /// Returns the extensions dictionary or an empty enumerable when none were provided.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object?>> EnumerateExtensions() =>
        Extensions ?? Enumerable.Empty<KeyValuePair<string, object?>>();

    /// <summary>
    /// Omits the <see cref="Extensions"/> property from the record-generated ToString output to keep it stable.
    /// </summary>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Code = ").Append(Code)
               .Append(", Message = ").Append(Message);
        return true;
    }

    /// <summary>
    /// Implicitly converts an Error to a boolean value.
    /// Returns true if the error is not null, false otherwise.
    /// </summary>
    /// <param name="error">The error to check for null</param>
    /// <returns>True if error is not null, false if null</returns>
    public static implicit operator bool([NotNullWhen(true)]Error? error) => error is not null;
}

/// <summary>
/// Fluent helpers for attaching extension data to <see cref="Error"/> instances without
/// allocating dictionaries by hand.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Adds the given key/value to the error's <see cref="Error.Extensions"/> and returns the same error.
    /// Mutates the error in place; the backing dictionary is allocated on the first call and reused on chains.
    /// </summary>
    public static TError With<TError>(this TError error, string key, object? value)
        where TError : Error
    {
        if (error.Extensions is Dictionary<string, object?> dict)
        {
            dict[key] = value;
            return error;
        }

        dict = new Dictionary<string, object?> { [key] = value };
        error.SetExtensions(dict);
        return error;
    }
}