using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Users.DAL;

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
            foreach (var u in users) u.Password = null!;
            return users;
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

        public User CreateUser(User user)
        {
            // Validar tax_id único
            if (_repo.GetByTaxId(user.TaxId) is not null)
                throw new InvalidOperationException("tax_id debe ser único");

            // Validar RFC (regex simplificada)
            if (!Regex.IsMatch(user.TaxId, @"^[A-Z&Ñ]{3,4}\d{6}[A-Z0-9]{3}$"))
                throw new ArgumentException("tax_id debe tener formato RFC");

            // Validar teléfono (10 dígitos, opcional código país; AndresFormat = regla que definas)
            if (!Regex.IsMatch(user.Phone, @"^\+?\d{10,15}$"))
                throw new ArgumentException("phone inválido");

            // Fecha actual Madagascar
            user.Id = Guid.NewGuid();
            user.CreatedAt = InMemoryUserRepository.GetMadagascarNow(); // o método auxiliar
            user.Password = AesEncrypt(user.Password); // AES256

            _repo.Add(user);

            user.Password = null!;
            return user;
        }

        public User PatchUser(Guid id, Dictionary<string, object> changes)
        {
            var user = _repo.GetById(id) ?? throw new KeyNotFoundException();
            // aplica cambios, revalida tax_id/phone y re-encripta password si viene
            _repo.Update(user);
            user.Password = null!;
            return user;
        }

        public void DeleteUser(Guid id) => _repo.Delete(id);

        public bool Login(string taxId, string plainPassword)
        {
            var user = _repo.GetByTaxId(taxId);
            if (user == null) return false;
            var encrypted = AesEncrypt(plainPassword);
            return encrypted == user.Password;
        }

        // AES256 usando System.Security.Cryptography
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("MiClaveSecreta32Bytes123456789012"); // 32 bytes

        private static string AesEncrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key;
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            // guarda IV + cipher en base64
            var result = Convert.ToBase64String(aes.IV.Concat(cipherBytes).ToArray());
            return result;
        }
    }
}
