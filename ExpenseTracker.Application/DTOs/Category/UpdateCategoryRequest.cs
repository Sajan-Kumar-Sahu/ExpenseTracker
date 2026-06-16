using ExpenseTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.DTOs.Category
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public CategoryType CategoryType { get; set; }

        public bool IsActive { get; set; }
    }
}
