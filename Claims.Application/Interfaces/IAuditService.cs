using Claims.Application.Common;

namespace Claims.Application.Interfaces;

public interface IAuditService
{
    Task AuditClaimAsync(string id, AuditAction auditAction, CancellationToken cancellationToken = default);
    Task AuditCoverAsync(string id, AuditAction auditAction, CancellationToken cancellationToken = default);
}
