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
            public const string Semilla = "miClaveSecreta32bytes123";
        }

        public static class Campos
        {
            public static class Usuario
            {
                public const string TaxId = "tax_id";
                public const string Email = "email";
                public const string Clave = "password";
            }
        }
    }
}
