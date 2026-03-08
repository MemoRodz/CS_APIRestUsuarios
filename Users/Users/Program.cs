using Microsoft.Extensions.Options;
using Users.API;
using Users.BLL;
using Users.DAL;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers(options => 
{
    options.Filters.Add<GlobalExceptionFilter>();
});
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Swagger (auxiliar para documentar)
builder.Services.AddSwaggerGen();
//builder.Services.AddEndpointsApiExplorer();

// DI de las capas
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<UserService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
