using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// IdentityServer4 ���Ӽ� ����
//builder.Services.AddIdentityServer();
builder.Services.AddIdentityServer()
    .AddInMemoryClients(new List<Client>())
    .AddInMemoryApiScopes(new List<ApiScope>())
    .AddInMemoryIdentityResources(new List<IdentityResource>())
    .AddTestUsers(new List<TestUser>())
    .AddDeveloperSigningCredential();

var app = builder.Build();

app.UseRouting();
// IdentityServer4 �̵���� �߰�
app.UseIdentityServer();

app.MapGet("/", () => "Hello World!");

app.Run();
