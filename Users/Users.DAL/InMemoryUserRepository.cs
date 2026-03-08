using System;
using System.Collections.Generic;
using System.Text;

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
            // user4, user5 similares...
        };
        }

        public static string GetMadagascarNow()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"); // Madagascar ~ UTC+3
            var madagascarNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            return madagascarNow.ToString("dd-MM-yyyy HH:mm");
        }

        // Implementaciones simples de GetAll, Add, Update, Delete...
        public void Add(User user)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public List<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public User? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public User? GetByTaxId(string taxId)
        {
            throw new NotImplementedException();
        }

        public void Update(User user)
        {
            throw new NotImplementedException();
        }
    }
}
