using APICatalogo_minimal.Context;
using APICatalogo_minimal.Models;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo_minimal.ApiEndpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        // Products
        // Get all products
        app.MapGet("/products", async (AppDbContext context) =>
        {
            var produtos = await context.Produtos.Where(produto => produto.ProuctId < 200).ToListAsync();

            if (produtos is null) { return Results.NotFound(); }

            return Results.Ok(produtos);
        }
        ).RequireAuthorization().RequireAuthorization().WithTags("Products");
        // Get products by id
        app.MapGet("/prducts/{id:int}", async (AppDbContext context, int id) =>
        {
            var product = await context.Produtos.FindAsync(id);

            if (product is null) { Results.NotFound(); }

            Results.Ok(product);

        }).WithTags("Products");


        // Get products by Categorie
        app.MapGet("products/categorie/{id:int}", async (AppDbContext context, int id) =>
        {
            var products = await context.Produtos.Where(prod => prod.CategoriaId == id).ToListAsync();

            if (products is null) Results.NotFound();

            return Results.Ok(products);
        }
        ).RequireAuthorization().WithTags("Products");

        // Create an product
        app.MapPost("/products", async (AppDbContext context, Produto product) =>
        {
            context.Add<Produto>(product);
            await context.SaveChangesAsync();

            return Results.Created($"products/{product.ProuctId}", product);
        }
        ).RequireAuthorization().WithTags("Products");

        // Update an product by id
        app.MapPut("/products/{id:int}", async (AppDbContext context, int id, Produto produto) =>
        {
            if (produto.ProuctId != id) return Results.BadRequest();

            Produto? productDB = await context.Produtos.FindAsync(id);

            if (productDB is null) Results.NotFound();

            productDB.Name = produto.Name;
            productDB.Description = produto.Description;
            productDB.Price = produto.Price;
            productDB.BuyDate = produto.BuyDate;
            productDB.Stock = produto.Stock;
            productDB.CategoriaId = id;
            productDB.Immage = produto.Immage;

            await context.SaveChangesAsync();

            return Results.Ok(productDB);
        }
        ).RequireAuthorization().WithTags("Products");

        // Remove an product by id
        app.MapDelete("/products/{id:int}", async (AppDbContext context, int id) =>
        {
            Produto? produto = await context.Produtos.FindAsync(id);
            if (produto is null) Results.NotFound();

            try
            {
                context.Produtos.Remove(produto);
                await context.SaveChangesAsync();

                return Results.NoContent();
            }
            catch { return Results.Problem(); }
        }
        ).RequireAuthorization().WithTags("Products");
    }



}
