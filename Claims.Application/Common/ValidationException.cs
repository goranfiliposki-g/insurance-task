namespace Claims.Application.Common;

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
