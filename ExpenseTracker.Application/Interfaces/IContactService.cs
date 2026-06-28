using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Contact;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IContactService
    {
        Task<Result<ContactResponse>> CreateAsync(Guid userId, CreateContactRequest request);

        Task<Result<ContactResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<ContactResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<ContactResponse>> UpdateAsync(Guid id, Guid userId, UpdateContactRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);
    }
}
