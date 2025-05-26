using Finamon.Service.DTOs.Request;
using Finamon.Service.DTOs.Response;
using Finamon.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finamon.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeywordsController : ControllerBase
    {
        private readonly IKeywordService _keywordService;

        public KeywordsController(IKeywordService keywordService)
        {
            _keywordService = keywordService;
        }

        [HttpPost]
        public async Task<ActionResult<KeywordResponse>> CreateKeyword([FromBody] CreateKeywordRequest request)
        {
            var response = await _keywordService.CreateKeywordAsync(request);
            return CreatedAtAction(nameof(GetKeywordById), new { id = response.Id }, response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KeywordResponse>> GetKeywordById(Guid id)
        {
            try
            {
                var keyword = await _keywordService.GetKeywordByIdAsync(id);
                return Ok(keyword);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeywordResponse>>> GetKeywords([FromQuery] KeywordQueryRequest request)
        {
            var keywords = await _keywordService.GetKeywordsAsync(request);
            Response.Headers.Add("X-Pagination", keywords.GetMetadata());
            return Ok(keywords);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<KeywordResponse>> UpdateKeyword(Guid id, [FromBody] UpdateKeywordRequest request)
        {
            try
            {
                var keyword = await _keywordService.UpdateKeywordAsync(id, request);
                return Ok(keyword);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKeyword(Guid id)
        {
            try
            {
                await _keywordService.DeleteKeywordAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
} 