using System.Reflection;
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
            // 1. Load configuration từ appsettings.json
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            // 2. Lấy connection string
            var connectionString = config.GetConnectionString("SchoolHealthManager");

            // 3. Build options cho DbContext
            var optionsBuilder = new DbContextOptionsBuilder<SchoolHealthManagerDbContext>();
            optionsBuilder.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly("Repositories")
            );

            // 4. Trả về instance context
            return new SchoolHealthManagerDbContext(optionsBuilder.Options);
        }
    }
}
