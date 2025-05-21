using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finamon.Service.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync(CategoryQueryRequest query)
        {
            var categories = _context.Categories
                .Include(c => c.Expenses)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                categories = categories.Where(c => c.Name.Contains(query.Name));
            }

            if (!string.IsNullOrWhiteSpace(query.Color))
            {
                categories = categories.Where(c => c.Color.Contains(query.Color));
            }

            if (query.CreatedFrom.HasValue)
            {
                categories = categories.Where(c => c.CreatedDate >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                categories = categories.Where(c => c.CreatedDate <= query.CreatedTo.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                categories = query.SortBy.ToLower() switch
                {
                    "name" => query.SortDescending
                        ? categories.OrderByDescending(c => c.Name)
                        : categories.OrderBy(c => c.Name),
                    "color" => query.SortDescending
                        ? categories.OrderByDescending(c => c.Color)
                        : categories.OrderBy(c => c.Color),
                    "createddate" => query.SortDescending
                        ? categories.OrderByDescending(c => c.CreatedDate)
                        : categories.OrderBy(c => c.CreatedDate),
                    "updateddate" => query.SortDescending
                        ? categories.OrderByDescending(c => c.UpdatedDate)
                        : categories.OrderBy(c => c.UpdatedDate),
                    _ => categories
                };
            }

            // Apply pagination
            if (query.PageSize > 0)
            {
                categories = categories
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            var result = await categories.ToListAsync();
            return _mapper.Map<IEnumerable<CategoryResponse>>(result);
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found");

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequestModel request)
        {
            var category = _mapper.Map<Category>(request);
            category.CreatedDate = DateTime.UtcNow.AddHours(7);

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, CategoryRequestModel request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found");

            _mapper.Map(request, category);
            category.UpdatedDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
} 