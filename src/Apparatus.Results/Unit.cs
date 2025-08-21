using System;
using System.Diagnostics;

namespace Apparatus.Results;

/// <summary>
/// Represents a unit type - a type with only one value. Used to indicate the presence of a value
/// when the specific value is not important, commonly for operations that succeed but don't return meaningful data.
/// </summary>
[DebuggerDisplay("Unit")]
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// The single instance of Unit. All Unit values are equivalent.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Determines whether two Unit instances are equal. Always returns true since all Unit values are equivalent.
    /// </summary>
    /// <param name="other">The other Unit instance to compare</param>
    /// <returns>Always true</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether the specified object is equal to this Unit instance.
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if obj is a Unit, false otherwise</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns the hash code for this Unit instance. Always returns the same value since all Unit instances are equal.
    /// </summary>
    /// <returns>A constant hash code</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a string representation of the Unit.
    /// </summary>
    /// <returns>The string "Unit"</returns>
    public override string ToString() => "Unit";

    /// <summary>
    /// Determines whether two Unit instances are equal. Always returns true.
    /// </summary>
    /// <param name="left">The first Unit instance</param>
    /// <param name="right">The second Unit instance</param>
    /// <returns>Always true</returns>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Determines whether two Unit instances are not equal. Always returns false.
    /// </summary>
    /// <param name="left">The first Unit instance</param>
    /// <param name="right">The second Unit instance</param>
    /// <returns>Always false</returns>
    public static bool operator !=(Unit left, Unit right) => false;
}