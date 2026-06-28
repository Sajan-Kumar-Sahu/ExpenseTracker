using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Project;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ProjectResponse>> CreateAsync(Guid userId, CreateProjectRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ProjectResponse>.Failure("Project name is required.");

            if (request.Name.Trim().Length > 200)
                return Result<ProjectResponse>.Failure("Project name cannot exceed 200 characters.");

            if (request.Description?.Trim().Length > 500)
                return Result<ProjectResponse>.Failure("Description cannot exceed 500 characters.");

            var exists = await _projectRepository.ExistsAsync(userId, request.Name.Trim());
            if (exists)
                return Result<ProjectResponse>.Failure("A project with this name already exists.");

            var project = new Project
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                IsActive = true
            };

            await _projectRepository.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return Result<ProjectResponse>.Success(Map(project), "Project created successfully.");
        }

        public async Task<Result<ProjectResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project is null)
                return Result<ProjectResponse>.Failure("Project not found.");

            if (project.UserId != userId)
                return Result<ProjectResponse>.Failure("Access denied.");

            return Result<ProjectResponse>.Success(Map(project));
        }

        public async Task<Result<List<ProjectListResponse>>> GetAllByUserAsync(Guid userId)
        {
            var projects = await _projectRepository.GetByUserIdAsync(userId);
            return Result<List<ProjectListResponse>>.Success(projects.Select(MapToList).ToList());
        }

        public async Task<Result<ProjectResponse>> UpdateAsync(Guid id, Guid userId, UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project is null)
                return Result<ProjectResponse>.Failure("Project not found.");

            if (project.UserId != userId)
                return Result<ProjectResponse>.Failure("Access denied.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<ProjectResponse>.Failure("Project name is required.");

            if (request.Name.Trim().Length > 200)
                return Result<ProjectResponse>.Failure("Project name cannot exceed 200 characters.");

            if (request.Description?.Trim().Length > 500)
                return Result<ProjectResponse>.Failure("Description cannot exceed 500 characters.");

            if (!request.Name.Trim().Equals(project.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _projectRepository.ExistsAsync(userId, request.Name.Trim());
                if (exists)
                    return Result<ProjectResponse>.Failure("A project with this name already exists.");
            }

            project.Name = request.Name.Trim();
            project.Description = request.Description?.Trim();
            project.IsActive = request.IsActive;

            await _projectRepository.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return Result<ProjectResponse>.Success(Map(project), "Project updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project is null)
                return Result.Failure("Project not found.");

            if (project.UserId != userId)
                return Result.Failure("Access denied.");

            await _projectRepository.DeleteAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Project deleted successfully.");
        }

        private static ProjectResponse Map(Project project) => new()
        {
            Id = project.Id,
            UserId = project.UserId,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };

        private static ProjectListResponse MapToList(Project project) => new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt
        };
    }
}
