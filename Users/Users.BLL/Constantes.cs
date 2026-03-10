using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Users.BLL
{
    public static class Constantes
    {
        public static class Seguridad
        {
            // Se cambia de 'const' por 'static' para llenarlo al arrancar
            public static string Semilla { get; set; } = string.Empty;
        }

        public static class Campos
        {
            public static class Usuario
            {
                public const string Clave = "password";
                public const string Email = "email";
                public const string Nombre = "name";
                public const string TaxId = "tax_id";
                public const string Telefono = "Phone";
            }
        }
    }
}
