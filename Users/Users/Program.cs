using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore;
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Rest de Usuarios - Memordz",
        Version = "v1",
        Description = "### Sobre mí\n" +
                      "Full Stack Developer con más de 3 años de experiencia como Jefe de Unidad Departamental de Programación y Sistemas. " +
                      "Especializado en la gestión de equipos y desarrollo de soluciones con ASP Clásico y C# moderno.\n\n" +
                      "**Experiencia:** Creación de aplicaciones para agilizar procesos administrativos y apoyo en la toma de decisiones.\n" +
                      "**Nivel de Inglés:** Intermedio.",
        Contact = new OpenApiContact
        {
            Name = "Guillermo Rodríguez",
            Email = "gmo.rodriguez@gmail.com"
        }
    });
});
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
