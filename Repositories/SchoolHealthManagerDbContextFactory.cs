using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Repositories
{
    public class SchoolHealthManagerDbContextFactory
    : IDesignTimeDbContextFactory<SchoolHealthManagerDbContext>
    {
        public SchoolHealthManagerDbContext CreateDbContext(string[] args)
        {
            // 1. Đặt basePath về thư mục chứa file .csproj của WebAPI, không phải bin folder:
            var basePath = Directory.GetCurrentDirectory();

            // 2. Build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets("6ec4b334-2a87-4b7f-a2a2-30ef560492b8")  // chính là UserSecretsId bạn đặt trong WebAPI.csproj
                .AddEnvironmentVariables()
                .Build();


            // 3. Lấy connection
            var connectionString = config.GetConnectionString("SchoolHealthManager");
            Console.WriteLine($"Using connection: {connectionString}");

            // 4. Build options
            var optionsBuilder = new DbContextOptionsBuilder<SchoolHealthManagerDbContext>();
            optionsBuilder.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly("Repositories")
            );

            return new SchoolHealthManagerDbContext(optionsBuilder.Options);
        }
    }

}
