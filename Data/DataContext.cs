using Microsoft.EntityFrameworkCore;
using Exercise.Entities;
using System.IO;

namespace Exercise.Data;

class DataContext : DbContext
{
    public DbSet<Veiculo> Veiculos { get; set; }

    public string DbPath { get; }

    public DataContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "veiculos.db");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
