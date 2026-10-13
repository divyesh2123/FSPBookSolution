using FSPBook.Data;
using FSPBook.Utilities;
using FSPBook.Repositroy.Implementations;
using FSPBook.Repositroy.Interfaces;
using FSPBook.OutboundService.Extensions;
using FSPBook.Service.Implementations;
using FSPBook.Service.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<Context>(options => options.UseInMemoryDatabase("FSPBookDataBase"));
builder.Services.AddRazorPages();

// Register Repository and Service dependencies
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Register outbound (third-party) HTTP clients with Polly resilience
builder.Services.AddOutboundServices(builder.Configuration);

var app = builder.Build();

SeedDatabase(app);

//Seed Data
void SeedDatabase(IHost app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

    using (var scope = scopedFactory.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            SeedData.Seed(services);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapRazorPages();

app.Run();