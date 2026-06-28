using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Contact;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ContactService(IContactRepository contactRepository, IUnitOfWork unitOfWork)
        {
            _contactRepository = contactRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ContactResponse>> CreateAsync(Guid userId, CreateContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ContactResponse>.Failure("Contact name is required.");

            if (request.Name.Trim().Length > 100)
                return Result<ContactResponse>.Failure("Contact name cannot exceed 100 characters.");

            if (!Enum.IsDefined(typeof(ContactType), request.ContactType))
                return Result<ContactResponse>.Failure("Invalid contact type.");

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name.Trim(),
                MobileNumber = request.MobileNumber?.Trim(),
                Email = request.Email?.Trim(),
                ContactType = request.ContactType,
                IsActive = true,
                Notes = request.Notes?.Trim()
            };

            await _contactRepository.AddAsync(contact);
            await _unitOfWork.SaveChangesAsync();

            return Result<ContactResponse>.Success(Map(contact), "Contact created successfully.");
        }

        public async Task<Result<ContactResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
                return Result<ContactResponse>.Failure("Contact not found.");

            if (contact.UserId != userId)
                return Result<ContactResponse>.Failure("Access denied.");

            return Result<ContactResponse>.Success(Map(contact));
        }

        public async Task<Result<List<ContactResponse>>> GetAllByUserAsync(Guid userId)
        {
            var contacts = await _contactRepository.GetByUserIdAsync(userId);
            return Result<List<ContactResponse>>.Success(contacts.Select(Map).ToList());
        }

        public async Task<Result<ContactResponse>> UpdateAsync(Guid id, Guid userId, UpdateContactRequest request)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
                return Result<ContactResponse>.Failure("Contact not found.");

            if (contact.UserId != userId)
                return Result<ContactResponse>.Failure("Access denied.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ContactResponse>.Failure("Contact name is required.");

            if (request.Name.Trim().Length > 100)
                return Result<ContactResponse>.Failure("Contact name cannot exceed 100 characters.");

            if (!Enum.IsDefined(typeof(ContactType), request.ContactType))
                return Result<ContactResponse>.Failure("Invalid contact type.");

            contact.Name = request.Name.Trim();
            contact.MobileNumber = request.MobileNumber?.Trim();
            contact.Email = request.Email?.Trim();
            contact.ContactType = request.ContactType;
            contact.IsActive = request.IsActive;
            contact.Notes = request.Notes?.Trim();

            await _contactRepository.UpdateAsync(contact);
            await _unitOfWork.SaveChangesAsync();

            return Result<ContactResponse>.Success(Map(contact), "Contact updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var contact = await _contactRepository.GetByIdAsync(id);

            if (contact is null)
                return Result.Failure("Contact not found.");

            if (contact.UserId != userId)
                return Result.Failure("Access denied.");

            await _contactRepository.DeleteAsync(contact);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Contact deleted successfully.");
        }

        private static ContactResponse Map(Contact contact) => new()
        {
            Id = contact.Id,
            UserId = contact.UserId,
            Name = contact.Name,
            MobileNumber = contact.MobileNumber,
            Email = contact.Email,
            ContactType = contact.ContactType,
            IsActive = contact.IsActive,
            Notes = contact.Notes,
            CreatedAt = contact.CreatedAt
        };
    }
}
