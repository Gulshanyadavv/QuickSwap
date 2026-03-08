using O_market.DTOs;
using O_market.Models;

namespace O_market.Repositories
{
    public interface ICategoryRepository
    {
        // Basic operations
        Task<Category?> GetByIdAsync(int id, bool includeChildren = false);
        Task<List<Category>> GetAllAsync(bool includeChildren = false);
        Task<List<Category>> GetSubCategoriesAsync(int parentId);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);

        // Parent validation
        Task<bool> IsValidParentAsync(int parentId, int? excludeId = null);

        // Dynamic field operations
        Task<List<DynamicField>> GetFieldsByCategoryAsync(int categoryId);
        Task<DynamicField> CreateDynamicFieldAsync(DynamicField field);
        Task<DynamicField> UpdateDynamicFieldAsync(DynamicField field);
        Task<bool> DeleteDynamicFieldAsync(int fieldId);
        Task<DynamicField?> GetDynamicFieldByIdAsync(int fieldId);
    }
}