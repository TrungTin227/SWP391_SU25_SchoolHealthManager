using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Services.Implementations;

namespace Services.Commons.Gmail
{
    public static class EmailServiceExtensions
    {
        // Overload 1: Nhận IConfiguration
        public static IServiceCollection AddEmailServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return AddEmailServicesCore(services, options =>
            {
                configuration.GetSection("EmailSettings").Bind(options);

                // School specific configurations
                options.SchoolName = configuration["SchoolSettings:Name"] ?? "Trường THCS ABC";
                options.SchoolAddress = configuration["SchoolSettings:Address"] ?? "123 Đường ABC, Quận XYZ, TP.HCM";
                options.SchoolPhone = configuration["SchoolSettings:Phone"] ?? "028.1234.5678";
                options.HealthDepartmentEmail = configuration["SchoolSettings:HealthDepartmentEmail"] ?? "yteschool@school.edu.vn";
                options.SchoolLogoUrl = configuration["SchoolSettings:LogoUrl"] ?? "";
            });
        }

        // Overload 2: Nhận Action<EmailSettings>
        public static IServiceCollection AddEmailServices(
            this IServiceCollection services,
            Action<EmailSettings> configureOptions)
        {
            return AddEmailServicesCore(services, configureOptions);
        }

        // Core method chung
        private static IServiceCollection AddEmailServicesCore(
            IServiceCollection services,
            Action<EmailSettings> configureOptions)
        {
            // Configure email settings
            services.Configure(configureOptions);

            // Register core email services
            services.AddSingleton<EmailQueue>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<ISchoolHealthEmailService, SchoolHealthEmailService>();

            // Register background service
            services.AddHostedService<EmailBackgroundService>();

            // Register jobs
            services.AddTransient<SchoolHealthEmailJob>();

            // Configure Quartz
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var dailyReportJobKey = new JobKey("DailyHealthReportJob");
                q.AddJob<SchoolHealthEmailJob>(opts => opts.WithIdentity(dailyReportJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(dailyReportJobKey)
                    .WithIdentity("DailyHealthReportJob-trigger")
                    .WithCronSchedule("0 0 7 * * ?")
                );

                var weeklyReportJobKey = new JobKey("WeeklyHealthSummaryJob");
                q.AddJob<SchoolHealthEmailJob>(opts => opts.WithIdentity(weeklyReportJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(weeklyReportJobKey)
                    .WithIdentity("WeeklyHealthSummaryJob-trigger")
                    .WithCronSchedule("0 0 8 ? * MON")
                );
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}