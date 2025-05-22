using Finamon.Repo.UnitOfWork;
using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Finamon_Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;

namespace Finamon.Service.Services
{
    public class BlogService : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BlogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<BlogResponse>> CreateBlogAsync(CreateBlogRequest request)
        {
            try
            {
                // Validate request
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new BaseResponse<BlogResponse>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = string.Join(", ", validationResults.Select(r => r.ErrorMessage))
                    };
                }

                var blog = new Blog
                {
                    Title = request.Title?.Trim(),
                    Content = request.Content?.Trim(),
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Blog>().InsertAsync(blog);
                await _unitOfWork.CommitAsync();

                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Blog created successfully",
                    Data = _mapper.Map<BlogResponse>(blog)
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to create blog: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse<BlogResponse>> UpdateBlogAsync(int id, UpdateBlogRequest request)
        {
            try
            {
                // Validate request
                var validationContext = new ValidationContext(request);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
                {
                    return new BaseResponse<BlogResponse>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = string.Join(", ", validationResults.Select(r => r.ErrorMessage))
                    };
                }

                var blog = await _unitOfWork.Repository<Blog>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

                if (blog == null)
                {
                    return new BaseResponse<BlogResponse>
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Blog not found"
                    };
                }

                blog.Title = request.Title?.Trim();
                blog.Content = request.Content?.Trim();
                blog.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.Repository<Blog>().Update(blog, blog.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Blog updated successfully",
                    Data = _mapper.Map<BlogResponse>(blog)
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to update blog: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse> DeleteBlogAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid blog ID"
                    };
                }

                var blog = await _unitOfWork.Repository<Blog>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

                if (blog == null)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Blog not found"
                    };
                }

                blog.IsDelete = true;
                blog.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.Repository<Blog>().Update(blog, blog.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Blog deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to delete blog: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse<BlogResponse>> GetBlogByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new BaseResponse<BlogResponse>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid blog ID"
                    };
                }

                var blog = await _unitOfWork.Repository<Blog>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete);

                if (blog == null)
                {
                    return new BaseResponse<BlogResponse>
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "Blog not found"
                    };
                }

                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Blog retrieved successfully",
                    Data = _mapper.Map<BlogResponse>(blog)
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<BlogResponse>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to retrieve blog: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse<PagedBlogResponse>> GetAllBlogsAsync(BlogFilterRequest filter)
        {
            try
            {
                // Validate filter
                var validationContext = new ValidationContext(filter);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(filter, validationContext, validationResults, true))
                {
                    return new BaseResponse<PagedBlogResponse>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = string.Join(", ", validationResults.Select(r => r.ErrorMessage))
                    };
                }

                var query = _unitOfWork.Repository<Blog>()
                    .AsQueryable()
                    .Where(b => !b.IsDelete);

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim().ToLower();
                    query = query.Where(b => 
                        (b.Title != null && b.Title.ToLower().Contains(searchTerm)) || 
                        (b.Content != null && b.Content.ToLower().Contains(searchTerm)));
                }

                if (filter.FromDate.HasValue)
                {
                    query = query.Where(b => b.CreatedDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(b => b.CreatedDate <= filter.ToDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var blogs = await query
                    .OrderByDescending(b => b.CreatedDate)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = new PagedBlogResponse
                {
                    Items = _mapper.Map<List<BlogResponse>>(blogs),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };

                return new BaseResponse<PagedBlogResponse>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Blogs retrieved successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<PagedBlogResponse>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to retrieve blogs: " + ex.Message
                };
            }
        }
    }
} 