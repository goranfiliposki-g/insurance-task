namespace Claims.Infrastructure.Auditing;

internal record AuditEntryMessage(bool IsClaim, string Id, string HttpRequestType);
