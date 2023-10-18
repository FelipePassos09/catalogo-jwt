using APICatalogo_minimal.Context;
using APICatalogo_minimal.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}
);

string? mySql = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySql, ServerVersion.AutoDetect(mySql))    
);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints definitions
// Root
app.MapGet("/", () => $"Catalogo de Produtos /n ultima atualização em: {DateTime.Now.Year}/{DateTime.Now.Month}").ExcludeFromDescription();

// Categorias
// Get all items from Category
app.MapGet("/categories", async (AppDbContext context) =>
{
    var items = await context.Categorias.Where<Categoria>(cat => cat.CategoriaId < 200).ToListAsync();

    if (items is null) { return Results.NotFound(); } else { return Results.Ok(items); }
}
);

// Get an Category by id
app.MapGet("/category/{id:int}", async (AppDbContext context, int id) =>
{
    Categoria? search = await context.FindAsync<Categoria>(id);
    
    if (search is null) { return Results.NotFound("Id não encontrado."); }
    return Results.Ok(search);
}
);

// Create an new category register
app.MapPost("/categories", async (Categoria categoria, AppDbContext context) =>
{
    context.Add(categoria);
    await context.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
}
    );

// Update an category by id
app.MapPut("/categories/{id:int}", async (AppDbContext context,int id, Categoria categoria) =>
{
    if(categoria.CategoriaId != id) { return Results.BadRequest(); }

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
);

// Delete an category by id
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
);


app.Run();

