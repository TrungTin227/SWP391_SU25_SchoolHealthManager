using System.Collections.Concurrent;

namespace Services.Commons.Gmail
{
    public class EmailQueue
    {
        private readonly ConcurrentQueue<EmailRequest> _emailRequests = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void EnqueueEmail(EmailRequest emailRequest)
        {
            if (emailRequest == null)
            {
                throw new ArgumentNullException(nameof(emailRequest));
            }

            _emailRequests.Enqueue(emailRequest);
            _signal.Release();
        }

        public async Task<EmailRequest> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _emailRequests.TryDequeue(out var emailRequest);
            return emailRequest;
        }
    }
}