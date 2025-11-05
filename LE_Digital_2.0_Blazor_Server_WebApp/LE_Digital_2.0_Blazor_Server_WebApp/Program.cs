using DevExpress.Blazor;
using LE_Digital_2_Blazor_Server_WebApp.Core.Interfaces;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Data;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Repositories;
using LE_Digital_2_Blazor_Server_WebApp.Infrastructure.Services;
using LE_Digital_2_Blazor_Server_WebApp.Server.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ADD THIS: Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// ADD THIS: Setup Windows Authentication
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// ADD THIS: Make authorization required by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// *** CHANGE THIS: Use AddDbContextFactory ***
// This registers the factory as Singleton and manages DbContext instances properly
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });
builder.Services.AddDevExpressBlazor();

// Register your custom services
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserService, UserService>(); 
builder.Services.AddScoped<IVersionService, VersionService>(); 
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDirectorService, DirectorService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
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

// ADD THESE TWO LINES
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();