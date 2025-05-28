using Microsoft.EntityFrameworkCore;

namespace WebAPI.Extensions
{
    public static class PipelineConfigurationExtensions
    {
        public static async Task<IApplicationBuilder> UseApplicationPipeline(this IApplicationBuilder app)
        {
            // 1. Chuẩn bị scope để migrate DB & Swagger
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var db = scope.ServiceProvider.GetRequiredService<SchoolHealthManagerDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                try
                {
                    db.Database.Migrate();
                    // nếu cần: await DBInitializer.Initialize(db);
                    await DBInitializer.Initialize(db, userManager);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error when migrating/seeding DB.");
                }

                if (env.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
            }

            // 2. Middleware chung
            app.UseHttpsRedirection();
            app.UseRouting();

            // 2.1. Kích hoạt CORS
            app.UseCors("CorsPolicy");

            // 2.2. Thêm header để cho phép popup OAuth Google postMessage về trang chủ
            app.Use(async (context, next) =>
            {
                // Phải đúng tên; có thể thay đổi hoa-thường, có 3 giá trị nằm trong ba token chuẩn.
                context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
                await next();
            });

            // 2.3. Xác thực & phân quyền
            app.UseAuthentication();
            app.UseMiddleware<SecurityStampValidationMiddleware>(); //Security Stamp
            app.UseAuthorization();

            // 3. Endpoint
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            return app;
        }
    }
}
