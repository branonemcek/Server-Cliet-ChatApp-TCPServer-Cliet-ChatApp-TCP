
using Microsoft.EntityFrameworkCore;

namespace ChatLogApi.Models;
public class ChatDbContext : DbContext
{
    public DbSet<ConnectionLog> ConnectionLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=chat.db");
    }
}