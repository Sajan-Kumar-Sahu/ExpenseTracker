using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CategoryResponse>> CreateAsync(
            CreateCategoryRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
                return Result<CategoryResponse>
                    .Failure("User not found.");

            var exists = await _categoryRepository.ExistsAsync(
                request.UserId,
                request.Name,
                request.CategoryType);

            if (exists)
                return Result<CategoryResponse>
                    .Failure("Category already exists.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = request.Name,
                Description = request.Description,
                CategoryType = request.CategoryType,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponse>.Success(
                Map(category),
                "Category created successfully.");
        }

        public async Task<Result<CategoryResponse>> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result<CategoryResponse>
                    .Failure("Category not found.");

            return Result<CategoryResponse>
                .Success(Map(category));
        }

        public async Task<Result<List<CategoryResponse>>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return Result<List<CategoryResponse>>
                .Success(categories.Select(Map).ToList());
        }

        public async Task<Result<List<CategoryResponse>>> GetByUserIdAsync(
            Guid userId)
        {
            var categories = await _categoryRepository.GetByUserIdAsync(userId);

            return Result<List<CategoryResponse>>
                .Success(categories.Select(Map).ToList());
        }

        public async Task<Result<CategoryResponse>> UpdateAsync(
            Guid id,
            UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result<CategoryResponse>
                    .Failure("Category not found.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.CategoryType = request.CategoryType;
            category.IsActive = request.IsActive;

            await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponse>.Success(
                Map(category),
                "Category updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                return Result.Failure("Category not found.");

            await _categoryRepository.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Category deleted successfully.");
        }

        private static CategoryResponse Map(Category category)
        {
            return new CategoryResponse
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
}
