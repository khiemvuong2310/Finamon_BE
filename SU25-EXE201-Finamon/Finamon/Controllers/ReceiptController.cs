using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Finamon.Service.Services;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Finamon.Service.ReponseModel;
using Finamon.Service.Interfaces;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ReceiptQueryRequest query)
        {
            var result = await _receiptService.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _receiptService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReceiptRequestModel model)
        {
            if (model == null) return BadRequest();
            var result = await _receiptService.CreateAsync(model);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReceiptUpdateRequestModel model)
        {
            if (model == null)
                return BadRequest();
            // Nếu tất cả các trường đều null/0/rỗng thì trả về BadRequest
            if (model.UserId == 0 && model.MembershipId == 0 && !model.Amount.HasValue && !model.Status.HasValue && string.IsNullOrEmpty(model.Note))
                return BadRequest("At least one field must be provided to update.");
            var result = await _receiptService.UpdateAsync(id, model);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _receiptService.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok();
        }
    }
} 