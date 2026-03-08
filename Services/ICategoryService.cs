using O_market.DTO;
using O_market.DTOs;
using O_market.Models;

namespace O_market.Services
{
    public interface ICategoryService
    {
        // Basic category operations
        Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto?> GetCategoryByIdAsync(int id);
        Task<List<CategoryResponseDto>> GetSubCategoriesAsync(int parentId);
        Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto dto, string userRole);
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto, string userRole);
        Task<bool> DeleteCategoryAsync(int id, string userRole);

        // Dynamic field operations
        Task<List<DynamicFieldDto>> GetFieldsByCategoryIdAsync(int categoryId);

        // Enhanced category operations
        Task<List<CategoryWithFieldsDto>> GetAllCategoriesWithFieldsAsync();
        Task<CategoryWithFieldsDto?> GetCategoryWithFieldsByIdAsync(int id);
        Task<CategoryBreadcrumbDto> GetCategoryBreadcrumbsAsync(int categoryId);
        Task<CategoryFilterSidebarDto> GetCategorySidebarFiltersAsync(int categoryId);

        // Dynamic field management (Admin)
        Task<DynamicFieldDto> CreateDynamicFieldAsync(DynamicFieldCreateDto dto, string userRole);
        Task<DynamicFieldDto> UpdateDynamicFieldAsync(int fieldId, DynamicFieldUpdateDto dto, string userRole);
        Task<bool> DeleteDynamicFieldAsync(int fieldId, string userRole);
    }
}