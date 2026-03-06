namespace Claims.Application.Interfaces;

public interface IAuditService
{
    Task AuditClaimAsync(string id, string httpRequestType, CancellationToken cancellationToken = default);
    Task AuditCoverAsync(string id, string httpRequestType, CancellationToken cancellationToken = default);
}
