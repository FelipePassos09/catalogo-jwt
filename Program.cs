using APICatalogo_minimal.ApiEndpoints;
using APICatalogo_minimal.AppServicesExtensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddApiSwagger();
builder.AddAuthentication();
builder.AddPersistence();
builder.Services.AddCors();

var app = builder.Build();

// Root
app.MapGet("/", () => $"Catalogo de Produtos /n ultima atualização em: {DateTime.Now.Year}/{DateTime.Now.Month}").ExcludeFromDescription();

// Endpoints definitions
app.MapAuthenticationEndpoints();
app.MapCategoriesEndpoints();
app.MapProductsEndpoints();

// Configure the environment configs.
var environment = app.Environment;
app.UseExceptionHandling(environment)
    .UseSwaggerMiddleware()
    .UseAppCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

