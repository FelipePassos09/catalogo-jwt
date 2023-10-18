using APICatalogo_minimal.Models;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo_minimal.Context;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    public DbSet<Produto>? Produtos { get; set; }
    public DbSet<Categoria>? Categorias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Categoria settings
        modelBuilder.Entity<Categoria>().HasKey(categoria => categoria.CategoriaId);
        modelBuilder.Entity<Categoria>().Property(categoria => categoria.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Categoria>().Property(categoria => categoria.Description).HasMaxLength(300);
        

        // Protuto settings
        modelBuilder.Entity<Produto>().HasKey(produto => produto.ProuctId);
        modelBuilder.Entity<Produto>().Property(produto => produto.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Produto>().Property(produto => produto.Description).HasMaxLength(300);
        modelBuilder.Entity<Produto>().Property(produto => produto.Price).HasPrecision(14,2);

        // Relationship
        modelBuilder.Entity<Produto>()
            .HasOne<Categoria>(categoria => categoria.Categoria)
            .WithMany(produto => produto.Produtos)
            .HasForeignKey(categoria => categoria.CategoriaId);
    }
}
