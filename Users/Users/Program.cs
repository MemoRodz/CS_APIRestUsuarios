using Users.API;
using Users.BLL;
using Users.DAL;

var builder = WebApplication.CreateBuilder(args);

// ASIGNACIÓN: Leemos del appsettings y se asigna a la clase de Constantes
Constantes.Seguridad.Semilla = builder.Configuration["Seguridad:Semilla"]
                               ?? throw new Exception("Falta la Semilla en appsettings.json");
// Controllers
builder.Services.AddControllers(options => 
{
    options.Filters.Add<GlobalExceptionFilter>();
});
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Swagger (auxiliar para documentar)
builder.Services.AddSwaggerGen();
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
