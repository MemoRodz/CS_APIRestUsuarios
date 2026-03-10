using Users.DAL.Models;
using System.Globalization;

namespace Users.DAL
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users;
        public InMemoryUserRepository()
        {
            var nowMadagascar = GetMadagascarNow();

            _users = new List<User> {
            new User {
                Id = Guid.NewGuid(),
                Email = "user1@mail.com",
                Name = "user1",
                Phone = "+1 55 555 555 55",
                Password = "7c4a8d09ca3762af61e59520943dc26494f8941b", // luego lo AES
                TaxId = "AARR990101XXX",
                CreatedAt = nowMadagascar,
                Addresses = new List<Address> {
                    new Address { Id = 1, Name = "workaddress", Street = "street No. 1", CountryCode = "UK" },
                    new Address { Id = 2, Name = "homeaddress", Street = "street No. 2", CountryCode = "AU" }
                }
            },
                        new User {
                Id = Guid.NewGuid(),
                Email = "carlo@mail.org",
                Name = "magnocarlo",
                Phone = "+52 555 123 4567",
                Password = "hash_password_2", // luego lo AES
                TaxId = "BBBS990202YYY",
                CreatedAt = nowMadagascar,
                Addresses = new List<Address> {
                    new Address { Id = 1, Name = "workaddress", Street = "street No. 1", CountryCode = "UK" },
                    new Address { Id = 2, Name = "homeaddress", Street = "street No. 2", CountryCode = "AU" }
                }
            },
                        new User {
                Id = Guid.NewGuid(),
                Email = "magno@mail.net",
                Name = "carlomagno",
                Phone = "+1 555 987 6543",
                Password = "mipassword123", // luego lo AES
                TaxId = "CCCT990303ZZZ",
                CreatedAt = nowMadagascar,
                Addresses = new List<Address> {
                    new Address { Id = 1, Name = "workaddress", Street = "street No. 1", CountryCode = "UK" },
                    new Address { Id = 2, Name = "homeaddress", Street = "street No. 2", CountryCode = "AU" }
                }
            },
            // user4, user5 similares... Puesto que es en memoria el almacenamiento.
        };
        }
        public static string GetMadagascarNow()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"); // Madagascar ~ UTC+3
            var madagascarNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            return madagascarNow.ToString("dd-MM-yyyy HH:mm");
        }
        public List<User> GetAll() => _users.ToList();
        public User? GetById(Guid id)
        {
            Console.WriteLine($"Id:{id}");
            User userIn = _users.FirstOrDefault(u => u.Id == id);
            Console.WriteLine($"Usuario obtenido: {userIn?.Id}");
            return userIn;
        }
        public User? GetByTaxId(string taxId)
        {
            Console.WriteLine($"TaxId:{taxId}");
            User userIn = _users.FirstOrDefault(u => u.TaxId.Equals(taxId, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"Usuario obtenido: {userIn?.Id}");
            if (userIn == null) return null;
            return userIn;
        }
        public void Add(User user) => _users.Add(user);
        public void Update(User user)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) _users[index] = user;
        }
        public void Delete(Guid id)
        {
            _users.RemoveAll(u => u.Id == id);
        }
    }
}
