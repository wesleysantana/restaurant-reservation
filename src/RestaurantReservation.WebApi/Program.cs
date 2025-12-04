using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using RestaurantReservation.Identity.Context;
using RestaurantReservation.WebApi.Extensions;
using RestaurantReservation.WebApi.Setup;
using Serilog;
using Identity = RestaurantReservation.Identity.Context;
using Infra = RestaurantReservation.Infra.Context;

Log.Logger = new LoggerConfiguration()    
    .WriteTo.Console() // Configure um sink para o console para ver os logs iniciais
    .CreateBootstrapLogger(); // Cria um logger leve para uso inicial

var currentDir = Directory.GetCurrentDirectory();
string envPath = Path.Combine(currentDir, "..", "..", ".env");
string normalizedPath = Path.GetFullPath(envPath);
if (!File.Exists(normalizedPath))
{
    Log.Warning($"ARQUIVO .ENV NÃO ENCONTRADO. O caminho esperado era: {normalizedPath}");
    return;
}

Env.Load(normalizedPath);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Infra.ConfigService.RegisterServices(builder.Services, builder.Configuration);
Identity.ConfigService.RegisterServices(builder.Services, builder.Configuration);

builder.Services.AddDataProtection();
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddSignInManager<SignInManager<IdentityUser>>()
.AddEntityFrameworkStores<IdentityDataContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(builder.Configuration);
DependencyInjector.RegisterServices(builder.Services);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await IdentitySeeder.SeedAsync(app.Services, builder.Configuration);

app.Run();