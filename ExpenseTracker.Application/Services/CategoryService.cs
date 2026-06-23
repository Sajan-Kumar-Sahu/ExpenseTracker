using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CategoryResponse>> CreateAsync(Guid userId, CreateCategoryRequest request)
        {
            var exists = await _categoryRepository.ExistsAsync(userId, request.Name, request.CategoryType);

            if (exists)
                return Result<CategoryResponse>.Failure("Category already exists.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                CategoryType = request.CategoryType,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponse>.Success(Map(category), "Category created successfully.");
        }

        public async Task<Result<CategoryResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result<CategoryResponse>.Failure("Category not found.");

            if (category.UserId != userId)
                return Result<CategoryResponse>.Failure("Access denied.");

            return Result<CategoryResponse>.Success(Map(category));
        }

        public async Task<Result<List<CategoryResponse>>> GetAllByUserAsync(Guid userId)
        {
            var categories = await _categoryRepository.GetByUserIdAsync(userId);

            return Result<List<CategoryResponse>>.Success(categories.Select(Map).ToList());
        }

        public async Task<Result<CategoryResponse>> UpdateAsync(Guid id, Guid userId, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result<CategoryResponse>.Failure("Category not found.");

            if (category.UserId != userId)
                return Result<CategoryResponse>.Failure("Access denied.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.CategoryType = request.CategoryType;
            category.IsActive = request.IsActive;

            await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponse>.Success(Map(category), "Category updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result.Failure("Category not found.");

            if (category.UserId != userId)
                return Result.Failure("Access denied.");

            await _categoryRepository.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Category deleted successfully.");
        }

        private static CategoryResponse Map(Category category) => new()
        {
            Id = category.Id,
            UserId = category.UserId,
            Name = category.Name,
            Description = category.Description,
            CategoryType = category.CategoryType,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }
}
