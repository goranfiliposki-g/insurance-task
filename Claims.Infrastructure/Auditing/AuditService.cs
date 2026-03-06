using System.Threading.Channels;
using Claims.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Claims.Infrastructure.Auditing;

public sealed class AuditService : IAuditService, IHostedService
{
    private readonly Channel<AuditEntryMessage> _channel = Channel.CreateUnbounded<AuditEntryMessage>(new UnboundedChannelOptions { SingleReader = true });
    private readonly IServiceProvider _serviceProvider;
    private Task? _processTask;
    private readonly CancellationTokenSource _cts = new();

    public AuditService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task AuditClaimAsync(string id, string httpRequestType, CancellationToken cancellationToken = default)
    {
        _channel.Writer.TryWrite(new AuditEntryMessage(true, id, httpRequestType));
        return Task.CompletedTask;
    }

    public Task AuditCoverAsync(string id, string httpRequestType, CancellationToken cancellationToken = default)
    {
        _channel.Writer.TryWrite(new AuditEntryMessage(false, id, httpRequestType));
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _processTask = ProcessAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        if (_processTask != null)
            await _processTask;
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                if (message.IsClaim)
                    db.ClaimAudits.Add(new ClaimAudit { ClaimId = message.Id, HttpRequestType = message.HttpRequestType, Created = DateTime.UtcNow });
                else
                    db.CoverAudits.Add(new CoverAudit { CoverId = message.Id, HttpRequestType = message.HttpRequestType, Created = DateTime.UtcNow });
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
               // log for prod.
            }
        }
    }
}
