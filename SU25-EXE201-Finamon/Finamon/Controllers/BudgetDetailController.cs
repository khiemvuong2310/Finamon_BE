using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetCategoryController : ControllerBase
    {
        private readonly IBudgetCategoryService _budgetCategoryService;

        public BudgetCategoryController(IBudgetCategoryService budgetCategoryService)
        {
            _budgetCategoryService = budgetCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<BudgetCategoryResponse>>> GetAllBudgetCategories([FromQuery] BudgetCategoryQueryRequest queryRequest)
        {
            var budgetCategories = await _budgetCategoryService.GetAllBudgetCategoriesAsync(queryRequest);
            return Ok(budgetCategories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetCategoryResponse>> GetBudgetCategoryById(int id)
        {
            try
            {
                var budgetCategory = await _budgetCategoryService.GetBudgetCategoryByIdAsync(id);
                return Ok(budgetCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<PaginatedResponse<BudgetCategoryResponse>>> GetBudgetCategoriesByCategoryId(int categoryId, [FromQuery] BudgetCategoryQueryRequest queryRequest)
        {
            var budgetCategories = await _budgetCategoryService.GetBudgetCategoriesByCategoryIdAsync(categoryId, queryRequest);
            return Ok(budgetCategories);
        }

        [HttpPost]
        public async Task<ActionResult<BudgetCategoryResponse>> CreateBudgetCategory(BudgetCategoryRequestModel request)
        {
            try
            {
                var budgetCategory = await _budgetCategoryService.CreateBudgetCategoryAsync(request);
                return CreatedAtAction(nameof(GetBudgetCategoryById), new { id = budgetCategory.Id }, budgetCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BudgetCategoryResponse>> UpdateBudgetCategory(int id, BudgetCategoryRequestModel request)
        {
            try
            {
                var budgetCategory = await _budgetCategoryService.UpdateBudgetCategoryAsync(id, request);
                return Ok(budgetCategory);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudgetCategory(int id)
        {
            try
            {
                await _budgetCategoryService.DeleteBudgetCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 