using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Xunit;

namespace Users.Tests
{
    public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UsersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Levantar API real en memoria
            _client = factory.CreateClient();
        }
        /// <summary>
        /// Verifica que el punto final de creación de usuario devuelva un estado de solicitud incorrecta 400
        /// y una respuesta de error JSON cuando se proporciona un número de teléfono no válido.
        /// </summary>
        /// <remarks>Esta prueba garantiza que la API valide correctamente el formato del número de teléfono durante la creación del usuario.
        /// Comprueba que la respuesta de error contenga el mensaje de error esperado relacionado con la validación del número de teléfono,
        /// lo que confirma que la entrada no válida se gestiona correctamente.</remarks>
        /// <returns>Una tarea que representa la operación de prueba asincrónica.</returns>
        [Fact]
        public async Task PostUser_DebeRetornar400YJsonError_CuandoElTelefonoEsInvalido()
        {
            // Arrange: Un usuario con teléfono de solo 3 dígitos (inválido)
            var usuarioInvalido = new
            {
                email = "test@error.com",
                name = "Test Error",
                phone = "123",
                tax_id = "ABCD990101XYZ",
                password = "password123"
            };

            // Act: Llamada HTTP POST real a tu controlador
            var response = await _client.PostAsJsonAsync("/users", usuarioInvalido);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Validar que el cuerpo del JSON sea el que configuró en el Filtro Global
            var errorBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            errorBody.Should().NotBeNull();
            errorBody!.Error.Should().BeTrue();
            errorBody.Message.Should().Contain("phone");
        }

        // Clase auxiliar para mapear la respuesta del filtro
        private class ErrorResponse
        {
            public bool Error { get; set; }
            public string Message { get; set; } = "";
        }
    }
}
