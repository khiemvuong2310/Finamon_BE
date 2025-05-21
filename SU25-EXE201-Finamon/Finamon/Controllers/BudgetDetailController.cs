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
    public class BudgetDetailController : ControllerBase
    {
        private readonly IBudgetDetailService _budgetDetailService;

        public BudgetDetailController(IBudgetDetailService budgetDetailService)
        {
            _budgetDetailService = budgetDetailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDetailResponse>>> GetAllBudgetDetails([FromQuery] BudgetDetailQueryRequest queryRequest)
        {
            var budgetDetails = await _budgetDetailService.GetAllBudgetDetailsAsync(queryRequest);
            return Ok(budgetDetails);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDetailResponse>> GetBudgetDetailById(int id)
        {
            try
            {
                var budgetDetail = await _budgetDetailService.GetBudgetDetailByIdAsync(id);
                return Ok(budgetDetail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("budget/{budgetId}")]
        public async Task<ActionResult<IEnumerable<BudgetDetailResponse>>> GetBudgetDetailsByBudgetId(int budgetId)
        {
            var budgetDetails = await _budgetDetailService.GetBudgetDetailsByBudgetIdAsync(budgetId);
            return Ok(budgetDetails);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BudgetDetailResponse>>> GetBudgetDetailsByCategoryId(int categoryId)
        {
            var budgetDetails = await _budgetDetailService.GetBudgetDetailsByCategoryIdAsync(categoryId);
            return Ok(budgetDetails);
        }

        [HttpPost]
        public async Task<ActionResult<BudgetDetailResponse>> CreateBudgetDetail(BudgetDetailRequestModel request)
        {
            try
            {
                var budgetDetail = await _budgetDetailService.CreateBudgetDetailAsync(request);
                return CreatedAtAction(nameof(GetBudgetDetailById), new { id = budgetDetail.Id }, budgetDetail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BudgetDetailResponse>> UpdateBudgetDetail(int id, BudgetDetailRequestModel request)
        {
            try
            {
                var budgetDetail = await _budgetDetailService.UpdateBudgetDetailAsync(id, request);
                return Ok(budgetDetail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudgetDetail(int id)
        {
            try
            {
                await _budgetDetailService.DeleteBudgetDetailAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 