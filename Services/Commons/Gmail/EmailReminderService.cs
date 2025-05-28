using Microsoft.Extensions.Hosting;
using Quartz;

namespace Services.Commons.Gmail
{
    public class EmailReminderService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;

        public EmailReminderService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            await _scheduler.Start(cancellationToken);

            var job = JobBuilder.Create<SendEmailJob>()
                .WithIdentity("sendEmailJob", "emailGroup")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("sendEmailTrigger", "emailGroup")
                .StartNow()
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(10, 00))
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown(cancellationToken);
            }
        }
    }
}