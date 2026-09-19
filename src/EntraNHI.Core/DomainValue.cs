namespace EntraNHI.Core;

/// <summary>
/// Represents whether a normalized domain value is present or is not presently known.
/// </summary>
/// <typeparam name="T">The normalized value type.</typeparam>
/// <remarks>
/// This type represents value presence only. It does not encode capability,
/// completeness, operational-failure, provider, or assessment-state semantics.
/// </remarks>
public sealed class DomainValue<T>
{
    private readonly T? _value;

    private DomainValue(bool isPresent, T? value)
    {
        IsPresent = isPresent;
        _value = value;
    }

    /// <summary>
    /// Gets whether this instance contains a present value.
    /// </summary>
    public bool IsPresent { get; }

    /// <summary>
    /// Gets the present value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value is not presently known.
    /// </exception>
    public T Value =>
        IsPresent
            ? _value!
            : throw new InvalidOperationException(
                "A value that is not presently known cannot be accessed.");

    /// <summary>
    /// Creates a domain value containing a present, non-null value.
    /// </summary>
    public static DomainValue<T> Present(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new DomainValue<T>(true, value);
    }

    /// <summary>
    /// Creates a domain value whose value is not presently known.
    /// </summary>
    public static DomainValue<T> NotPresentlyKnown()
    {
        return new DomainValue<T>(false, default);
    }
}
