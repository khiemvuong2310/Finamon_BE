using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetAlertController : ControllerBase
    {
        private readonly IBudgetAlertService _alertService;

        public BudgetAlertController(IBudgetAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetAlertResponse>>> GetAllAlerts([FromQuery] BudgetAlertQueryRequest query)
        {
            var alerts = await _alertService.GetAllAlertsAsync(query);
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
        public async Task<ActionResult<IEnumerable<BudgetAlertResponse>>> GetAlertsByBudgetId(int budgetId)
        {
            var alerts = await _alertService.GetAlertsByBudgetIdAsync(budgetId);
            return Ok(alerts);
        }

        [HttpPost]
        public async Task<ActionResult<BudgetAlertResponse>> CreateAlert(BudgetAlertRequestModel request)
        {
            var alert = await _alertService.CreateAlertAsync(request);
            return CreatedAtAction(nameof(GetAlertById), new { id = alert.Id }, alert);
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
        }

        [HttpDelete("{id}")]
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