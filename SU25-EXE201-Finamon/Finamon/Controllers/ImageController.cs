using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Finamon.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Finamon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IFirebaseStorageService _firebaseStorageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IFirebaseStorageService firebaseStorageService, ILogger<ImageController> logger)
        {
            _firebaseStorageService = firebaseStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a single image to Firebase Storage
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <returns>The public URL of the uploaded image</returns>
        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file was provided");
                }

                var imageUrl = await _firebaseStorageService.UploadImageAsync(file);
                return Ok(new { url = imageUrl });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid upload attempt: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading image: {ex.Message}");
                return StatusCode(500, "An error occurred while uploading the image");
            }
        }

        /// <summary>
        /// Upload multiple images to Firebase Storage
        /// </summary>
        /// <param name="files">The image files to upload</param>
        /// <returns>List of public URLs of the uploaded images</returns>
        [HttpPost("upload-multiple")]
        [Authorize]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB total limit
        public async Task<ActionResult<List<string>>> UploadMultipleImages([FromForm] List<IFormFile> files)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest("No files were provided");
                }

                var imageUrls = await _firebaseStorageService.UploadMultipleImagesAsync(files);
                return Ok(new { urls = imageUrls });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid upload attempt: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading images: {ex.Message}");
                return StatusCode(500, "An error occurred while uploading the images");
            }
        }

        /// <summary>
        /// Delete an image from Firebase Storage
        /// </summary>
        /// <param name="imageUrl">The public URL of the image to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("delete")]
        [Authorize]
        public async Task<ActionResult> DeleteImage([FromQuery] string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest("Image URL is required");
                }

                var success = await _firebaseStorageService.DeleteImageAsync(imageUrl);
                if (success)
                {
                    return Ok(new { message = "Image deleted successfully" });
                }
                return NotFound("Image not found or could not be deleted");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid delete attempt: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting image: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the image");
            }
        }
    }
} 