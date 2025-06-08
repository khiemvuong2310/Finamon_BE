using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryAlertController : ControllerBase
    {
        private readonly ICategoryAlertService _categoryAlertService;

        public CategoryAlertController(ICategoryAlertService categoryAlertService)
        {
            _categoryAlertService = categoryAlertService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CategoryAlertResponse>>> GetAllCategoryAlerts([FromQuery] CategoryAlertQueryRequest queryRequest)
        {
            try
            {
                var alerts = await _categoryAlertService.GetAllCategoryAlertsAsync(queryRequest);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving category alerts.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryAlertResponse>> GetCategoryAlertById(int id)
        {
            try
            {
                var alert = await _categoryAlertService.GetCategoryAlertByIdAsync(id);
                return Ok(alert);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the category alert.", error = ex.Message });
            }
        }

        [HttpGet("budgetCategory/{budgetCategoryId}")]
        public async Task<ActionResult<PaginatedResponse<CategoryAlertResponse>>> GetAlertsByBudgetCategoryId(int budgetCategoryId, [FromQuery] CategoryAlertQueryRequest queryRequest)
        {
            try
            {
                var alerts = await _categoryAlertService.GetAlertsByBudgetCategoryIdAsync(budgetCategoryId, queryRequest);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving category alerts.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CategoryAlertResponse>> CreateCategoryAlert([FromBody] CategoryAlertRequestModel request)
        {
            try
            {
                var createdAlert = await _categoryAlertService.CreateCategoryAlertAsync(request);
                return CreatedAtAction(nameof(GetCategoryAlertById), new { id = createdAlert.Id }, createdAlert);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the category alert.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryAlertResponse>> UpdateCategoryAlert(int id, [FromBody] CategoryAlertRequestModel request)
        {
            try
            {
                var updatedAlert = await _categoryAlertService.UpdateCategoryAlertAsync(id, request);
                return Ok(updatedAlert);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the category alert.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoryAlert(int id)
        {
            try
            {
                await _categoryAlertService.DeleteCategoryAlertAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the category alert.", error = ex.Message });
            }
        }
    }
}