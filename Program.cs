using EmailRelayServer.Data;
using EmailRelayServer.DTOs;
using EmailRelayServer.Models;
using EmailRelayServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);

var config = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(config.GetConnectionString("Default")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = "emailrelay",
            ValidAudience = "emailrelay",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapPost("/register", async (RegisterRequest req, AppDbContext db, AuthService auth) =>
{
    var exists = await db.Users.AnyAsync(u => u.Email == req.Email);
    if (exists) return Results.Conflict("User already exists");

    var user = new User
    {
        Email = req.Email,
        PasswordHash = auth.HashPassword(req.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok("User registered");
});

app.MapPost("/login", async (LoginRequest req, AppDbContext db, AuthService auth) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
    if (user is null || !auth.VerifyPassword(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = auth.GenerateJwtToken(user.Email);
    return Results.Ok(new { token });
});

app.MapPost("/send", async (SendEmailRequest req, EmailService emailService, ClaimsPrincipal user) =>
{
    var senderEmail = user.FindFirst(ClaimTypes.Email)?.Value;
    if (senderEmail == null) return Results.Unauthorized();

    await emailService.SendEmailAsync(req.To, req.Subject, req.Body);
    return Results.Ok($"Email sent by {senderEmail}");
}).RequireAuthorization();

app.MapPost("/contact", async (ContactFormRequest req, EmailService email, ClaimsPrincipal user) =>
{

    var senderEmail = user.FindFirst(ClaimTypes.Email)?.Value;
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
        return Results.BadRequest("Name, email and message are required.");
    if (senderEmail != null)
    {
        await email.SendContactFormAsync(req, senderEmail);
        return Results.Ok("Your message has been delivered. Thank you!");
    }
    return Results.InternalServerError("Something went wrong on our side");
});


app.Run();
