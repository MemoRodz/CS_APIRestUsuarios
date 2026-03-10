using Moq;
using FluentAssertions;
using Users.BLL;
using Users.DAL;
using Users.DAL.Models;

namespace Users.Tests
{
    public class UserServiceTests
    {
        #region Probando Seguridad
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly UserService _service;
        public UserServiceTests()
        {
            // Setup inicial para todas las pruebas
            _mockRepo = new Mock<IUserRepository>();
            _service = new UserService(_mockRepo.Object);

            // Importante: Asegurar que la Semilla esté cargada para las pruebas
            // La clase Constantes es estática, asígnar un valor aquí:
            Constantes.Seguridad.Semilla = "EstaEsMiClaveSecretaSuperSeguraDe32Bytes";
        }
        /// <summary>
        /// Verifica que el proceso de inicio de sesión devuelva <see langword="true"/> cuando se proporcionan credenciales válidas.
        /// </summary>
        /// <remarks>Esta prueba unitaria simula un intento de inicio de sesión exitoso comparando el ID fiscal 
        /// y la contraseña de texto simple proporcionados con la contraseña cifrada almacenada en el repositorio. 
        /// Garantiza que el método de inicio de sesión autentique correctamente a los usuarios 
        /// cuando las credenciales sean correctas.</remarks>
        [Fact]
        public void Login_DebeRetornarTrue_CuandoCredencialesSonCorrectas()
        {
            // Arrange
            var taxId = "EEEV040404BBB";
            var passwordPlano = "claveTest123";

            // Encriptar manualmente para simular lo que estaría en el "Repo"
            // Nota: Revisar AesEncrypt sea internal
            var passwordEncriptado = InvokeAesEncrypt(passwordPlano);

            var usuarioEnRepo = new User
            {
                TaxId = taxId,
                Password = passwordEncriptado
            };

            // Simular que el repo encuentra al usuario
            _mockRepo.Setup(r => r.GetByTaxId(taxId)).Returns(usuarioEnRepo);

            // Act
            var resultado = _service.Login(taxId, passwordPlano);

            // Assert (FluentAssertions)
            resultado.Should().BeTrue("porque la contraseña desencriptada debe coincidir con la enviada");
        }
        /// <summary>
        /// Valida que la funcionalidad de inicio de sesión devuelva <see langword="false"/> cuando 
        /// se proporciona una contraseña incorrecta para una identificación fiscal de usuario válida.
        /// </summary>
        /// <remarks>Esta prueba garantiza que el proceso de inicio de sesión rechace correctamente las credenciales inválidas 
        /// simulando un intento de inicio de sesión con un usuario válido y una contraseña incorrecta. 
        /// Confirma que el mecanismo de autenticación no permite el acceso cuando 
        /// la contraseña no coincide con el valor almacenado.</remarks>
        [Fact]
        public void Login_DebeRetornarFalse_CuandoPasswordEsErroneo()
        {
            // Arrange
            var taxId = "USER123";
            var usuarioEnRepo = new User { TaxId = taxId, Password = InvokeAesEncrypt("passwordReal") };
            _mockRepo.Setup(r => r.GetByTaxId(taxId)).Returns(usuarioEnRepo);

            // Act
            var resultado = _service.Login(taxId, "passwordEquivocado");

            // Assert
            resultado.Should().BeFalse("el login no debe permitir contraseñas incorrectas");
        }
        /// <summary>
        /// Cifra el texto plano especificado utilizando el algoritmo de cifrado AES.
        /// </summary>
        /// <remarks>Este método invoca un método estático privado 'AesEncrypt' para realizar el cifrado. 
        /// Asegúrese de que la entrada sea válida para evitar excepciones durante el proceso de cifrado.</remarks>
        /// <param name="plainText">La cadena de texto sin formato que se va a cifrar. Este valor no puede ser nulo ni estar vacío.</param>
        /// <returns>Una cadena que representa la versión cifrada del texto sin formato proporcionado.</returns>
        private string InvokeAesEncrypt(string plainText)
        {
            var method = typeof(UserService).GetMethod("AesEncrypt",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (string)method.Invoke(null, new object[] { plainText });
        }

        #endregion

        #region Funcionalidades API
        /// <summary>
        /// Verifica que el método PostUser encripte la contraseña cuando se proporciona un usuario válido.
        /// </summary>
        /// <remarks>Esta prueba garantiza que la contraseña no se almacene en texto plano 
        /// y que el usuario se agregue correctamente al repositorio. 
        /// Es importante para mantener la seguridad, ya que confirma que la información 
        /// confidencial se gestiona correctamente durante la creación del usuario.</remarks>
        [Fact]
        public void PostUser_DebeEncriptarPassword_CuandoEsValido()
        {
            // Arrange (Preparar)
            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object);
            var user = new User
            {
                TaxId = "AAAA990101XXX",
                Password = "password123",
                Phone = "1234567890"
            };
            // Act (Ejecutar)
            var result = service.PostUser(user);

            // Assert (Verificar)
            Assert.NotEqual("password123", result.Password); // No debe ser plano
            Assert.NotNull(result.Password);
            mockRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        }
        /// <summary>
        /// Verifica que el método PostUser genere una InvalidOperationException 
        /// al intentar agregar un usuario con un ID fiscal duplicado.
        /// </summary>
        /// <remarks>Esta prueba simula un escenario donde el repositorio ya 
        /// contiene un usuario con el NIF especificado. 
        /// Garantiza que el servicio evite correctamente los NIF duplicados 
        /// mediante la generación de la excepción correspondiente.</remarks>
        [Fact]
        public void PostUser_DebeLanzarExcepcion_SiTaxIdDuplicado()
        {
            var mockRepo = new Mock<IUserRepository>();
            // Simular que el TaxId ya existe
            mockRepo.Setup(r => r.GetByTaxId(It.IsAny<string>())).Returns(new User());

            var service = new UserService(mockRepo.Object);
            var user = new User { TaxId = "DUPLICADO123" };

            Assert.Throws<InvalidOperationException>(() => service.PostUser(user));
        }
        /// <summary>
        /// Verifica que el método de recuperación de usuarios filtre correctamente 
        /// a los usuarios por nombre cuando se utiliza el operador 'contiene' en la cadena de filtro.
        /// </summary>
        /// <remarks>Esta prueba garantiza que solo se devuelvan los usuarios cuyos nombres incluyan la subcadena especificada. 
        /// También verifica que la información confidencial, como las contraseñas, se excluya de los resultados, 
        /// lo que confirma la correcta protección de datos durante el proceso de filtrado.</remarks>
        [Fact]
        public void GetUsers_DebeFiltrarPorNombre_CuandoSeUsaOperadorContiene()
        {
            // Arrange
            var listaFicticia = new List<User>
            {
                new User { Name = "Carlos Magno", Email = "carlos@test.com" },
                new User { Name = "Juan Perez", Email = "juan@test.com" },
                new User { Name = "Ana Maria", Email = "ana@test.com" }
            };

            _mockRepo.Setup(r => r.GetAll()).Returns(listaFicticia);

            // Act: Filtro tipo "name+co+carlos" (Nombre contiene 'carlos')
            var filtro = "name+co+carlos";
            var resultado = _service.GetUsers(null, filtro).ToList();

            // Assert
            resultado.Should().HaveCount(1, "porque solo hay un 'Carlos' en la lista");
            resultado.First().Name.Should().Be("Carlos Magno");
            resultado.All(u => u.Password == null).Should().BeTrue("porque el password debe ocultarse en los resultados");
        }
        /// <summary>
        /// Verifica que la recuperación de usuarios no modifique ni elimine la información de contraseña del repositorio.
        /// </summary>
        /// <remarks>Esta prueba garantiza que el método GetUsers devuelva objetos de usuario con 
        /// el campo de contraseña excluido, mientras que los datos originales del usuario en 
        /// el repositorio permanecen inalterados. Valida que no se exponga información confidencial 
        /// ni que la fuente de datos se altere como consecuencia de la operación.</remarks>
        [Fact]
        public void GetUsers_NoDebeBorrarPasswordsDelRepositorio_CuandoSeEjecuta()
        {
            // Arrange
            var passOriginal = "hash_secreto_aes";
            var listaFicticia = new List<User> {
                new User { TaxId = "TEST1", Password = passOriginal }
            };
            _mockRepo.Setup(r => r.GetAll()).Returns(listaFicticia);

            // Act
            var listaPublica = _service.GetUsers(null, null).ToList();

            // Assert
            listaPublica.First().Password.Should().BeNull("porque la salida debe ser limpia");

            // VERIFICACIÓN CRÍTICA: El objeto en la lista original NO debe haber cambiado
            listaFicticia.First().Password.Should().Be(passOriginal,
                "porque el proceso de GetUsers no debe afectar la base de datos en memoria");
        }
        #endregion

    }
}
