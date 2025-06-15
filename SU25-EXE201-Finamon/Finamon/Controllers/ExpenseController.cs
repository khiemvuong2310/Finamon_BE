using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Finamon.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseResponse>> CreateExpense([FromBody] ExpenseRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var expense = await _expenseService.CreateExpenseAsync(request, userId);
            return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, expense);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ExpenseResponse>> UpdateExpense(int id, [FromBody] ExpenseUpdateRequest request)
        {
            try
            {
                var expense = await _expenseService.UpdateExpenseAsync(id, request);
                return Ok(expense);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteExpense(int id)
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseResponse>> GetExpenseById(int id)
        {
            try
            {
                var expense = await _expenseService.GetExpenseByIdAsync(id);
                return Ok(expense);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ExpenseDetailResponse>> GetExpenseDetailById(int id)
        {
            try
            {
                var expense = await _expenseService.GetExpenseDetailByIdAsync(id);
                return Ok(expense);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[HttpGet]
        //public async Task<ActionResult<List<ExpenseResponse>>> GetAllExpenses()
        //{
        //    var expenses = await _expenseService.GetAllExpensesAsync();
        //    return Ok(expenses);
        //}

        [HttpGet("user")]
        public async Task<ActionResult<List<ExpenseResponse>>> GetExpensesByUserId()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var expenses = await _expenseService.GetExpensesByUserIdAsync(userId);
            return Ok(expenses);
        }

        [HttpGet]
        public async Task<ActionResult<(List<ExpenseResponse> Expenses, int TotalCount)>> GetExpensesByFilter([FromQuery] ExpenseQueryRequest query)
        {
            var result = await _expenseService.GetExpensesByFilterAsync(query);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PaginatedResponse<ExpenseResponse>>> GetExpenseByUserId(int userId, [FromQuery] ExpenseQueryRequest query)
        {
            try
            {
                var expenses = await _expenseService.GetExpenseByUserIdAsync(userId, query);
                return Ok(expenses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
