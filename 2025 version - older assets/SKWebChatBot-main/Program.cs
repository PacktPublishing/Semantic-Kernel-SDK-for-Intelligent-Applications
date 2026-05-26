using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using SKWebChatBot.Data;
using SKWebChatBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = "Data Source=chatbot.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ChatService>();    
builder.Services.AddSingleton<SemanticKernelService>();
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
