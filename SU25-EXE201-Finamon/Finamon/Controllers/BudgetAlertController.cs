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
    public class BudgetAlertController : ControllerBase
    {
        private readonly IBudgetAlertService _alertService;

        public BudgetAlertController(IBudgetAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<BudgetAlertResponse>>> GetAllAlerts([FromQuery] BudgetAlertQueryRequest queryRequest)
        {
            var alerts = await _alertService.GetAllAlertsAsync(queryRequest);
            return Ok(alerts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetAlertResponse>> GetAlertById(int id)
        {
            try
            {
                var alert = await _alertService.GetAlertByIdAsync(id);
                return Ok(alert);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("budget/{budgetId}")]
        public async Task<ActionResult<PaginatedResponse<BudgetAlertResponse>>> GetAlertsByBudgetId(int budgetId, [FromQuery] BudgetAlertQueryRequest queryRequest)
        {
            var alerts = await _alertService.GetAlertsByBudgetIdAsync(budgetId, queryRequest);
            return Ok(alerts);
        }

        [HttpPost]
        public async Task<ActionResult<BudgetAlertResponse>> CreateAlert(BudgetAlertRequestModel request)
        {
            try
            {
                var alert = await _alertService.CreateAlertAsync(request);
                return CreatedAtAction(nameof(GetAlertById), new { id = alert.Id }, alert);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BudgetAlertResponse>> UpdateAlert(int id, BudgetAlertRequestModel request)
        {
            try
            {
                var alert = await _alertService.UpdateAlertAsync(id, request);
                return Ok(alert);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="Admmin,Staff")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            try
            {
                await _alertService.DeleteAlertAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 