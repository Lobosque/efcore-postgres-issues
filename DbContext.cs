using Microsoft.EntityFrameworkCore;

namespace ConcurrencyPoC
{
    public class BaseContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
    }

    public class MysqlDbContext : BaseContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("Server=concurrency-check-mysql.cimyatvarevu.sa-east-1.rds.amazonaws.com;Database=concurrencycheck;Uid=admin;Pwd=concurrencycheck;")
        .UseSnakeCaseNamingConvention();
    }

    public class PostgresDbContext : BaseContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql("Server=concurrency-check-postgres.cimyatvarevu.sa-east-1.rds.amazonaws.com;Database=concurrencycheck;Uid=postgres;Pwd=concurrencycheck;")
        .UseSnakeCaseNamingConvention();
    }

    public static class ContextFactory
    {
        public static BaseContext CreateContext(string databaseType) {
            if(databaseType == "mysql") {
                return new MysqlDbContext();
            }
            return new PostgresDbContext();
        }
    }
}