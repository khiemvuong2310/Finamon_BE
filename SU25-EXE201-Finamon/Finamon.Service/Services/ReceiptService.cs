using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel.QueryRequest;
using Microsoft.EntityFrameworkCore;
using Finamon.Service.Interfaces;

namespace Finamon.Service.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public ReceiptService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<ReceiptResponseModel>> GetAllAsync(ReceiptQueryRequest query)
        {
            try
            {
                var source = _context.Receipts.AsQueryable();

                // Filter by UserId
                if (query.UserId.HasValue)
                    source = source.Where(r => r.UserId == query.UserId.Value);

                // Filter by MembershipId
                if (query.MembershipId.HasValue)
                    source = source.Where(r => r.MembershipId == query.MembershipId.Value);

                // Filter by Status
                if (query.Status.HasValue)
                    source = source.Where(r => r.Status == query.Status.Value);

                // Filter by CreatedDate range
                if (query.CreatedFrom.HasValue)
                    source = source.Where(r => r.CreatedDate >= query.CreatedFrom.Value);
                if (query.CreatedTo.HasValue)
                    source = source.Where(r => r.CreatedDate <= query.CreatedTo.Value);

                // Luôn chỉ lấy các bản ghi chưa bị xóa
                source = source.Where(r => !r.IsDelete);

                // Sắp xếp theo SortBy nếu có
                if (query.SortBy.HasValue)
                {
                    switch (query.SortBy.Value)
                    {
                        case SortByEnum.CreatedDate:
                            source = query.SortDescending ? source.OrderByDescending(r => r.CreatedDate) : source.OrderBy(r => r.CreatedDate);
                            break;
                        case SortByEnum.UpdatedDate:
                            source = query.SortDescending ? source.OrderByDescending(r => r.UpdatedDate) : source.OrderBy(r => r.UpdatedDate);
                            break;
                        case SortByEnum.Amount:
                            source = query.SortDescending ? source.OrderByDescending(r => r.Amount) : source.OrderBy(r => r.Amount);
                            break;
                        default:
                            source = source.OrderByDescending(r => r.Id);
                            break;
                    }
                }
                else
                {
                    source = source.OrderByDescending(r => r.Id);
                }

                var paginated = await PaginatedResponse<Receipt>.CreateAsync(source, query.PageNumber, query.PageSize);
                var mapped = paginated.Items.Select(r => _mapper.Map<ReceiptResponseModel>(r)).ToList();
                return new PaginatedResponse<ReceiptResponseModel>(mapped, paginated.TotalCount, paginated.PageIndex, query.PageSize);
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây
                throw new Exception($"Error getting receipts: {ex.Message}", ex);
            }
        }

        public async Task<ReceiptResponseModel> GetByIdAsync(int id)
        {
            try
            {
                var receipt = await _context.Receipts.FindAsync(id);
                if (receipt == null || receipt.IsDelete)
                    return null;
                return _mapper.Map<ReceiptResponseModel>(receipt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting receipt by id: {ex.Message}", ex);
            }
        }

        public async Task<ReceiptResponseModel> CreateAsync(ReceiptRequestModel model)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                var receipt = _mapper.Map<Receipt>(model);
                receipt.CreatedDate = DateTime.UtcNow.AddHours(7);
                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();
                return _mapper.Map<ReceiptResponseModel>(receipt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating receipt: {ex.Message}", ex);
            }
        }

        public async Task<ReceiptResponseModel> UpdateAsync(int id, ReceiptUpdateRequestModel model)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                var receipt = await _context.Receipts.FindAsync(id);
                if (receipt == null || receipt.IsDelete)
                    return null;

                // Chỉ update các trường có giá trị khác null
                if (model.UserId != 0)
                    receipt.UserId = model.UserId;
                if (model.MembershipId != 0)
                    receipt.MembershipId = model.MembershipId;
                if (model.Amount.HasValue)
                    receipt.Amount = model.Amount;
                if (model.Status.HasValue)
                    receipt.Status = model.Status.Value;
                if (!string.IsNullOrEmpty(model.Note))
                    receipt.Note = model.Note;
                receipt.UpdatedDate = DateTime.UtcNow.AddHours(7);

                await _context.SaveChangesAsync();
                return _mapper.Map<ReceiptResponseModel>(receipt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating receipt: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var receipt = await _context.Receipts.FindAsync(id);
                if (receipt == null || receipt.IsDelete)
                    return false;
                receipt.IsDelete = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting receipt: {ex.Message}", ex);
            }
        }
    }
}