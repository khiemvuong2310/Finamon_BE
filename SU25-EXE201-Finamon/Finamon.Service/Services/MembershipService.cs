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
            var queryable = _context.Memberships.AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(queryRequest.Name))
            {
                queryable = queryable.Where(m => m.Name.Contains(queryRequest.Name));
            }
            if (queryRequest.MinPrice.HasValue)
            {
                queryable = queryable.Where(m => m.Price >= queryRequest.MinPrice.Value);
            }
            if (queryRequest.MaxPrice.HasValue)
            {
                queryable = queryable.Where(m => m.Price <= queryRequest.MaxPrice.Value);
            }
            if (queryRequest.MinDuration.HasValue)
            {
                queryable = queryable.Where(m => m.Duration >= queryRequest.MinDuration.Value);
            }
            if (queryRequest.MaxDuration.HasValue)
            {
                queryable = queryable.Where(m => m.Duration <= queryRequest.MaxDuration.Value);
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
                        ? queryable.OrderByDescending(m => m.Price)
                        : queryable.OrderBy(m => m.Price),
                    _ => queryable.OrderBy(m => m.Id)
                };
            }
            else
            {
                queryable = queryable.OrderBy(m => m.Id);
            }

            var paginatedMemberships = await PaginatedResponse<Membership>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            var membershipResponses = _mapper.Map<List<MembershipResponse>>(paginatedMemberships.Items);
            return new PaginatedResponse<MembershipResponse>(membershipResponses, paginatedMemberships.TotalCount, paginatedMemberships.PageIndex, queryRequest.PageSize);
        }

        public async Task<MembershipResponse> GetMembershipByIdAsync(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
            {
                throw new KeyNotFoundException($"Membership with ID {id} not found.");
            }
            return _mapper.Map<MembershipResponse>(membership);
        }

        public async Task<MembershipResponse> CreateMembershipAsync(CreateMembershipRequest request)
        {
            var membership = _mapper.Map<Membership>(request);
            membership.CreatedDate = DateTime.UtcNow.AddHours(7); // Assuming UTC+7 for your timezone
            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();
            return _mapper.Map<MembershipResponse>(membership);
        }

        public async Task<MembershipResponse> UpdateMembershipAsync(int id, UpdateMembershipRequest request)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
            {
                throw new KeyNotFoundException($"Membership with ID {id} not found.");
            }

            _mapper.Map(request, membership);
            membership.UpdatedDate = DateTime.UtcNow.AddHours(7); // Assuming UTC+7
            await _context.SaveChangesAsync();
            return _mapper.Map<MembershipResponse>(membership);
        }

        public async Task<bool> DeleteMembershipAsync(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
            {
                throw new KeyNotFoundException($"Membership with ID {id} not found.");
            }

            // Check if there are any UserMemberships associated with this Membership
            var hasUserMemberships = await _context.UserMemberships.AnyAsync(um => um.MembershipId == id);
            if (hasUserMemberships)
            {
                // You might want to prevent deletion or handle this case differently
                // For now, let's throw an exception or return false
                throw new InvalidOperationException("Cannot delete membership as it is associated with existing user memberships.");
                // Alternatively, return false;
            }

            _context.Memberships.Remove(membership);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 