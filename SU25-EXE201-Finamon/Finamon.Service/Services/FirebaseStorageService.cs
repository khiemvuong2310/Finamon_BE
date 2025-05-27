using Finamon.Service.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon.Service.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<FirebaseStorageService> _logger;

        public FirebaseStorageService(IConfiguration configuration, ILogger<FirebaseStorageService> logger)
        {
            _logger = logger;

            // Khởi tạo Storage Client từ environment variable
            var firebaseConfig = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            if (string.IsNullOrEmpty(firebaseConfig))
            {
                throw new ArgumentNullException("FIREBASE_CONFIG environment variable is not set");
            }

            var credential = GoogleCredential.FromJson(firebaseConfig);
            _storageClient = StorageClient.Create(credential);
            
            // Lấy bucket name từ environment variable
            _bucketName = Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET") 
                ?? throw new ArgumentNullException("FIREBASE_STORAGE_BUCKET environment variable is not set");
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or null");

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    throw new ArgumentException($"File type {file.ContentType} is not allowed");

                // Validate file size (max 10MB)
                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("File size exceeds 5mb limit");

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Upload to Firebase Storage
                using var stream = file.OpenReadStream();

                var storageObject = new Google.Apis.Storage.v1.Data.Object
                {
                    Bucket = _bucketName,
                    Name = fileName,
                    ContentType = file.ContentType,
                    Acl = new List<Google.Apis.Storage.v1.Data.ObjectAccessControl>
                    {
                        new Google.Apis.Storage.v1.Data.ObjectAccessControl
                        {
                            Entity = "allUsers",
                            Role = "READER"
                        }
                    }
                };

                await _storageClient.UploadObjectAsync(storageObject, stream);

                // Return public URL
                var publicUrl = $"https://storage.googleapis.com/{_bucketName}/{fileName}";

                _logger.LogInformation($"Successfully uploaded image: {publicUrl}");
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading image: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files)
        {
            // Validate total size
            long totalSize = files.Sum(f => f.Length);
            if (totalSize > 50 * 1024 * 1024) // 50MB total
            {
                throw new ArgumentException("Total file size exceeds 50MB limit");
            }

            var uploadTasks = files.Select(file => UploadImageAsync(file));
            var results = await Task.WhenAll(uploadTasks);
            return results.ToList();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri? uri))
                {
                    throw new ArgumentException("Invalid image URL format");
                }

                if (!uri.Host.Contains("storage.googleapis.com"))
                {
                    throw new ArgumentException("URL is not from Firebase Storage");
                }

                // Extract filename from URL
                var fileName = uri.AbsolutePath.TrimStart('/').Replace($"{_bucketName}/", "");

                await _storageClient.DeleteObjectAsync(_bucketName, fileName);

                _logger.LogInformation($"Successfully deleted image: {imageUrl}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image {imageUrl}: {ex.Message}");
                return false;
            }
        }
    }
}
