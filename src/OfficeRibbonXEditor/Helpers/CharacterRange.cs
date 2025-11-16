using System.Runtime.InteropServices;

namespace OfficeRibbonXEditor.Helpers;

/// <summary>
/// Specifies a range of characters. If the min and max positions are equal, the range is empty.
/// The range includes everything if min position is 0 and max position is –1.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct CharacterRange : IEquatable<CharacterRange>
{
    /// <summary>
    /// Character position index immediately preceding the first character in the range.
    /// </summary>
    public int MinPosition { get; }

    /// <summary>
    /// Character position immediately following the last character in the range.
    /// </summary>
    public int MaxPosition { get; }

    /// <summary>
    /// Specifies a range of characters. If the min and max positions are equal, the range is empty.
    /// The range includes everything if min position is 0 and max position is –1.
    /// </summary>
    /// <param name="minPosition">The minimum, or start position.</param>
    /// <param name="maxPosition">The maximum, or end position.</param>
    public CharacterRange(int minPosition, int maxPosition)
    {
        MinPosition = minPosition;
        MaxPosition = maxPosition;
    }

    public override int GetHashCode()
    {
        return 31 * MinPosition + 17 ^ MaxPosition;
    }

    public static bool operator ==(CharacterRange left, CharacterRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CharacterRange left, CharacterRange right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CharacterRange other)
        {
            return false;
        }

        return Equals(other);
    }

    public bool Equals(CharacterRange other)
    {
        return MinPosition == other.MinPosition && MaxPosition == other.MaxPosition;
    }
}