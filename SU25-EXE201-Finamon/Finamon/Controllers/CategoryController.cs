using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllCategories([FromQuery] CategoryQueryRequest query)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(query);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> CreateCategory(CategoryRequestModel request)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryResponse>> UpdateCategory(int id, CategoryRequestModel request)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, request);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("many")]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> CreateManyCategories([FromBody] CreateManyCategoriesRequest request)
        {
            try
            {
                var categories = await _categoryService.CreateManyCategoriesAsync(request);
                return Ok(categories);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PaginatedResponse<CategoryResponse>>> GetCategoryByUserId(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var categories = await _categoryService.GetCategoryByUserIdAsync(userId, pageNumber, pageSize);
                return Ok(categories);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 