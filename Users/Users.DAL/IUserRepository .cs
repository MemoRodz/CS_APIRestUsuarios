using System;
using System.Collections.Generic;
using System.Text;

namespace Users.DAL
{
    public interface IUserRepository
    {
        List<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(Guid id);
        User? GetById(Guid id);
        User? GetByTaxId(string taxId);
    }
}
