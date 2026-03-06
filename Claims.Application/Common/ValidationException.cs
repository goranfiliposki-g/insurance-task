namespace Claims.Application.Common;

/// <summary>Thrown when request or domain validation fails.</summary>
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors) : base(string.Join("; ", errors))
    {
        Errors = errors;
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new[] { message };
    }
}
