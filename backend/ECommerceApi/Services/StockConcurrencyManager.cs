using System.Collections.Concurrent;

namespace ECommerceApi.Services;

/// <summary>
/// Manages per-product SemaphoreSlim locks for concurrent stock deduction.
/// Registered as a SINGLETON so the lock dictionary is shared across all HTTP requests.
/// This is the core mechanism that prevents overselling when multiple threads/requests
/// try to purchase the same product simultaneously.
///
/// Design:
/// - Uses ConcurrentDictionary for thread-safe lock lookup/creation.
/// - One SemaphoreSlim(1,1) per product ID — a per-product mutex.
/// - Product IDs are sorted before acquisition to prevent deadlocks (consistent lock ordering).
/// - Configurable timeout (default 30s) prevents hung requests from blocking forever.
/// - Returns a LockReleaser (IAsyncDisposable) for use with `await using`.
/// </summary>
public class StockConcurrencyManager : IDisposable
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _productLocks = new();
    private readonly TimeSpan _lockTimeout;
    private bool _disposed;

    public StockConcurrencyManager(TimeSpan? lockTimeout = null)
    {
        _lockTimeout = lockTimeout ?? TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Acquires exclusive locks for the given product IDs.
    /// IDs are sorted to ensure consistent lock ordering across all callers,
    /// eliminating the possibility of deadlocks.
    ///
    /// Usage: await using var releaser = await manager.AcquireLocksAsync(productIds);
    /// </summary>
    public async Task<LockReleaser> AcquireLocksAsync(IEnumerable<int> productIds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Sort to prevent deadlock — all callers acquire in same order
        var sortedIds = productIds.Distinct().OrderBy(id => id).ToList();
        var acquiredSemaphores = new List<SemaphoreSlim>(sortedIds.Count);

        try
        {
            foreach (var productId in sortedIds)
            {
                // GetOrAdd: atomic get-or-create for the semaphore
                var semaphore = _productLocks.GetOrAdd(productId, _ => new SemaphoreSlim(1, 1));

                // WaitAsync with timeout prevents hung requests from blocking forever
                var entered = await semaphore.WaitAsync(_lockTimeout);
                if (!entered)
                {
                    throw new TimeoutException(
                        $"Unable to acquire lock for product {productId} within {_lockTimeout.TotalSeconds}s. " +
                        "Another operation may be holding the lock too long.");
                }

                acquiredSemaphores.Add(semaphore);
            }

            return new LockReleaser(acquiredSemaphores);
        }
        catch
        {
            // Release any locks we already acquired before the failure
            foreach (var s in acquiredSemaphores)
            {
                s.Release();
            }
            throw;
        }
    }

    /// <summary>
    /// Returns the number of currently tracked product locks.
    /// Useful for diagnostics/monitoring.
    /// </summary>
    public int TrackedProductCount => _productLocks.Count;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var semaphore in _productLocks.Values)
        {
            semaphore.Dispose();
        }
        _productLocks.Clear();
    }
}

/// <summary>
/// Disposable struct that releases all acquired semaphores when disposed.
/// Released in reverse acquisition order as a best practice.
/// Designed for use with `await using`.
/// </summary>
public readonly struct LockReleaser : IAsyncDisposable
{
    private readonly List<SemaphoreSlim> _semaphores;

    public LockReleaser(List<SemaphoreSlim> semaphores)
    {
        _semaphores = semaphores;
    }

    public async ValueTask DisposeAsync()
    {
        // Release in reverse order (best practice, though not strictly required here)
        for (int i = _semaphores.Count - 1; i >= 0; i--)
        {
            _semaphores[i].Release();
        }
        await ValueTask.CompletedTask;
    }
}
