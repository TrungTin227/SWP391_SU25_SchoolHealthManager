using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services
      .AddInfrastructure(builder.Configuration)
      .AddSwaggerServices()
      .AddCustomServices(); 

var app = builder.Build();


app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Health API V1");
    c.RoutePrefix = string.Empty;
});

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SchoolHealthManagerDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();

        logger.LogInformation("Seeding database...");
        await DBInitializer.Initialize(db, userManager);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database migration or seeding.");
}

app.UseHttpsRedirection();

app.UseRouting(); 

app.UseCors("CorsPolicy"); 

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    await next();
});

app.UseAuthentication();
app.UseMiddleware<SecurityStampValidationMiddleware>(); // Middleware Security Stamp của bạn
app.UseAuthorization();

app.MapControllers();

app.Run();