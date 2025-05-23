using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Finamon.Service.RequestModel.QueryRequest;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<BudgetResponse>>> GetAllBudgets([FromQuery] BudgetQueryRequest queryRequest)
        {
            var budgets = await _budgetService.GetAllBudgetsAsync(queryRequest);
            return Ok(budgets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetResponse>> GetBudgetById(int id)
        {
            try
            {
                var budget = await _budgetService.GetBudgetByIdAsync(id);
                return Ok(budget);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<BudgetResponse>> CreateBudget(BudgetRequestModel request)
        {
            var budget = await _budgetService.CreateBudgetAsync(request);
            return CreatedAtAction(nameof(GetBudgetById), new { id = budget.Id }, budget);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BudgetResponse>> UpdateBudget(int id, BudgetRequestModel request)
        {
            try
            {
                var budget = await _budgetService.UpdateBudgetAsync(id, request);
                return Ok(budget);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            try
            {
                await _budgetService.DeleteBudgetAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
