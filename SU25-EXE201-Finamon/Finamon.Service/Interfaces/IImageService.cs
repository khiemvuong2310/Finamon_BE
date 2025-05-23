using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IImageService
    {
        Task<IEnumerable<ImageResponse>> GetAllImagesAsync(ImageQueryRequest queryRequest);
        Task<ImageResponse> GetImageByIdAsync(int id);
        Task<ImageResponse> CreateImageAsync(ImageRequest imageRequest);
        Task<ImageResponse> UpdateImageAsync(int id, ImageRequest imageRequest);
        Task<bool> DeleteImageAsync(int id);
    }
}