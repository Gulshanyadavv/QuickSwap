using Microsoft.EntityFrameworkCore;
using O_market.Models;
using O_market.Repositories;

namespace O_market.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly OlxdbContext _context;

        public CategoryRepository(OlxdbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id, bool includeChildren = false)
        {
            var query = _context.Categories.AsQueryable();

            if (includeChildren)
                query = query.Include(c => c.InverseParent);

            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Category>> GetAllAsync(bool includeChildren = false)
        {
            var query = _context.Categories.AsQueryable();

            if (includeChildren)
                query = query.Include(c => c.InverseParent);

            return await query
                .OrderBy(c => c.ParentId)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category == null)
                return false;

            // Check if category has ads
            var hasAds = await _context.Ads.AnyAsync(a => a.CategoryId == id);
            if (hasAds)
                return false;

            // Check if category has subcategories
            var hasSubCategories = await _context.Categories.AnyAsync(c => c.ParentId == id);
            if (hasSubCategories)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsValidParentAsync(int parentId, int? excludeId = null)
        {
            if (parentId <= 0)
                return false;

            var query = _context.Categories.Where(c => c.Id == parentId);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<DynamicField>> GetFieldsByCategoryAsync(int categoryId)
        {
            return await _context.DynamicFields
                .Where(f => f.CategoryId == categoryId)
                .OrderBy(f => f.Id)
                .ToListAsync();
        }

        public async Task<DynamicField> CreateDynamicFieldAsync(DynamicField field)
        {
            _context.DynamicFields.Add(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task<DynamicField> UpdateDynamicFieldAsync(DynamicField field)
        {
            _context.DynamicFields.Update(field);
            await _context.SaveChangesAsync();
            return field;
        }

        public async Task<bool> DeleteDynamicFieldAsync(int fieldId)
        {
            var field = await _context.DynamicFields.FindAsync(fieldId);
            if (field == null)
                return false;

            // Check if field is used
            var isUsed = await _context.AdDynamicValues.AnyAsync(dv => dv.FieldId == fieldId);
            if (isUsed)
                return false;

            _context.DynamicFields.Remove(field);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DynamicField?> GetDynamicFieldByIdAsync(int fieldId)
        {
            return await _context.DynamicFields.FindAsync(fieldId);
        }
    }
}