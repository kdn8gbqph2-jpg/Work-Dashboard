using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Work_Dashboard.Data;
using Work_Dashboard.Data.Entities;

namespace Work_Dashboard.Services;

/// <summary>
/// Writes business-level audit records to the AuditLogs DB table.
/// Inject via DI and call LogAsync whenever a record is created, updated, or deleted.
/// </summary>
public class AuditService
{
    private readonly IDbContextFactory<BdaDbContext> _dbFactory;
    private readonly ILogger<AuditService>           _logger;

    public AuditService(
        IDbContextFactory<BdaDbContext> dbFactory,
        ILogger<AuditService>           logger)
    {
        _dbFactory = dbFactory;
        _logger    = logger;
    }

    /// <summary>
    /// Persist an audit entry and emit a structured log line.
    /// </summary>
    /// <param name="table">DB table name (e.g. "works", "engineers").</param>
    /// <param name="recordId">Primary key of the affected row.</param>
    /// <param name="action">INSERT | UPDATE | DELETE.</param>
    /// <param name="changedBy">EngineerId of the acting user (null if system).</param>
    /// <param name="details">Optional payload — will be JSON-serialised.</param>
    public async Task LogAsync(
        string      table,
        int         recordId,
        AuditAction action,
        int?        changedBy,
        object?     details = null)
    {
        var detailsJson = details is null
            ? null
            : JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = false });

        // ── Structured application log ─────────────────────────
        _logger.LogInformation(
            "AUDIT {Action} on {Table} #{RecordId} by engineer {ChangedBy} | {Details}",
            action, table, recordId, changedBy ?? 0, detailsJson ?? "—");

        // ── Persist to DB ──────────────────────────────────────
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            db.AuditLogs.Add(new AuditLog
            {
                TableName  = table,
                RecordId   = recordId,
                Action     = action,
                ChangedBy  = changedBy,
                ChangedAt  = DateTime.UtcNow,
                Details    = detailsJson
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Never let audit failure bubble up and break the main operation
            _logger.LogError(ex,
                "Failed to write audit log for {Action} on {Table} #{RecordId}",
                action, table, recordId);
        }
    }
}
