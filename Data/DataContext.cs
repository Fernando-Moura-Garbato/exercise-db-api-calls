using Microsoft.EntityFrameworkCore;
using Exercise.Entities;

namespace Exercise.Data;

// Classe de contexto do Entity Framework Core que gerencia a conexão com o banco de dados, usando SQLite e a mapeando a entidade Vehicle à tabela vehicles
class DataContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }

    public string DbPath { get; }

    public DataContext()
    {
        string folder = Environment.CurrentDirectory;
        DbPath = Path.Join(folder, "Data/vehicles_data.db");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
