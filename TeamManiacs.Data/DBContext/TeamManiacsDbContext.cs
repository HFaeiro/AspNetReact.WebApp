using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeamManiacs.Core.Models;

namespace TeamManiacs.Data;

public partial class TeamManiacsDbContext : DbContext
{

    public TeamManiacsDbContext(DbContextOptions<TeamManiacsDbContext> options)
        : base(options)
    {
    }

    //public virtual DbSet<Login> Logins { get; set; }
    public virtual DbSet<Users> UserModels { get; set; }
    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<Video> Videos { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=.;Database=FlySpotsDB;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Login>(
        //    x =>
        //    {
        //        x.HasNoKey();
        //        x.Property(v => v.Username).HasColumnName("Username");
        //    }
        //    );
        var converter = new ValueConverter<List<int>?, string>(
                v => string.Join(";", v),
                v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(val => int.Parse(val)).ToList());
        modelBuilder.Entity<Video>(entity =>
        {

            entity.Property(e => e.ID).ValueGeneratedOnAdd();
                        
        });
        modelBuilder.Entity<Users>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedOnAdd();
            entity.Property(e => e.videos).HasConversion(converter);
        });
        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
