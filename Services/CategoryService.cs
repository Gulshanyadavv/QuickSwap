using AutoMapper;
using Microsoft.EntityFrameworkCore;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Repositories;
using O_market.Services;
using System.Text.Json;

namespace O_market.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;
        private readonly OlxdbContext _context;

        public CategoryService(ICategoryRepository repo, IMapper mapper, OlxdbContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _repo.GetAllAsync(includeChildren: true);
            var dtos = _mapper.Map<List<CategoryResponseDto>>(categories);

            foreach (var dto in dtos)
            {
                dto.SubCategories = dtos.Where(c => c.ParentId == dto.Id).ToList();
            }

            return dtos.Where(c => c.ParentId == null)
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id, includeChildren: true);
            if (category == null) return null;

            var dto = _mapper.Map<CategoryResponseDto>(category);
            dto.SubCategories = _mapper.Map<List<CategoryResponseDto>>(category.InverseParent);
            return dto;
        }

        public async Task<List<CategoryResponseDto>> GetSubCategoriesAsync(int parentId)
        {
            var categories = await _repo.GetSubCategoriesAsync(parentId);
            return _mapper.Map<List<CategoryResponseDto>>(categories);
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto dto, string userRole)
        {
            if (userRole != "Admin" && userRole != "Seller")
                throw new UnauthorizedAccessException("Admin/Seller required.");

            if (dto.ParentId.HasValue && !await _repo.IsValidParentAsync(dto.ParentId.Value))
                throw new ArgumentException("Invalid parent category.");

            var category = _mapper.Map<Category>(dto);
            var created = await _repo.CreateAsync(category);
            return _mapper.Map<CategoryResponseDto>(created);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto, string userRole)
        {
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("Admin required.");

            var category = await _repo.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException("Category not found.");

            if (dto.ParentId.HasValue && !await _repo.IsValidParentAsync(dto.ParentId.Value, id))
                throw new ArgumentException("Invalid parent.");

            _mapper.Map(dto, category);
            var updated = await _repo.UpdateAsync(category);
            return await GetCategoryByIdAsync(id);
        }

        public async Task<bool> DeleteCategoryAsync(int id, string userRole)
        {
            if (userRole != "Admin" && userRole != "Seller")
                throw new UnauthorizedAccessException("Admin required.");

            return await _repo.DeleteAsync(id);
        }

        public async Task<List<DynamicFieldDto>> GetFieldsByCategoryIdAsync(int categoryId)
        {
            var fields = await _repo.GetFieldsByCategoryAsync(categoryId);
            var dtos = _mapper.Map<List<DynamicFieldDto>>(fields);

            // Parse JSON options if needed
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrEmpty(dto.Options) &&
                    (dto.FieldType == "dropdown" || dto.FieldType == "checkbox" || dto.FieldType == "radio"))
                {
                    try
                    {
                        var parsedOptions = JsonSerializer.Deserialize<List<string>>(dto.Options);
                        if (parsedOptions != null)
                        {
                            dto.Options = JsonSerializer.Serialize(parsedOptions);
                        }
                    }
                    catch
                    {
                        // Keep original if parsing fails
                    }
                }
            }

            return dtos;
        }

        // NEW: Get all categories with dynamic fields
        public async Task<List<CategoryWithFieldsDto>> GetAllCategoriesWithFieldsAsync()
        {
            var categories = await _repo.GetAllAsync(includeChildren: true);
            var dtos = _mapper.Map<List<CategoryWithFieldsDto>>(categories);

            // Get dynamic fields for all categories
            foreach (var dto in dtos)
            {
                dto.DynamicFields = await GetFieldsByCategoryIdAsync(dto.Id);
                dto.SubCategories = dtos.Where(c => c.ParentId == dto.Id).ToList();

                foreach (var sub in dto.SubCategories)
                {
                    sub.DynamicFields = await GetFieldsByCategoryIdAsync(sub.Id);
                }
            }

            return dtos.Where(c => c.ParentId == null).OrderBy(c => c.Name).ToList();
        }

        // NEW: Get category with dynamic fields by ID
        public async Task<CategoryWithFieldsDto?> GetCategoryWithFieldsByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id, includeChildren: true);
            if (category == null) return null;

            var dto = _mapper.Map<CategoryWithFieldsDto>(category);
            dto.DynamicFields = await GetFieldsByCategoryIdAsync(id);
            dto.SubCategories = _mapper.Map<List<CategoryWithFieldsDto>>(category.InverseParent);

            foreach (var sub in dto.SubCategories)
            {
                sub.DynamicFields = await GetFieldsByCategoryIdAsync(sub.Id);
            }

            return dto;
        }

        // NEW: Get category breadcrumbs
        public async Task<CategoryBreadcrumbDto> GetCategoryBreadcrumbsAsync(int categoryId)
        {
            var category = await _repo.GetByIdAsync(categoryId);
            if (category == null) return null;

            var breadcrumbs = new List<BreadcrumbDto>();

            breadcrumbs.Add(new BreadcrumbDto
            {
                Text = "Home",
                Url = "/",
                IsActive = false
            });

            var current = category;
            var parentStack = new Stack<Category>();

            while (current != null)
            {
                parentStack.Push(current);
                if (current.ParentId.HasValue)
                {
                    current = await _repo.GetByIdAsync(current.ParentId.Value);
                }
                else
                {
                    current = null;
                }
            }

            while (parentStack.Count > 0)
            {
                var cat = parentStack.Pop();
                breadcrumbs.Add(new BreadcrumbDto
                {
                    Text = cat.Name,
                    Url = $"/ads/category/{cat.Id}",
                    IsActive = parentStack.Count == 0
                });
            }

            return new CategoryBreadcrumbDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                ParentCategoryId = category.ParentId,
                ParentCategoryName = category.Parent?.Name,
                Breadcrumbs = breadcrumbs
            };
        }

        // NEW: Get sidebar filters for category
        public async Task<CategoryFilterSidebarDto> GetCategorySidebarFiltersAsync(int categoryId)
        {
            var category = await _repo.GetByIdAsync(categoryId);
            if (category == null) return null;

            var filters = new CategoryFilterSidebarDto
            {
                CategoryId = categoryId,
                CategoryName = category.Name
            };

            // Price filter
            var priceStats = await GetPriceStatsAsync(categoryId);
            filters.PriceFilter = new PriceFilterDto
            {
                Min = priceStats.MinPrice,
                Max = priceStats.MaxPrice,
                QuickRanges = GetPriceRanges(priceStats.MinPrice, priceStats.MaxPrice)
            };

            // Location filter
            filters.LocationFilter = await GetLocationFilterAsync(categoryId);

            // Dynamic field filters
            filters.DynamicFilters = await GetDynamicFieldFiltersAsync(categoryId);

            // Other filters
            filters.ConditionFilters = await GetConditionFiltersAsync(categoryId);
            filters.SellerTypeFilters = GetSellerTypeFilters();
            filters.PostedDateFilters = GetPostedDateFilters();

            return filters;
        }

        // NEW: Create dynamic field
        public async Task<DynamicFieldDto> CreateDynamicFieldAsync(DynamicFieldCreateDto dto, string userRole)
        {
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("Admin required.");

            // Validate field type
            var validFieldTypes = new[] { "text", "number", "dropdown", "checkbox", "radio", "textarea" };
            if (!validFieldTypes.Contains(dto.FieldType.ToLower()))
            {
                throw new ArgumentException($"Invalid field type. Must be one of: {string.Join(", ", validFieldTypes)}");
            }

            var field = _mapper.Map<DynamicField>(dto);
            var created = await _repo.CreateDynamicFieldAsync(field);
            return _mapper.Map<DynamicFieldDto>(created);
        }

        // NEW: Update dynamic field
        public async Task<DynamicFieldDto> UpdateDynamicFieldAsync(int fieldId, DynamicFieldUpdateDto dto, string userRole)
        {
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("Admin required.");

            var field = await _repo.GetDynamicFieldByIdAsync(fieldId);
            if (field == null)
                throw new KeyNotFoundException("Dynamic field not found.");

            _mapper.Map(dto, field);
            var updated = await _repo.UpdateDynamicFieldAsync(field);
            return _mapper.Map<DynamicFieldDto>(updated);
        }

        // NEW: Delete dynamic field
        public async Task<bool> DeleteDynamicFieldAsync(int fieldId, string userRole)
        {
            if (userRole != "Admin")
                throw new UnauthorizedAccessException("Admin required.");

            return await _repo.DeleteDynamicFieldAsync(fieldId);
        }

        // Helper methods for sidebar filters
        private async Task<PriceStats> GetPriceStatsAsync(int categoryId)
        {
            var prices = await _context.Ads
                .Where(a => a.CategoryId == categoryId && a.Status == "Active")
                .Select(a => a.Price)
                .ToListAsync();

            return new PriceStats
            {
                MinPrice = prices.Any() ? prices.Min() : 0,
                MaxPrice = prices.Any() ? prices.Max() : 1000000,
                AvgPrice = prices.Any() ? prices.Average() : 500000
            };
        }

        private List<PriceRangeOption> GetPriceRanges(decimal min, decimal max)
        {
            return new List<PriceRangeOption>
            {
                new() { Label = "Under ₹50,000", Max = 50000 },
                new() { Label = "₹50,000 - ₹1 Lakh", Min = 50000, Max = 100000 },
                new() { Label = "₹1 Lakh - ₹2 Lakh", Min = 100000, Max = 200000 },
                new() { Label = "₹2 Lakh - ₹5 Lakh", Min = 200000, Max = 500000 },
                new() { Label = "₹5 Lakh+", Min = 500000 }
            };
        }

        private async Task<LocationFilterDto> GetLocationFilterAsync(int categoryId)
        {
            var locations = await _context.Ads
                .Where(a => a.CategoryId == categoryId && a.Status == "Active")
                .Select(a => a.Location)
                .Distinct()
                .OrderBy(l => l)
                .Take(20)
                .ToListAsync();

            var popularCities = new List<string>
            {
                "Mumbai", "Delhi", "Bangalore", "Chennai",
                "Hyderabad", "Pune", "Kolkata", "Ahmedabad"
            };

            return new LocationFilterDto
            {
                PopularCities = popularCities,
                AllLocations = locations
            };
        }

        private async Task<List<DynamicFieldFilterDto>> GetDynamicFieldFiltersAsync(int categoryId)
        {
            var dynamicFields = await _repo.GetFieldsByCategoryAsync(categoryId);
            var filters = new List<DynamicFieldFilterDto>();

            foreach (var field in dynamicFields.Take(5))
            {
                var filter = new DynamicFieldFilterDto
                {
                    FieldId = field.Id,
                    Label = field.Label,
                    FieldType = field.FieldType,
                    IsMultiSelect = field.FieldType == "checkbox"
                };

                if (!string.IsNullOrEmpty(field.Options))
                {
                    try
                    {
                        var options = JsonSerializer.Deserialize<List<string>>(field.Options);
                        if (options != null)
                        {
                            foreach (var option in options.Take(10))
                            {
                                var count = await _context.AdDynamicValues
                                    .Include(dv => dv.Ad)
                                    .CountAsync(dv => dv.FieldId == field.Id &&
                                                    dv.FieldValue == option &&
                                                    dv.Ad.CategoryId == categoryId &&
                                                    dv.Ad.Status == "Active");

                                filter.Options.Add(new FilterOptionDto
                                {
                                    Value = option,
                                    Label = option,
                                    Count = count
                                });
                            }
                        }
                    }
                    catch { }
                }

                filters.Add(filter);
            }

            return filters;
        }

        private async Task<List<FilterOptionDto>> GetConditionFiltersAsync(int categoryId)
        {
            var conditions = new[] { "New", "Used", "Refurbished" };
            var filters = new List<FilterOptionDto>();

            foreach (var condition in conditions)
            {
                var count = await _context.AdDynamicValues
                    .Include(dv => dv.Ad)
                    .CountAsync(dv => dv.FieldId == 1 && // Assuming condition field has ID 1
                                    dv.FieldValue == condition &&
                                    dv.Ad.CategoryId == categoryId &&
                                    dv.Ad.Status == "Active");

                filters.Add(new FilterOptionDto
                {
                    Value = condition.ToLower(),
                    Label = condition,
                    Count = count
                });
            }

            return filters;
        }

        private List<FilterOptionDto> GetSellerTypeFilters()
        {
            return new List<FilterOptionDto>
            {
                new() { Value = "individual", Label = "Individual", Count = 0 },
                new() { Value = "dealer", Label = "Dealer", Count = 0 },
                new() { Value = "verified", Label = "Verified Sellers", Count = 0 }
            };
        }

        private List<FilterOptionDto> GetPostedDateFilters()
        {
            return new List<FilterOptionDto>
            {
                new() { Value = "today", Label = "Today", Count = 0 },
                new() { Value = "yesterday", Label = "Yesterday", Count = 0 },
                new() { Value = "last_week", Label = "Last 7 days", Count = 0 },
                new() { Value = "last_month", Label = "Last 30 days", Count = 0 }
            };
        }

        private class PriceStats
        {
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public decimal AvgPrice { get; set; }
        }
    }
}