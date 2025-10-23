using DevExpress.Blazor;
using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Repositories;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services;
using LE_Digital_2_Blazor_Server_WebApp.Server.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// *** CHANGE THIS: Use AddDbContextFactory ***
// This registers the factory as Singleton and manages DbContext instances properly
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDevExpressBlazor();

// Register your custom services
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserService, UserService>(); // Keep services Scoped
builder.Services.AddScoped<IVersionService, VersionService>(); // Keep services Scoped
builder.Services.AddScoped<IEmailService, EmailService>(); // Keep services Scoped
builder.Services.AddScoped<IDirectorService, DirectorService>(); // Add DirectorService registration
builder.Services.AddScoped<AppState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();