using APICatalogo_minimal.Context;
using APICatalogo_minimal.Models;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo_minimal.ApiEndpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this WebApplication app)
    {
        // Categorias
        // Get all items from Category
        app.MapGet("/categories", async (AppDbContext context) =>
        {
            var items = await context.Categorias.Where(cat => cat.CategoriaId < 200).ToListAsync();

            if (items is null) { return Results.NotFound(); } else { return Results.Ok(items); }
        }
        ).RequireAuthorization().WithTags("Categories");

        // Get an Category by id
        app.MapGet("/category/{id:int}", async (AppDbContext context, int id) =>
        {
            Categoria? search = await context.FindAsync<Categoria>(id);

            if (search is null) { return Results.NotFound("Id não encontrado."); }
            return Results.Ok(search);
        }
        ).RequireAuthorization().WithTags("Categories");

        // Create an new category register
        app.MapPost("/categories", async (Categoria categoria, AppDbContext context) =>
        {
            context.Add(categoria);
            await context.SaveChangesAsync();

            return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
        }
        ).RequireAuthorization().WithTags("Categories");

        // Update an category by id
        app.MapPut("/categories/{id:int}", async (AppDbContext context, int id, Categoria categoria) =>
        {
            if (categoria.CategoriaId != id) { return Results.BadRequest(); }

            Categoria? categoriaDB = await context.Categorias.FindAsync(id);

            if (categoriaDB is null)
            {
                return Results.NotFound();
            }

            categoriaDB.Name = categoria.Name;
            categoriaDB.Description = categoria.Description;

            await context.SaveChangesAsync();

            return Results.Ok(categoriaDB);
        }
        ).RequireAuthorization().WithTags("Categories");

        // Remove an category by id
        app.MapDelete("/categories/{id:int}", async (AppDbContext context, int id) =>
        {
            Categoria? categoria = context.Find<Categoria>(id);
            if (categoria is null) { return Results.NotFound(); }
            else
            {
                try
                {
                    context.Categorias.Remove(categoria);
                    await context.SaveChangesAsync();
                    return Results.Ok(categoria);
                }
                catch { return Results.Problem(); }
            }
        }
        ).RequireAuthorization().WithTags("Categories");
    }
}
