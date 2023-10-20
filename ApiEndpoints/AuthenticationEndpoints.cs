using APICatalogo_minimal.Models;
using APICatalogo_minimal.Services;
using Microsoft.AspNetCore.Authorization;

namespace APICatalogo_minimal.ApiEndpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        // Login
        app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
        {
            if (userModel == null) { return Results.BadRequest(); }
            if (userModel.UserName == "felipe" && userModel.Password == "abc@123")
            {
                var tokenString = tokenService.Createtoken(app.Configuration["Jwt:Key"],
                    app.Configuration["Jwt:Issuer"],
                    app.Configuration["Jwt:Audience"],
                    userModel);
                return Results.Ok(new { token = tokenString });
            }
            else
            {
                return Results.Unauthorized();
            }
        }
        ).Produces(StatusCodes.Status400BadRequest).Produces(StatusCodes.Status200OK).WithTags("Authentication");

    }
}
