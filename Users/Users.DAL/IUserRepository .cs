using Users.DAL.Models;

namespace Users.DAL
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetById(Guid id);
        User? GetByTaxId(string taxId);
        void Add(User user);
        void Update(User user);
        void Delete(Guid id);
    }
}
