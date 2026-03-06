using System.Threading.Channels;
using Claims.Application.Common;
using Claims.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Infrastructure.Auditing;

public sealed class AuditService : IAuditService, IHostedService
{
    private const int ChannelCapacity = 4096;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 100;

    private readonly Channel<AuditEntryMessage> _channel = Channel.CreateBounded<AuditEntryMessage>(
        new BoundedChannelOptions(ChannelCapacity) { SingleReader = true });
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditService> _logger;
    private Task? _processTask;
    private readonly CancellationTokenSource _cts = new();

    public AuditService(IServiceProvider serviceProvider, ILogger<AuditService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task AuditClaimAsync(string id, AuditAction auditAction, CancellationToken cancellationToken = default)
    {
        return TryEnqueue(new AuditEntryMessage(true, id, auditAction.ToHttpMethodString()), "Claim", id);
    }

    public Task AuditCoverAsync(string id, AuditAction auditAction, CancellationToken cancellationToken = default)
    {
        return TryEnqueue(new AuditEntryMessage(false, id, auditAction.ToHttpMethodString()), "Cover", id);
    }

    private Task TryEnqueue(AuditEntryMessage message, string kind, string id)
    {
        if (!_channel.Writer.TryWrite(message))
        {
            _logger.LogWarning("Audit channel full, dropping {Kind} audit entry for Id {Id}", kind, id);
        }

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
            var attempt = 0;
            while (true)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                    if (message.IsClaim)
                    {
                        db.ClaimAudits.Add(new ClaimAudit { ClaimId = message.Id, HttpRequestType = message.HttpRequestType, Created = DateTime.UtcNow });
                    }
                    else
                    {
                        db.CoverAudits.Add(new CoverAudit { CoverId = message.Id, HttpRequestType = message.HttpRequestType, Created = DateTime.UtcNow });
                    }

                    await db.SaveChangesAsync(cancellationToken);
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    attempt++;
                    _logger.LogError(ex, "Audit write failed for {Kind} Id {Id}, attempt {Attempt}/{MaxRetries}",
                        message.IsClaim ? "Claim" : "Cover", message.Id, attempt, MaxRetries);

                    if (attempt >= MaxRetries)
                    {
                        _logger.LogError("Audit entry dropped after {MaxRetries} failures: {Kind} Id {Id}, HttpRequestType {HttpRequestType}",
                            MaxRetries, message.IsClaim ? "Claim" : "Cover", message.Id, message.HttpRequestType);
                        break;
                    }

                    await Task.Delay(RetryDelayMs, cancellationToken);
                }
            }
        }
    }
}
