using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Persistence.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public ContactRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Contact?> GetByIdAsync(Guid id)
        {
            return await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Contact>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Contacts
                .Where(c => c.UserId == userId)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Contact contact)
        {
            await _context.Contacts.AddAsync(contact);
        }

        public Task UpdateAsync(Contact contact)
        {
            _context.Contacts.Update(contact);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Contact contact)
        {
            _context.Contacts.Remove(contact);
            return Task.CompletedTask;
        }
    }
}
