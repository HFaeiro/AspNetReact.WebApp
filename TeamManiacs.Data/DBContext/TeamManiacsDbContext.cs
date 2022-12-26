using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TeamManiacs.Core.Models;

namespace TeamManiacs.Data;

public partial class TeamManiacsDbContext : DbContext
{

    public TeamManiacsDbContext(DbContextOptions<TeamManiacsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Login> Logins { get; set; }
    public virtual DbSet<Users> UserModels { get; set; }
    public virtual DbSet<Item> Items { get; set; }


//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=.;Database=FlySpotsDB;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Login>(
            x =>
            {
                x.HasNoKey();
                x.Property(v => v.Username).HasColumnName("Username");
            }
            );
        modelBuilder.Entity<Users>(entity =>
        {
            entity.Property(e => e.UserId).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
