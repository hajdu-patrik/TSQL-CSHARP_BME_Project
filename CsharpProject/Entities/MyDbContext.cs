/**
 * Since the database has already been created in advance using Microsoft TSQL,
 * we generate a Code-First model from it using the Reverse Engineering Code First methodology!
 * 
 * Reverse Engineering Code First comand:
    * Scaffold-DbContext 'Data Source=(localdb)\MSSQLLocalDB;
    * Initial Catalog=CsharpProject;Integrated Security=True' Microsoft.EntityFrameworkCore.SqlServer -Context MyDbContext -OutputDir Entities
 */

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CsharpProject.Entities;

public partial class MyDbContext : DbContext
{
    public MyDbContext() {}
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) {}

    /**
     * These properties represent tables in the code.
     * db.Products in C#, EF Core will referring to the SQL [Product] table.
     * These are the "entry points" to the data.
     */
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Product> Products { get; set; }

    /**
     * This method tells DbContext where to connect.
     * The generator has hard-coded the connection string specified in the command here.
     */
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CsharpProject;Integrated Security=True");

    /**
     * The Scaffold-DbContext command analyzed the SQL tables, column types, primary keys, and foreign keys,
     * then described them in C# using the so-called Fluent API.
     * 
     * The Fluent API is a special way of describing database operations in C#.
     */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /**
         * Instead of having to manually JOIN the Category table, EF Core does it for you.
         * 
         * The HasOne and WithMany settings allow you to simply write product.Category.Name in C#,
         * and EF Core automatically generates the JOIN in the background.
         */
        modelBuilder.Entity<Category>(entity => {
            entity.HasKey(e => e.ID).HasName("PK__Category__3214EC27FB8E1682");

            entity.ToTable("Category");

            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity => {
            entity.HasKey(e => e.ID).HasName("PK__Order__3214EC27B09D79BA");

            entity.ToTable("Order");

            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(20);
            entity.Property(e => e.ProductID).HasColumnName("ProductID");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__ProductID__29572725");
        });

        modelBuilder.Entity<Product>(entity => {
            entity.HasKey(e => e.ID).HasName("PK__Product__3214EC27B8ACD718");

            entity.ToTable("Product");

            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.CategoryID).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasColumnType("xml");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Vatpercentage).HasColumnName("VATPercentage");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__267ABA7A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
