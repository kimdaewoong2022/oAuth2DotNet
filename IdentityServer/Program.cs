using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// IdentityServer4 종속성 주입
//builder.Services.AddIdentityServer();
builder.Services.AddIdentityServer()
    .AddInMemoryClients(new List<Client>())
    .AddInMemoryApiScopes(new List<ApiScope>())
    .AddInMemoryIdentityResources(new List<IdentityResource>())
    .AddTestUsers(new List<TestUser>())
    .AddDeveloperSigningCredential();



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
// IdentityServer4 미들웨어 추가
app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
