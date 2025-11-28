using System.Collections.Concurrent;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services.Printing;

/// <summary>
/// Print queue untuk menyimpan dan retry print jobs yang gagal
/// Requirements: 5.3 - Menyimpan struk untuk dicetak ulang
/// </summary>
public class PrintQueue
{
    private readonly ConcurrentQueue<PrintJob> _pendingJobs = new();
    private readonly ConcurrentDictionary<Guid, PrintJob> _failedJobs = new();
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;
    private bool _isProcessing;
    private CancellationTokenSource? _cts;

    public event EventHandler<PrintJobEventArgs>? JobCompleted;
    public event EventHandler<PrintJobEventArgs>? JobFailed;
    public event EventHandler<PrintJobEventArgs>? JobRetrying;

    public int PendingCount => _pendingJobs.Count;
    public int FailedCount => _failedJobs.Count;
    public IEnumerable<PrintJob> FailedJobs => _failedJobs.Values;

    public PrintQueue(int maxRetries = 3, int retryDelaySeconds = 5)
    {
        _maxRetries = maxRetries;
        _retryDelay = TimeSpan.FromSeconds(retryDelaySeconds);
    }

    /// <summary>
    /// Enqueue print job
    /// </summary>
    public Guid Enqueue(TransactionDto transaction)
    {
        var job = new PrintJob
        {
            Id = Guid.NewGuid(),
            Transaction = transaction,
            CreatedAt = DateTime.Now,
            RetryCount = 0,
            Status = PrintJobStatus.Pending
        };

        _pendingJobs.Enqueue(job);
        return job.Id;
    }

    /// <summary>
    /// Start processing queue
    /// </summary>
    public void StartProcessing(Func<TransactionDto, Task<bool>> printAction)
    {
        if (_isProcessing) return;

        _isProcessing = true;
        _cts = new CancellationTokenSource();

        Task.Run(async () => await ProcessQueueAsync(printAction, _cts.Token));
    }

    /// <summary>
    /// Stop processing queue
    /// </summary>
    public void StopProcessing()
    {
        _isProcessing = false;
        _cts?.Cancel();
    }

    /// <summary>
    /// Retry specific failed job
    /// </summary>
    public bool RetryJob(Guid jobId)
    {
        if (_failedJobs.TryRemove(jobId, out var job))
        {
            job.RetryCount = 0;
            job.Status = PrintJobStatus.Pending;
            _pendingJobs.Enqueue(job);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Retry all failed jobs
    /// </summary>
    public int RetryAllFailed()
    {
        var count = 0;
        foreach (var jobId in _failedJobs.Keys.ToList())
        {
            if (RetryJob(jobId))
                count++;
        }
        return count;
    }

    /// <summary>
    /// Remove failed job from queue
    /// </summary>
    public bool RemoveFailedJob(Guid jobId)
    {
        return _failedJobs.TryRemove(jobId, out _);
    }

    /// <summary>
    /// Clear all failed jobs
    /// </summary>
    public void ClearFailedJobs()
    {
        _failedJobs.Clear();
    }

    private async Task ProcessQueueAsync(Func<TransactionDto, Task<bool>> printAction, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_pendingJobs.TryDequeue(out var job))
            {
                job.Status = PrintJobStatus.Processing;

                try
                {
                    var success = await printAction(job.Transaction);

                    if (success)
                    {
                        job.Status = PrintJobStatus.Completed;
                        job.CompletedAt = DateTime.Now;
                        OnJobCompleted(job);
                    }
                    else
                    {
                        await HandleFailedJobAsync(job, "Print returned false");
                    }
                }
                catch (Exception ex)
                {
                    await HandleFailedJobAsync(job, ex.Message);
                }
            }
            else
            {
                // No jobs, wait a bit
                await Task.Delay(100, ct);
            }
        }
    }

    private async Task HandleFailedJobAsync(PrintJob job, string error)
    {
        job.RetryCount++;
        job.LastError = error;

        if (job.RetryCount < _maxRetries)
        {
            job.Status = PrintJobStatus.Retrying;
            OnJobRetrying(job);

            await Task.Delay(_retryDelay);
            _pendingJobs.Enqueue(job);
        }
        else
        {
            job.Status = PrintJobStatus.Failed;
            _failedJobs[job.Id] = job;
            OnJobFailed(job);
        }
    }

    private void OnJobCompleted(PrintJob job)
    {
        JobCompleted?.Invoke(this, new PrintJobEventArgs { Job = job });
    }

    private void OnJobFailed(PrintJob job)
    {
        JobFailed?.Invoke(this, new PrintJobEventArgs { Job = job });
    }

    private void OnJobRetrying(PrintJob job)
    {
        JobRetrying?.Invoke(this, new PrintJobEventArgs { Job = job });
    }
}

/// <summary>
/// Print job model
/// </summary>
public class PrintJob
{
    public Guid Id { get; set; }
    public TransactionDto Transaction { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public PrintJobStatus Status { get; set; }
}

/// <summary>
/// Print job status
/// </summary>
public enum PrintJobStatus
{
    Pending,
    Processing,
    Retrying,
    Completed,
    Failed
}

/// <summary>
/// Event args for print job events
/// </summary>
public class PrintJobEventArgs : EventArgs
{
    public PrintJob Job { get; set; } = null!;
}
