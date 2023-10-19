using APICatalogo_minimal.Context;
using APICatalogo_minimal.Models;
using APICatalogo_minimal.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space]. Example: \'Bearer 12345abcdef\'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
}
);

string? mySql = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySql, ServerVersion.AutoDetect(mySql))    
);

builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
}
);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// Endpoints definitions
// Login
app.MapPost("/login", [AllowAnonymous](UserModel userModel, ITokenService tokenService) =>
{
    if (userModel == null) { return Results.BadRequest(); }
    if(userModel.UserName =="felipe" && userModel.Password == "abc@123")
    {
        var tokenString = tokenService.Createtoken(app.Configuration["Jwt:Key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"],
            userModel);
        return Results.Ok(new {token = tokenString});
    }
    else
    {
        return Results.Unauthorized();
    }
}
).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status200OK).WithTags("Autenticacao");

// Categories
// Root
app.MapGet("/", () => $"Catalogo de Produtos /n ultima atualização em: {DateTime.Now.Year}/{DateTime.Now.Month}").ExcludeFromDescription();

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

// Products
// Get all products
app.MapGet("/products", async (AppDbContext context) =>
    {
        var produtos = await context.Produtos.Where(produto=> produto.ProuctId < 200).ToListAsync();

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

    if(productDB is null) Results.NotFound();

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
    if(produto is null) Results.NotFound();

    try
    {
        context.Produtos.Remove(produto);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }
    catch { return Results.Problem(); }
}
).RequireAuthorization().WithTags("Products");


app.UseAuthentication();
app.UseAuthorization();

app.Run();

