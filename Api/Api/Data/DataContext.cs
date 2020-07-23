using Api.Entities;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GraphEntity> GraphEntities { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}
