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

        public User PostUser(User newUser)
        {
            // Validar tax_id único
            if (_repo.GetByTaxId(newUser.TaxId) is not null)
                throw new InvalidOperationException("tax_id debe ser único");
            // Validar RFC (regex simplificada)
            if (!Regex.IsMatch(newUser.TaxId, @"^[A-Z&Ñ]{3,4}\d{6}[A-Z0-9]{3}$"))
                throw new ArgumentException("tax_id debe tener formato RFC");
            if (!string.IsNullOrEmpty(newUser.Phone))
                newUser.Phone = Regex.Replace(newUser.Phone, @"[\s\-\(\)]", "");
            // Validar teléfono (10 dígitos, opcional código país; AndresFormat = regla por definr)
            if (!Regex.IsMatch(newUser.Phone, @"^\+?\d{10,15}$"))
                throw new ArgumentException("phone inválido");
            // Fecha actual Madagascar
            newUser.Id = Guid.NewGuid();
            newUser.CreatedAt = InMemoryUserRepository.GetMadagascarNow(); // o método auxiliar
            if (!string.IsNullOrEmpty(newUser.Password))
                newUser.Password = AesEncrypt(newUser.Password); // AES256
            _repo.Add(newUser);
            Console.WriteLine($"PostUser");
            return new User
            {
                Id = newUser.Id,
                Email = newUser.Email,
                Name = newUser.Name,
                Phone = newUser.Phone,
                TaxId = newUser.TaxId,
                CreatedAt = newUser.CreatedAt,
                Addresses = newUser.Addresses,
                Password = null // El nuevo usuario en el repo sigue teniendo su hash
            };
        }

        public User PatchUser(Guid id, Dictionary<string, object> changes)
        {
            var patchUser = _repo.GetById(id) ?? throw new KeyNotFoundException("Usuario no encontrado");
            foreach (var kv in changes)
            {
                var key = kv.Key.ToLower();
                var val = kv.Value?.ToString();

                if (val == null) continue;

                switch (key)
                {
                    case Constantes.Campos.Usuario.Email:
                        patchUser.Email = val;
                        break;
                    case Constantes.Campos.Usuario.Nombre:
                        patchUser.Name = val;
                        break;
                    case Constantes.Campos.Usuario.Telefono:
                        patchUser.Phone = val;
                        //  TODO: revalidar de ser necesario
                        break;
                    case Constantes.Campos.Usuario.TaxId:
                        if (!patchUser.TaxId.Equals(val, StringComparison.OrdinalIgnoreCase)
                            && _repo.GetByTaxId(val) is not null)
                            throw new InvalidOperationException("tax_id debe ser único");
                        patchUser.TaxId = val;
                        break;
                    case Constantes.Campos.Usuario.Clave:
                        patchUser.Password = AesEncrypt(val);
                        break;
                }
            }
            _repo.Update(patchUser);
            Console.WriteLine($"PatchUser");
            return new User
            {
                Id = patchUser.Id,
                Email = patchUser.Email,
                Name = patchUser.Name,
                Phone = patchUser.Phone,
                TaxId = patchUser.TaxId,
                CreatedAt = patchUser.CreatedAt,
                Addresses = patchUser.Addresses,
                Password = null     // El usuario actualizado en el repo sigue teniendo su hash
            };
        }

        public void DeleteUser(Guid id) => _repo.Delete(id);

        public bool Login(string taxId, string plainPassword)
        {
            var user = _repo.GetByTaxId(taxId);
            if (user == null || string.IsNullOrEmpty(user.Password)) return false;
            try
            {
                string decrypted = AesDecrypt(user.Password);
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
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        }

        internal static string AesEncrypt(string plainText)
        {
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
            return result;
        }

        internal static string AesDecrypt(string cipherTextWithIv)
        {
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
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
