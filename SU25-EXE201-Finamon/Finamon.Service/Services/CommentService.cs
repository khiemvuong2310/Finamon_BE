using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Finamon.Service.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CommentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private IQueryable<Comment> GetBaseQuery()
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Where(c => !c.IsDelete);
        }

        public async Task<PaginatedResponse<CommentResponse>> GetAllCommentsAsync(CommentQueryRequest queryRequest)
        {
            var queryable = GetBaseQuery();

            if (queryRequest.UserId.HasValue)
            {
                queryable = queryable.Where(c => c.UserId == queryRequest.UserId.Value);
            }
            if (queryRequest.PostId.HasValue)
            {
                queryable = queryable.Where(c => c.PostId == queryRequest.PostId.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryRequest.ContentSearch))
            {
                queryable = queryable.Where(c => c.Content.Contains(queryRequest.ContentSearch));
            }
            if (queryRequest.DateFrom.HasValue)
            {
                queryable = queryable.Where(c => c.CreatedDate >= queryRequest.DateFrom.Value);
            }
            if (queryRequest.DateTo.HasValue)
            {
                queryable = queryable.Where(c => c.CreatedDate <= queryRequest.DateTo.Value);
            }

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
                    _ => queryable.OrderByDescending(c => c.CreatedDate) // Default sort
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(c => c.CreatedDate); // Default sort
            }

            var paginatedComments = await PaginatedResponse<Comment>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            var commentResponses = _mapper.Map<System.Collections.Generic.List<CommentResponse>>(paginatedComments.Items);
            return new PaginatedResponse<CommentResponse>(commentResponses, paginatedComments.TotalCount, paginatedComments.PageIndex, queryRequest.PageSize);
        }

        public async Task<CommentResponse> GetCommentByIdAsync(int id)
        {
            var comment = await GetBaseQuery().FirstOrDefaultAsync(c => c.Id == id);
            return _mapper.Map<CommentResponse>(comment);
        }

        public async Task<PaginatedResponse<CommentResponse>> GetCommentsByPostIdAsync(int postId, CommentQueryRequest queryRequest)
        {
            queryRequest.PostId = postId; // Ensure PostId is set for filtering
            return await GetAllCommentsAsync(queryRequest);
        }

        public async Task<PaginatedResponse<CommentResponse>> GetCommentsByUserIdAsync(int userId, CommentQueryRequest queryRequest)
        {
            queryRequest.UserId = userId; // Ensure UserId is set for filtering
            return await GetAllCommentsAsync(queryRequest);
        }

        public async Task<CommentResponse> CreateCommentAsync(CommentRequest commentRequest)
        {
            // Validate if User and Blog exist
            var userExists = await _context.Users.AnyAsync(u => u.Id == commentRequest.UserId && !u.IsDelete);
            if (!userExists) throw new ArgumentException("User not found.");

            var blogExists = await _context.Blogs.AnyAsync(b => b.Id == commentRequest.PostId && !b.IsDelete);
            if (!blogExists) throw new ArgumentException("Blog post not found.");

            var comment = _mapper.Map<Comment>(commentRequest);
            comment.CreatedDate = DateTime.UtcNow;
            comment.UpdatedDate = DateTime.UtcNow;
            comment.IsDelete = false;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            
            // Re-fetch to include User and Blog details for the response
            var createdComment = await GetBaseQuery().FirstOrDefaultAsync(c => c.Id == comment.Id);
            return _mapper.Map<CommentResponse>(createdComment);
        }

        public async Task<CommentResponse> UpdateCommentAsync(int id, CommentUpdateRequest commentUpdateRequest, int userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete);

            if (comment == null) return null; // Or throw NotFoundException
            if (comment.UserId != userId) throw new UnauthorizedAccessException("User is not authorized to update this comment.");

            _mapper.Map(commentUpdateRequest, comment);
            comment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            var updatedComment = await GetBaseQuery().FirstOrDefaultAsync(c => c.Id == comment.Id);
            return _mapper.Map<CommentResponse>(updatedComment);
        }

        public async Task<bool> DeleteCommentAsync(int id, int userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete);

            if (comment == null) return false;
            // Add role-based check later if admins can delete any comment
            if (comment.UserId != userId) throw new UnauthorizedAccessException("User is not authorized to delete this comment."); 

            comment.IsDelete = true;
            comment.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 