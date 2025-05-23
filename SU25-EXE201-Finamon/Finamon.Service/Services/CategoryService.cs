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

        public async Task<PaginatedResponse<CategoryResponse>> GetAllCategoriesAsync(CategoryQueryRequest queryRequest)
        {
            var queryable = _context.Categories
                .Include(c => c.Expenses)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(queryRequest.Name))
            {
                queryable = queryable.Where(c => c.Name.Contains(queryRequest.Name));
            }

            if (!string.IsNullOrWhiteSpace(queryRequest.Color))
            {
                queryable = queryable.Where(c => c.Color.Contains(queryRequest.Color));
            }

            if (queryRequest.CreatedFrom.HasValue)
            {
                queryable = queryable.Where(c => c.CreatedDate >= queryRequest.CreatedFrom.Value);
            }

            if (queryRequest.CreatedTo.HasValue)
            {
                queryable = queryable.Where(c => c.CreatedDate <= queryRequest.CreatedTo.Value);
            }
            
            // Default to not showing deleted if not specified
            if (queryRequest.IsDeleted.HasValue)
            {
                queryable = queryable.Where(c => c.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                queryable = queryable.Where(c => !c.IsDelete);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryRequest.SortBy))
            {
                queryable = queryRequest.SortBy.ToLower() switch
                {
                    "name" => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.Name)
                        : queryable.OrderBy(c => c.Name),
                    "color" => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.Color)
                        : queryable.OrderBy(c => c.Color),
                    "createddate" => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.CreatedDate)
                        : queryable.OrderBy(c => c.CreatedDate),
                    "updateddate" => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.UpdatedDate)
                        : queryable.OrderBy(c => c.UpdatedDate),
                    _ => queryable.OrderBy(c => c.Id) // Default sort by Id
                };
            }
            else
            {
                queryable = queryable.OrderBy(c => c.Id); // Default sort by Id
            }

            var paginatedCategories = await PaginatedResponse<Category>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            var categoryResponses = _mapper.Map<List<CategoryResponse>>(paginatedCategories.Items);
            
            return new PaginatedResponse<CategoryResponse>(categoryResponses, paginatedCategories.TotalCount, paginatedCategories.PageIndex, queryRequest.PageSize);
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