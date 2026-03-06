namespace Claims.Application.Common
{
    public static class AuditActionExtension
    {
        public static string ToHttpMethodString(this AuditAction action) => action switch
        {
            AuditAction.Create => "POST",
            AuditAction.Delete => "DELETE",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
}
