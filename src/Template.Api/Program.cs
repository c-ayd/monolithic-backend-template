using Cayd.AspNetCore.Settings.DependencyInjection;
using System.Reflection;
using Template.Application;
using Template.Infrastructure;
using Template.Persistence;

var builder = WebApplication.CreateBuilder(args);

//~ Begin - Add services
builder.Services.AddControllers();

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

builder.AddSettingsFromAssemblies(Assembly.GetAssembly(typeof(Program))!,
    Assembly.GetAssembly(typeof(Template.Persistence.ServiceRegistrations))!,
    Assembly.GetAssembly(typeof(Template.Infrastructure.ServiceRegistrations))!,
    Assembly.GetAssembly(typeof(Template.Application.ServiceRegistrations))!);
//~ End

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
