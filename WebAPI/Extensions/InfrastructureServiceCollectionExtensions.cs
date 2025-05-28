using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


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

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserEmailService, UserEmailService>();
            // 5. Email + Quartz
            services.AddEmailServices(opts =>
                configuration.GetSection("EmailSettings").Bind(opts)
            );

            // 6. Controllers
            services.AddControllers();

            return services;
        }
    }
}
