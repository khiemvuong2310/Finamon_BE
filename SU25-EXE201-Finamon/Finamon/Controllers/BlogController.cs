using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<BlogResponse>>> CreateBlog([FromBody] CreateBlogRequest request)
        {
            var response = await _blogService.CreateBlogAsync(request);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<BlogResponse>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<BlogResponse>>> UpdateBlog(int id, [FromBody] UpdateBlogRequest request)
        {
            var response = await _blogService.UpdateBlogAsync(id, request);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<BlogResponse>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse>> DeleteBlog(int id)
        {
            var response = await _blogService.DeleteBlogAsync(id);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<BlogResponse>>> GetBlogById(int id)
        {
            var response = await _blogService.GetBlogByIdAsync(id);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<BlogResponse>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<PagedBlogResponse>>> GetAllBlogs([FromQuery] BlogFilterRequest filter)
        {
            var response = await _blogService.GetAllBlogsAsync(filter);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<PagedBlogResponse>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }
    }
} 