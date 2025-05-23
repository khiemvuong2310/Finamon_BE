using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finamon.Service.Interfaces;

namespace Finamon.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ImageService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ImageResponse>> GetAllImagesAsync(ImageQueryRequest queryRequest)
        {
            var query = _context.Images.AsQueryable();

            if (queryRequest.IsDeleted.HasValue)
            {
                query = query.Where(i => i.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                query = query.Where(i => !i.IsDelete); // Default to not showing deleted
            }

            // Sorting
            if (!string.IsNullOrEmpty(queryRequest.SortBy))
            {
                switch (queryRequest.SortBy.ToLower())
                {
                    case "createdat":
                        query = queryRequest.SortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt);
                        break;
                    case "updatedat":
                        query = queryRequest.SortDescending ? query.OrderByDescending(i => i.UpdatedAt) : query.OrderBy(i => i.UpdatedAt);
                        break;
                    default:
                        query = query.OrderBy(i => i.Id); // Default sort by Id
                        break;
                }
            }
            else
            {
                query = query.OrderBy(i => i.Id); // Default sort by Id
            }

            // Pagination
            var images = await query
                .Skip((queryRequest.PageNumber - 1) * queryRequest.PageSize)
                .Take(queryRequest.PageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ImageResponse>>(images);
        }

        public async Task<ImageResponse> GetImageByIdAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null || image.IsDelete)
            {
                return null;
            }
            return _mapper.Map<ImageResponse>(image);
        }

        public async Task<ImageResponse> CreateImageAsync(ImageRequest imageRequest)
        {
            var image = _mapper.Map<Image>(imageRequest);
            image.CreatedAt = DateTime.UtcNow;
            image.UpdatedAt = DateTime.UtcNow;
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return _mapper.Map<ImageResponse>(image);
        }

        public async Task<ImageResponse> UpdateImageAsync(int id, ImageRequest imageRequest)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null || image.IsDelete)
            {
                return null;
            }

            _mapper.Map(imageRequest, image);
            image.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return _mapper.Map<ImageResponse>(image);
        }

        public async Task<bool> DeleteImageAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null || image.IsDelete)
            {
                return false;
            }

            image.IsDelete = true;
            image.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 