using Microsoft.EntityFrameworkCore;
using Exercise.Entities;
using System;
using System.IO;

namespace Exercise.Data;

class DataContext : DbContext
{
    public DbSet<Vehicle> Vehicles { get; set; }

    public string DbPath { get; }

    public DataContext()
    {
        string folder = Environment.CurrentDirectory;
        DbPath = Path.Join(folder, "Data/vehicles.db");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
