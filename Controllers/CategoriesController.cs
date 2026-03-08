using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O_market.DTO;
using O_market.DTOs;
using O_market.Services;

namespace O_market.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // =========================================
        // 1. GET ALL CATEGORIES (HOME PAGE)
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetAll()
        {
            return Ok(await _categoryService.GetAllCategoriesAsync());
        }

        // =========================================
        // 2. GET CATEGORY DETAILS
        // =========================================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryResponseDto>> Get(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        // =========================================
        // 3. GET SUBCATEGORIES
        // =========================================
        [HttpGet("{id}/subcategories")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CategoryResponseDto>>> GetSubCategories(int id)
        {
            return Ok(await _categoryService.GetSubCategoriesAsync(id));
        }

        // =========================================
        // 4. GET DYNAMIC FIELDS (POST AD FLOW)
        // =========================================
        [HttpGet("{categoryId}/fields")]
        [AllowAnonymous]
        public async Task<ActionResult<List<DynamicFieldDto>>> GetFields(int categoryId)
        {
            return Ok(await _categoryService.GetFieldsByCategoryIdAsync(categoryId));
        }

        // =========================================
        // 5. BREADCRUMBS (CATEGORY NAVIGATION)
        // =========================================
        [HttpGet("{categoryId}/breadcrumbs")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryBreadcrumbDto>> GetBreadcrumbs(int categoryId)
        {
            var breadcrumbs = await _categoryService.GetCategoryBreadcrumbsAsync(categoryId);
            return breadcrumbs == null ? NotFound() : Ok(breadcrumbs);
        }

        // =========================================
        // ADMIN OPERATIONS
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryResponseDto>> Create(CategoryCreateDto dto)
        {
            var category = await _categoryService.CreateCategoryAsync(dto, "Admin");
            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryResponseDto>> Update(int id, CategoryUpdateDto dto)
        {
            return Ok(await _categoryService.UpdateCategoryAsync(id, dto, "Admin"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteCategoryAsync(id, "Admin");
            return NoContent();
        }
    }
}
