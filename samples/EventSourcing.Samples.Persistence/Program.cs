using EventSourcing.Net;
using EventSourcing.Samples.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using EventSourcing.Samples.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// register db context
builder.Services.RegisterDbContext(builder.Configuration);
// register event sourcing
builder.Services.RegisterEventSourcing(builder.Configuration);

var app = builder.Build();

// create databases for example
await app.CreateDatabases();
// run event sourcing engine
await app.Services.StartEventSourcingEngine();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();