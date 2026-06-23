using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Category;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<CategoryResponse>> CreateAsync(Guid userId, CreateCategoryRequest request);

        Task<Result<CategoryResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<CategoryResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<CategoryResponse>> UpdateAsync(Guid id, Guid userId, UpdateCategoryRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);
    }
}
