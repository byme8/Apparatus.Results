using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Apparatus.Results;

/// <summary>
/// Abstract base class for all error types in the Result pattern.
/// Provides a consistent structure for error information with code and message.
/// </summary>
/// <param name="Code">A unique identifier for the error type</param>
/// <param name="Message">A human-readable description of the error</param>
[DebuggerDisplay("{Code}: {Message}")]
public record Error(string Code, string Message)
{
    /// <summary>
    /// Implicitly converts an Error to a boolean value.
    /// Returns true if the error is not null, false otherwise.
    /// </summary>
    /// <param name="error">The error to check for null</param>
    /// <returns>True if error is not null, false if null</returns>
    public static implicit operator bool([NotNullWhen(true)]Error? error) => error is not null;
}