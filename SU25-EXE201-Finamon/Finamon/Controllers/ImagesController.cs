using Finamon_Data.Entities;
using Finamon.Service.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Finamon.Service.Interfaces;
using Finamon_Data.Models;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.IO;

namespace Finamon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetImages([FromQuery] Finamon.Service.RequestModel.QueryRequest.ImageQueryRequest queryRequest)
        {
            var images = await _imageService.GetAllImagesAsync(queryRequest);
            return Ok(images);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetImage(int id)
        {
            var imageViewModel = await _imageService.GetImageByIdAsync(id);
            if (imageViewModel == null)
                return NotFound();
            return Ok(imageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateImage([FromForm] ImageCreateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest("File is required.");
            }

            string base64Image;
            using (var memoryStream = new MemoryStream())
            {
                await model.File.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                base64Image = Convert.ToBase64String(imageBytes);
            }

            var serviceRequest = new Finamon.Service.RequestModel.ImageRequest
            {
                Base64Image = base64Image,
                ContentType = model.File.ContentType
            };

            try
            {
                var imageViewModel = await _imageService.CreateImageAsync(serviceRequest);
                return CreatedAtAction(nameof(GetImage), new { id = imageViewModel.Id }, imageViewModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(int id, [FromForm] ImageUpdateModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest("File is required for update.");
            }

            string base64Image;
            using (var memoryStream = new MemoryStream())
            {
                await model.File.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                base64Image = Convert.ToBase64String(imageBytes);
            }

            var serviceRequest = new Finamon.Service.RequestModel.ImageRequest
            {
                Base64Image = base64Image,
                ContentType = model.File.ContentType
            };

            try
            {
                var imageViewModel = await _imageService.UpdateImageAsync(id, serviceRequest);
                if (imageViewModel == null)
                    return NotFound();
                return Ok(imageViewModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var result = await _imageService.DeleteImageAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}