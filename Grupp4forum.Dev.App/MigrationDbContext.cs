using Microsoft.EntityFrameworkCore;

namespace Grupp4forum.Dev.App
{
    public class MigrationDbContext : DbContext
    {
        public MigrationDbContext(DbContextOptions<MigrationDbContext> options)
            : base(options)
        {
        }

    }
}

