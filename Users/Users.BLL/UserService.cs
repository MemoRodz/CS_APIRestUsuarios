using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Users.DAL;
using Users.DAL.Models;
using Users.BLL;

namespace Users.BLL
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<User> GetUsers(string? sortedBy, string? filter)
        {
            var users = _repo.GetAll().ToList();

            // Filtro
            if (!string.IsNullOrEmpty(filter))
            {
                var parts = filter.Split('+', 3);
                if (parts.Length == 3)
                {
                    var attr = parts[0];
                    var op = parts[1];
                    var value = parts[2];
                    users = users.Where(u => MatchesFilter(u, attr, op, value)).ToList();
                }
            }

            // Ordenamiento
            if (!string.IsNullOrEmpty(sortedBy))
            {
                users = users.OrderBy(u => GetFieldValue(u, sortedBy)).ToList();
            }

            // Limpia password antes de regresar a la API
            //foreach (var u in users) u.Password = null!;
            //return users;
            return users.Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                Phone = u.Phone,
                TaxId = u.TaxId,
                CreatedAt = u.CreatedAt,
                Addresses = u.Addresses,
                Password = null // Solo esta copia va con null
            }).ToList();
        }

        private static string GetFieldValue(User u, string field) =>
            field.ToLower() switch
            {
                "email" => u.Email,
                "id" => u.Id.ToString(),
                "name" => u.Name,
                "phone" => u.Phone,
                "tax_id" => u.TaxId,
                "created_at" => u.CreatedAt,
                _ => ""
            };

        private static bool MatchesFilter(User u, string attr, string op, string value)
        {
            var v = value.ToLower();
            var f = GetFieldValue(u, attr).ToLower();
            return op switch
            {
                "co" => f.Contains(v),
                "eq" => f == v,
                "sw" => f.StartsWith(v),
                "ew" => f.EndsWith(v),
                _ => false
            };
        }

        public User? GetUserById(Guid id)
        {
            var originalUser = _repo.GetById(id);
            if (originalUser == null) return null;

            // Seguridad: nunca devolver el hash
            return new User
            {
                Id = originalUser.Id,
                Email = originalUser.Email,
                Name = originalUser.Name,
                Phone = originalUser.Phone,
                TaxId = originalUser.TaxId,
                CreatedAt = originalUser.CreatedAt,
                Addresses = originalUser.Addresses,
                Password = null // El original en el repo sigue teniendo su hash
            };
        }

        public User PostUser(User user)
        {
            Console.WriteLine($"\nPostUser...\nValidando TaxId: {user.TaxId}");
            // Validar tax_id único
            if (_repo.GetByTaxId(user.TaxId) is not null)
                throw new InvalidOperationException("tax_id debe ser único");
            Console.WriteLine($"Validando RFC Formato: {user.TaxId}");
            // Validar RFC (regex simplificada)
            if (!Regex.IsMatch(user.TaxId, @"^[A-Z&Ñ]{3,4}\d{6}[A-Z0-9]{3}$"))
                throw new ArgumentException("tax_id debe tener formato RFC");
            Console.WriteLine($"Validando Phone: {user.Phone}");
            if (!string.IsNullOrEmpty(user.Phone))
                user.Phone = Regex.Replace(user.Phone, @"[\s\-\(\)]", "");
            Console.WriteLine($"Validando Formato Phone: {user.Phone}");
            // Validar teléfono (10 dígitos, opcional código país; AndresFormat = regla que definas)
            if (!Regex.IsMatch(user.Phone, @"^\+?\d{10,15}$"))
                throw new ArgumentException("phone inválido");
            // Fecha actual Madagascar
            user.Id = Guid.NewGuid();
            Console.WriteLine($"Creando Usuario Id: {user.Id}");
            user.CreatedAt = InMemoryUserRepository.GetMadagascarNow(); // o método auxiliar
            Console.WriteLine($"PostUser\nAcceso antes: {user.Password}\n\tSemilla: {Constantes.Seguridad.Semilla}");
            if (!string.IsNullOrEmpty(user.Password))
                user.Password = AesEncrypt(user.Password); // AES256
            Console.WriteLine($"PostUser\nAcceso encriptado: {user.Password}");
            _repo.Add(user);

            return user;
        }

        public User PatchUser(Guid id, Dictionary<string, object> changes)
        {
            var user = _repo.GetById(id) ?? throw new KeyNotFoundException("Usuario no encontrado");
            foreach (var kv in changes)
            {
                var key = kv.Key.ToLower();
                var val = kv.Value?.ToString();

                if (val == null) continue;

                switch (key)
                {
                    case Constantes.Campos.Usuario.Email:
                        user.Email = val;
                        break;
                    case Constantes.Campos.Usuario.Nombre:
                        user.Name = val;
                        break;
                    case Constantes.Campos.Usuario.Telefono:
                        user.Phone = val;
                        //  TODO: revalidar de ser necesario
                        break;
                    case Constantes.Campos.Usuario.TaxId:
                        if (!user.TaxId.Equals(val, StringComparison.OrdinalIgnoreCase)
                            && _repo.GetByTaxId(val) is not null)
                            throw new InvalidOperationException("tax_id debe ser único");
                        user.TaxId = val;
                        break;
                    case Constantes.Campos.Usuario.Clave:
                        user.Password = AesEncrypt(val);
                        Console.WriteLine($"PatchUser:\n\tAcceso encriptado:{user.Password}");
                        break;
                        // TODO: addresses se podría mapear desde JSON a List<Address> en el controlador
                }
            }
            _repo.Update(user);
            return user;
        }

        public void DeleteUser(Guid id) => _repo.Delete(id);

        public bool Login(string taxId, string plainPassword)
        {
            Console.WriteLine($"LoginService:\n\tTax: {taxId},\n\tClave:{plainPassword}");
            var user = _repo.GetByTaxId(taxId);
            Console.WriteLine($"LoginService:\n\tuser: {user?.TaxId},\n\tClave:{plainPassword}");
            if (user == null || string.IsNullOrEmpty(user.Password)) return false;
            try
            {
                string decrypted = AesDecrypt(user.Password);
                Console.WriteLine($"LoginService:\n\tdesencritado:{decrypted}");
                return decrypted == plainPassword;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de descifrado: {ex.Message}");
                // Si Semilla cambió o el dato está corrupto, fallará aquí
                return false;
            }

        }

        private static byte[] Key(string password)
        {
            Console.WriteLine($"KeyService:\n\tClave: {password}");
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        }

        internal static string AesEncrypt(string plainText)
        {
            Console.WriteLine($"EncriptadoService:\n\tEntrada: {plainText}");
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key(Constantes.Seguridad.Semilla);
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            // guarda IV + cipher en base64
            var result = Convert.ToBase64String(aes.IV.Concat(cipherBytes).ToArray());
            Console.WriteLine($"EncriptadoService:\n\tSalida: {result}");
            return result;
        }

        internal static string AesDecrypt(string cipherTextWithIv)
        {
            Console.WriteLine($"DesencriptadoService:\n\tEntrada: {cipherTextWithIv}");
            var fullCipher = Convert.FromBase64String(cipherTextWithIv);
            using var aes = Aes.Create();
            // IMPORTANTE: Mantener la misma configuración que en Encrypt
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key(Constantes.Seguridad.Semilla);

            var iv = fullCipher.Take(16).ToArray();
            var cipherBytes = fullCipher.Skip(16).ToArray();

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            Console.WriteLine($"DesencriptadoService:\n\tSalida: {Encoding.UTF8.GetString(plainBytes)}");
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
