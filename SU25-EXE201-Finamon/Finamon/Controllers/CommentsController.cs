using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Finamon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Uncomment this if you want to protect all endpoints
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/comments
        [HttpGet]
        public async Task<IActionResult> GetAllComments([FromQuery] CommentQueryRequest queryRequest)
        {
            var comments = await _commentService.GetAllCommentsAsync(queryRequest);
            return Ok(comments);
        }

        // GET: api/comments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComment(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound(new { Message = "Comment not found." });
            }
            return Ok(comment);
        }

        // GET: api/comments/post/{postId}
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsByPostId(int postId, [FromQuery] CommentQueryRequest queryRequest)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId, queryRequest);
            return Ok(comments);
        }

        // GET: api/comments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUserId(int userId, [FromQuery] CommentQueryRequest queryRequest)
        {
            var comments = await _commentService.GetCommentsByUserIdAsync(userId, queryRequest);
            return Ok(comments);
        }

        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentRequest commentRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Optionally, set UserId from logged-in user if not relying on request body
            // var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (currentUserId == null) return Unauthorized();
            // commentRequest.UserId = int.Parse(currentUserId);

            try
            {
                var createdComment = await _commentService.CreateCommentAsync(commentRequest);
                return CreatedAtAction(nameof(GetComment), new { id = createdComment.Id }, createdComment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        // PUT: api/comments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentUpdateRequest commentUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized(new { Message = "User not authenticated." });

            try
            {
                var updatedComment = await _commentService.UpdateCommentAsync(id, commentUpdateRequest, currentUserId.Value);
                if (updatedComment == null)
                {
                    return NotFound(new { Message = "Comment not found." });
                }
                return Ok(updatedComment);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        // DELETE: api/comments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized(new { Message = "User not authenticated." });

            try
            {
                var success = await _commentService.DeleteCommentAsync(id, currentUserId.Value);
                if (!success)
                {
                    return NotFound(new { Message = "Comment not found or user not authorized." });
                }
                return NoContent(); // Standard response for successful delete
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
} 