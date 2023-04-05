using KnitTest.Entities;
using KnitTest.Models;
using KnitTest.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMvcCore();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IOTPService, OTPService>();
builder.Services.AddDbContext<OTPContext>(option => option.UseSqlServer(Configuration.GetConnectionString("DefaultString")));
var sendGridConfigSection = builder.Configuration.GetSection("SendGridConfig");
if (sendGridConfigSection.Exists())
{
    builder.Services.AddOptions<SendGridConfig>().Bind(sendGridConfigSection);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
