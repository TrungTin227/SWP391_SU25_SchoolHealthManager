using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repositories.Implementations;
using Repositories.Interfaces;
using System.Text;


namespace WebAPI.Extensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Cấu hình Settings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // 2. DbContext và CORS
            services.AddDbContext<SchoolHealthManagerDbContext>(opt =>
                opt.UseSqlServer(
                    configuration.GetConnectionString("SchoolHealthManager"),
                    sql => sql.MigrationsAssembly("Repositories")));
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", b => b
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            // 3. Identity & Authentication
            services.AddIdentity<User, Role>(opts =>
            {
                // Bắt buộc phải xác thực email mới cho SignIn
                opts.SignIn.RequireConfirmedEmail = true;

                opts.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
                opts.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opts.Lockout.AllowedForNewUsers = true;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireDigit = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<SchoolHealthManagerDbContext>()
            .AddDefaultTokenProviders();

            var jwt = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                      ?? throw new InvalidOperationException("JWT key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.ValidIssuer,
                    ValidAudience = jwt.ValidAudience,
                    IssuerSigningKey = key
                };
                // Custom error handling
                opts.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        var res = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            message = "You are not authorized. Please authenticate."
                        });
                        return ctx.Response.WriteAsync(res);
                    }
                };
            });

            // 4. Repositories & Domain Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ISessionStudentRepository, SessionStudentRepository>();
            services.AddScoped<IHealProfileRepository, HealProfileRepository>();
            services.AddScoped<IParentRepository, ParentRepository>();
            services.AddScoped<IParentMedicationDeliveryRepository, ParentMedicationDeliveryRepository>();
            services.AddScoped<IMedicationRepository, MedicationRepository>(); 
            services.AddScoped<IMedicationLotRepository, MedicationLotRepository>();
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            services.AddScoped<IMedicalSupplyLotRepository, MedicalSupplyLotRepository>();
            services.AddScoped<IMedicalSupplyRepository, MedicalSupplyRepository>();
            services.AddScoped<IHealthEventRepository, HealthEventRepository>();
            services.AddScoped<ICurrentTime, CurrentTime>();
            services.AddScoped<IVaccineDoseInfoRepository, VaccineDoseInfoRepository>();
            services.AddScoped<IVaccineTypeRepository, VaccineTypeRepository>();
            services.AddScoped<IVaccineLotRepository, VaccineLotRepository>();
            services.AddScoped<IVaccinationCampaignRepository, VaccinationCampaignRepository>();
            services.AddScoped<IVaccinationScheduleRepository, VaccinationScheduleRepository>();
            services.AddScoped<IParentVaccinationRepository, ParentVaccinationRepository>();
            services.AddScoped<ICheckupCampaignRepository, CheckupCampaignRepository>();
            services.AddScoped<ICheckupScheduleRepository, CheckupScheduleRepository>(); 
            services.AddScoped<ICounselingAppointmentRepository, CounselingAppointmentRepository>();
            services.AddScoped<ICheckupRecordRepository, CheckupRecordRepository>();
            services.AddScoped<INurseProfileRepository, NurseProfileRepository>();
            services.AddScoped<IVaccinationRecordRepository, VaccinationRecordRepository>();
            services.AddScoped<IParentVaccinationRecordRepository, ParentVaccinationRecordRepository>();
            services.AddScoped<IMedicationUsageRecordRepository, MedicationUsageRecordRepository>();
            services.AddScoped<IParentMedicationDeliveryDetailRepository, ParentMedicationDeliveryDetailRepository>();
            services.AddScoped<IMedicationScheduleRepository, MedicationScheduleRepository>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IParentService, ParentService>();
            services.AddScoped<IHealProfileService, HealProfileService>();
            services.AddScoped<IParentMedicationDeliveryService, ParentMedicationDeliveryService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ISessionStudentService, SessionStudentService>();
            services.AddScoped<IUserEmailService, UserEmailService>();
            services.AddScoped<IMedicationService, MedicationService>();
            services.AddScoped<IMedicationLotService, MedicationLotService>();
            services.AddScoped<IMedicalSupplyLotService, MedicalSupplyLotService>();
            services.AddScoped<IMedicalSupplyService, MedicalSupplyService>();
            services.AddScoped<IHealthEventService, HealthEventService>();
            services.AddScoped<IVaccineDoseInfoService, VaccineDoseInfoService>();
            services.AddScoped<IVaccineTypeService, VaccineTypeService>();
            services.AddScoped<IVaccineLotService, VaccineLotService>();
            services.AddScoped<IVaccinationCampaignService, VaccinationCampaignService>();
            services.AddScoped<IVaccinationScheduleService, VaccinationScheduleService>();
            services.AddScoped<IParentVaccinationService, ParentVaccinationService>();
            services.AddScoped<ICheckupCampaignService, CheckupCampaignService>();
            services.AddScoped<ICheckupScheduleService, CheckupScheduleService>();
            services.AddScoped<ICounselingAppointmentService, CounselingAppointmentService>();
            services.AddScoped<ICheckupRecordService, CheckupRecordService>();
            services.AddScoped<IVaccinationRecordService, VaccinationRecordService>();
            services.AddScoped<INurseProfileService, NurseProfileService>();
            services.AddScoped<IMedicationUsageRecordService, MedicationUsageRecordService>();
            services.AddScoped<IParentMedicationDeliveryDetailService, ParentMedicationDeliveryDetailService>();
            // 5. Email + Quartz
            services.AddEmailServices(options =>
            {
                configuration.GetSection("EmailSettings").Bind(options);
                options.SchoolName = "Trường Tiểu học Lê Văn Việt";
            });

            // 6. Controllers
            services.AddControllers();

            return services;
        }
    }
}
