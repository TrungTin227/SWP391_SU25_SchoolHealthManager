using System.Collections.Concurrent;

namespace Services.Commons.Gmail
{
    public class EmailQueue
    {
        private readonly ConcurrentQueue<EmailRequest> _emailRequests = new();
        private readonly SemaphoreSlim _signal = new(0);
        private volatile bool _isDisposed = false;

        public void EnqueueEmail(EmailRequest emailRequest)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EmailQueue));

            if (emailRequest == null)
                throw new ArgumentNullException(nameof(emailRequest));

            _emailRequests.Enqueue(emailRequest);
            _signal.Release();
        }

        public async Task<EmailRequest?> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed)
                return null;

            try
            {
                await _signal.WaitAsync(cancellationToken);

                if (_emailRequests.TryDequeue(out var emailRequest))
                    return emailRequest;

                return null;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _signal?.Dispose();
        }
    }
}