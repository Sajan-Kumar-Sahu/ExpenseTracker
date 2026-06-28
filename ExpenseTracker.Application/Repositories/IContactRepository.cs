using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Repositories
{
    public interface IContactRepository
    {
        Task<Contact?> GetByIdAsync(Guid id);

        Task<List<Contact>> GetByUserIdAsync(Guid userId);

        Task AddAsync(Contact contact);

        Task UpdateAsync(Contact contact);

        Task DeleteAsync(Contact contact);
    }
}
