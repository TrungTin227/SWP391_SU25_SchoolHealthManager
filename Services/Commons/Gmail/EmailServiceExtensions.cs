using Quartz;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Commons.Gmail
{
    public static class EmailServiceExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, Action<EmailSettings> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddSingleton<EmailQueue>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<SendEmailJob>();
            services.AddHostedService<EmailBackgroundService>();
            services.AddHostedService<EmailReminderService>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey("SendEmailJob");
                q.AddJob<SendEmailJob>(opts => opts.WithIdentity(jobKey));

                 q.AddTrigger(opts => opts
                     .ForJob(jobKey)
                     .WithIdentity("SendEmailJob-trigger")
                     .StartNow()
                     .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever()));
            });



            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
