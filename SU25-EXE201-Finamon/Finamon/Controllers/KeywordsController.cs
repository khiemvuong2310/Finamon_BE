using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.API.Controllers // Adjusted to match the folder structure you provided
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
        public async Task<IActionResult> CreateKeyword([FromBody] CreateKeywordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _keywordService.CreateKeywordAsync(request);
            return CreatedAtAction(nameof(GetKeywordById), new { id = response.Id }, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetKeywordById(Guid id)
        {
            try
            {
                var keyword = await _keywordService.GetKeywordByIdAsync(id);
                return Ok(keyword);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKeywords([FromQuery] KeywordQueryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var keywords = await _keywordService.GetKeywordsAsync(request);
            // Assuming PaginatedResponse has a way to get metadata, e.g., through properties
            // Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new { keywords.TotalCount, keywords.PageSize, keywords.PageIndex, keywords.TotalPages, keywords.HasNextPage, keywords.HasPreviousPage }));
            return Ok(keywords);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateKeyword(Guid id, [FromBody] UpdateKeywordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var keyword = await _keywordService.UpdateKeywordAsync(id, request);
                return Ok(keyword);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteKeyword(Guid id)
        {
            try
            {
                await _keywordService.DeleteKeywordAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(knfex.Message);
            }
        }
    }
} 