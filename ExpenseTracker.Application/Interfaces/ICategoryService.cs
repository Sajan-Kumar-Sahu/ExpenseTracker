using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request);

        Task<Result<CategoryResponse>> GetByIdAsync(Guid id);

        Task<Result<List<CategoryResponse>>> GetAllAsync();

        Task<Result<List<CategoryResponse>>> GetByUserIdAsync(Guid userId);

        Task<Result<CategoryResponse>> UpdateAsync(
            Guid id,
            UpdateCategoryRequest request);

        Task<Result> DeleteAsync(Guid id);
    }
}
