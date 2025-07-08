using Template.Infrastructure;
using Template.Persistence;

var builder = WebApplication.CreateBuilder(args);

//~ Begin - Add services
builder.Services.AddControllers();

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices();
//~ End

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
