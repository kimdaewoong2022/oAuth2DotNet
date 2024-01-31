using IdentityServer;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// IdentityServer4 종속성 주입
//builder.Services.AddIdentityServer();
builder.Services.AddIdentityServer()
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryApiScopes(Config.ApiScopes)
    //.AddInMemoryIdentityResources(Config.IdentityResources)
    //.AddTestUsers(Config.TestUsers)
    .AddDeveloperSigningCredential();

var app = builder.Build();

app.UseRouting();
// IdentityServer4 미들웨어 추가
app.UseIdentityServer();

app.MapGet("/", () => "Hello World!");

app.Run();
