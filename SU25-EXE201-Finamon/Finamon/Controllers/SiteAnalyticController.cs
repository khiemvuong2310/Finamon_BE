using Microsoft.AspNetCore.Mvc;
using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteAnalyticController : ControllerBase
    {
        private readonly ISiteAnalyticService _siteAnalyticService;

        public SiteAnalyticController(ISiteAnalyticService siteAnalyticService)
        {
            _siteAnalyticService = siteAnalyticService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SiteAnalyticQueryRequest request)
        {
            var analytics = await _siteAnalyticService.GetAllAsync(request);
            return Ok(analytics);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var analytic = await _siteAnalyticService.GetByIdAsync(id);
                return Ok(analytic);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SiteAnalyticRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var analytic = await _siteAnalyticService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = analytic.Id }, analytic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SiteAnalyticRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var analytic = await _siteAnalyticService.UpdateAsync(id, request);
                return Ok(analytic);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _siteAnalyticService.DeleteAsync(id);
            if (!result)
                return NotFound($"Site analytic with ID {id} not found");

            return NoContent();
        }

        [HttpPost("{id}/increment")]
        public async Task<IActionResult> IncrementCount(int id)
        {
            try
            {
                var analytic = await _siteAnalyticService.IncrementCountAsync(id);
                return Ok(analytic);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 