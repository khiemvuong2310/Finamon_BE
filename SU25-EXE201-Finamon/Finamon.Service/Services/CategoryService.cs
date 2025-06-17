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
            if (queryRequest.SortBy.HasValue)
            {
                queryable = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.CreatedDate)
                        : queryable.OrderBy(c => c.CreatedDate),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.UpdatedDate)
                        : queryable.OrderBy(c => c.UpdatedDate),
                    SortByEnum.Amount => queryRequest.SortDescending
                        ? queryable.OrderByDescending(c => c.Expenses.Sum(e => e.Amount))
                        : queryable.OrderBy(c => c.Expenses.Sum(e => e.Amount)),
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
            // Validate if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && !u.IsDelete);
            if (!userExists)
                throw new KeyNotFoundException($"User with ID {request.UserId} not found");

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

            category.IsDelete = true; // Soft delete
            category.UpdatedDate = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryResponse>> CreateManyCategoriesAsync(CreateManyCategoriesRequest request)
        {
            if (request.Categories == null || !request.Categories.Any())
                throw new ArgumentException("At least one category is required");

            // Get all unique user IDs from the request
            var userIds = request.Categories.Select(c => c.UserId).Distinct().ToList();

            // Validate all users exist
            var existingUserIds = await _context.Users
                .Where(u => userIds.Contains(u.Id) && !u.IsDelete)
                .Select(u => u.Id)
                .ToListAsync();

            var notFoundUserIds = userIds.Except(existingUserIds).ToList();
            if (notFoundUserIds.Any())
                throw new KeyNotFoundException($"Users with IDs {string.Join(", ", notFoundUserIds)} not found");

            var categories = request.Categories.Select(categoryRequest =>
            {
                var category = _mapper.Map<Category>(categoryRequest);
                category.CreatedDate = DateTime.UtcNow.AddHours(7);
                return category;
            }).ToList();

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            var responses = _mapper.Map<IEnumerable<CategoryResponse>>(categories);
            return responses.ToArray(); // Ensure we return an array for ReactJS
        }

        public async Task<PaginatedResponse<CategoryResponse>> GetCategoryByUserIdAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            // Check if user exists and is not deleted
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId && !u.IsDelete);
            if (!userExists)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var queryable = _context.Categories
                .Include(c => c.Expenses)
                .Where(c => c.UserId == userId && !c.IsDelete)
                .OrderBy(c => c.CreatedDate);

            var paginatedCategories = await PaginatedResponse<Category>.CreateAsync(queryable, pageNumber, pageSize);
            var categoryResponses = _mapper.Map<List<CategoryResponse>>(paginatedCategories.Items);
            
            return new PaginatedResponse<CategoryResponse>(categoryResponses, paginatedCategories.TotalCount, paginatedCategories.PageIndex, pageSize);
        }
    }
} 