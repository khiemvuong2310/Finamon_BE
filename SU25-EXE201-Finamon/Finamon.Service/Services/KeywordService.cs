using AutoMapper;
using Finamon.Repo.UnitOfWork;
using Finamon.Service.Interfaces;
using Finamon_Data.Entities;
using Microsoft.EntityFrameworkCore;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;

namespace Finamon.Service.Services
{
    public class KeywordService : IKeywordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public KeywordService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<KeywordResponse> CreateKeywordAsync(CreateKeywordRequest request)
        {
            var keyword = _mapper.Map<Keyword>(request);
            await _unitOfWork.Repository<Keyword>().InsertAsync(keyword);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<KeywordResponse>(keyword);
        }

        public async Task DeleteKeywordAsync(Guid id)
        {
            var keyword = await _unitOfWork.Repository<Keyword>().GetByIdGuid(id);
            if (keyword == null)
                throw new KeyNotFoundException($"Keyword with ID {id} not found");

            await _unitOfWork.Repository<Keyword>().DeleteAsync(keyword);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<KeywordResponse> GetKeywordByIdAsync(Guid id)
        {
            var keyword = await _unitOfWork.Repository<Keyword>().GetByIdGuid(id);
            if (keyword == null)
                throw new KeyNotFoundException($"Keyword with ID {id} not found");

            return _mapper.Map<KeywordResponse>(keyword);
        }

        public async Task<PaginatedResponse<KeywordResponse>> GetKeywordsAsync(KeywordQueryRequest request)
        {
            var query = _unitOfWork.Repository<Keyword>().GetAll();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(k => k.Text.Contains(request.SearchTerm));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "text" => request.IsDescending ? query.OrderByDescending(k => k.Text) : query.OrderBy(k => k.Text),
                "createdat" => request.IsDescending ? query.OrderByDescending(k => k.CreatedAt) : query.OrderBy(k => k.CreatedAt),
                _ => request.IsDescending ? query.OrderByDescending(k => k.CreatedAt) : query.OrderBy(k => k.CreatedAt)
            };

            var pagedKeywords = await PaginatedResponse<Keyword>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);
            
            var keywordResponses = _mapper.Map<List<KeywordResponse>>(pagedKeywords.Items);
            return new PaginatedResponse<KeywordResponse>(keywordResponses, pagedKeywords.TotalCount, pagedKeywords.PageIndex, request.PageSize);
        }

        public async Task<KeywordResponse> UpdateKeywordAsync(Guid id, UpdateKeywordRequest request)
        {
            var keyword = await _unitOfWork.Repository<Keyword>().GetByIdGuid(id);
            if (keyword == null)
                throw new KeyNotFoundException($"Keyword with ID {id} not found");

            _mapper.Map(request, keyword);
            _unitOfWork.Repository<Keyword>().UpdateDetached(keyword);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<KeywordResponse>(keyword);
        }
    }
} 