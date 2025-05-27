using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
