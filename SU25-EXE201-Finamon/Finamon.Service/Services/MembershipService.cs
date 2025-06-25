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
    public class MembershipService : IMembershipService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MembershipService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<MembershipResponse>> GetAllMembershipsAsync(MembershipQueryRequest queryRequest)
        {
            var queryable = _context.Memberships
                .Include(m => m.UserMemberships.Where(um => !um.IsDelete))
                .AsQueryable();

            // Handle soft delete
            if (queryRequest.IsDeleted.HasValue)
            {
                queryable = queryable.Where(m => m.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                queryable = queryable.Where(m => !m.IsDelete);
            }

            // Filtering
            if (!string.IsNullOrWhiteSpace(queryRequest.Name))
            {
                queryable = queryable.Where(m => m.Name.Contains(queryRequest.Name.Trim()));
            }
            if (queryRequest.MinMonthlyPrice.HasValue)
            {
                queryable = queryable.Where(m => m.MonthlyPrice >= queryRequest.MinMonthlyPrice.Value);
            }
            if (queryRequest.MaxMonthlyPrice.HasValue)
            {
                queryable = queryable.Where(m => m.MonthlyPrice <= queryRequest.MaxMonthlyPrice.Value);
            }
            if (queryRequest.MinYearlyPrice.HasValue)
            {
                queryable = queryable.Where(m => m.YearlyPrice >= queryRequest.MinYearlyPrice.Value);
            }
            if (queryRequest.MaxYearlyPrice.HasValue)
            {
                queryable = queryable.Where(m => m.YearlyPrice <= queryRequest.MaxYearlyPrice.Value);
            }

            // Sorting (default to Id if no SortBy is provided)
            if (queryRequest.SortBy.HasValue)
            {
                queryable = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(m => m.CreatedDate)
                        : queryable.OrderBy(m => m.CreatedDate),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(m => m.UpdatedDate)
                        : queryable.OrderBy(m => m.UpdatedDate),
                    SortByEnum.Amount => queryRequest.SortDescending
                        ? queryable.OrderByDescending(m => m.MonthlyPrice)
                        : queryable.OrderBy(m => m.MonthlyPrice),
                    _ => queryRequest.SortDescending
                        ? queryable.OrderByDescending(m => m.Id)
                        : queryable.OrderBy(m => m.Id)
                };
            }
            else
            {
                queryable = queryRequest.SortDescending
                    ? queryable.OrderByDescending(m => m.Id)
                    : queryable.OrderBy(m => m.Id);
            }

            var paginatedMemberships = await PaginatedResponse<Membership>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            var membershipResponses = _mapper.Map<List<MembershipResponse>>(paginatedMemberships.Items);
            return new PaginatedResponse<MembershipResponse>(membershipResponses, paginatedMemberships.TotalCount, paginatedMemberships.PageIndex, queryRequest.PageSize);
        }

        public async Task<MembershipResponse> GetMembershipByIdAsync(int id)
        {
            var membership = await _context.Memberships
                .Include(m => m.UserMemberships.Where(um => !um.IsDelete))
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDelete);

            if (membership == null)
            {
                throw new KeyNotFoundException($"Membership with ID {id} not found or has been deleted.");
            }

            var response = _mapper.Map<MembershipResponse>(membership);
            response.ActiveSubscriptions = membership.UserMemberships?.Count ?? 0;
            return response;
        }

        public async Task<MembershipResponse> CreateMembershipAsync(CreateMembershipRequest request)
        {
            // Check if membership with same name exists
            if (await _context.Memberships.AnyAsync(m => m.Name == request.Name.Trim() && !m.IsDelete))
            {
                throw new InvalidOperationException($"Membership with name '{request.Name}' already exists.");
            }

            var membership = _mapper.Map<Membership>(request);
            membership.Name = request.Name.Trim();
            membership.CreatedDate = DateTime.UtcNow.AddHours(7);
            membership.IsDelete = false;
            membership.UserMemberships = new List<UserMembership>();

            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();

            return await GetMembershipByIdAsync(membership.Id);
        }

        public async Task<MembershipResponse> UpdateMembershipAsync(int id, UpdateMembershipRequest request)
        {
            var membership = await _context.Memberships
                .Include(m => m.UserMemberships.Where(um => !um.IsDelete))
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDelete);

            if (membership == null)
            {
                throw new KeyNotFoundException($"Membership with ID {id} not found or has been deleted.");
            }

            // Check if new name already exists (if name is being updated)
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != membership.Name)
            {
                var nameExists = await _context.Memberships
                    .AnyAsync(m => m.Name == request.Name.Trim() && m.Id != id && !m.IsDelete);
                if (nameExists)
                {
                    throw new InvalidOperationException($"Membership with name '{request.Name}' already exists.");
                }
                membership.Name = request.Name.Trim();
            }

            // Update price if provided
            if (request.MonthlyPrice.HasValue)
            {
                if (request.MonthlyPrice.Value <= 0)
                {
                    throw new ArgumentException("Monthly price must be greater than 0.");
                }
                membership.MonthlyPrice = request.MonthlyPrice.Value;
            }

            // Update yearly price if provided
            if (request.YearlyPrice.HasValue)
            {
                if (request.YearlyPrice.Value <= 0)
                {
                    throw new ArgumentException("Yearly price must be greater than 0.");
                }
                membership.YearlyPrice = request.YearlyPrice.Value;
            }

            membership.UpdatedDate = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();

            return await GetMembershipByIdAsync(membership.Id);
        }

        public async Task<bool> DeleteMembershipAsync(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
            {
                throw new KeyNotFoundException($"Membership with ID {id} not found.");
            }

            // Check if there are any active UserMemberships associated with this Membership
            var hasActiveUserMemberships = await _context.UserMemberships
                .AnyAsync(um => um.MembershipId == id && !um.IsDelete);
            if (hasActiveUserMemberships)
            {
                throw new InvalidOperationException("Cannot delete membership as it has active user subscriptions.");
            }

            membership.IsDelete = true;
            membership.UpdatedDate = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 