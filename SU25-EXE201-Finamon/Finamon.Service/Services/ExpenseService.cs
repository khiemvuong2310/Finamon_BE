using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Microsoft.EntityFrameworkCore;

namespace Finamon.Service.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ExpenseService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ExpenseResponse> CreateExpenseAsync(ExpenseRequest request, int userId)
        {
            var expense = _mapper.Map<Expense>(request);
            expense.UserId = userId;
            expense.IsDelete = false;

            await _context.Expenses.AddAsync(expense);
            await _context.SaveChangesAsync();

            return await GetExpenseByIdAsync(expense.Id);
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(int id, ExpenseUpdateRequest request)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDelete);

            if (expense == null)
            {
                throw new Exception("Expense not found");
            }

            _mapper.Map(request, expense);

            await _context.SaveChangesAsync();
            return await GetExpenseByIdAsync(expense.Id);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return false;
            }

            expense.IsDelete = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ExpenseResponse> GetExpenseByIdAsync(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDelete);

            if (expense == null)
            {
                throw new Exception("Expense not found");
            }

            return _mapper.Map<ExpenseResponse>(expense);
        }

        public async Task<ExpenseDetailResponse> GetExpenseDetailByIdAsync(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDelete);

            if (expense == null)
            {
                throw new Exception("Expense not found");
            }

            return _mapper.Map<ExpenseDetailResponse>(expense);
        }

        public async Task<List<ExpenseResponse>> GetAllExpensesAsync()
        {
            // Start with base query including related entities
            var queryable = _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Budget)
                .AsQueryable();

            // Apply soft delete filter to exclude deleted expenses
            queryable = queryable.Where(e => !e.IsDelete);

            // Apply sorting by Id in descending order (newest first)
            queryable = queryable.OrderByDescending(e => e.Id);

            // Execute the query and get results
            var expenses = await queryable.ToListAsync();

            // Map the results to ExpenseResponse DTOs
            return _mapper.Map<List<ExpenseResponse>>(expenses);
        }

        public async Task<List<ExpenseResponse>> GetExpensesByUserIdAsync(int userId)
        {
            var expenses = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Budget)
                .Where(e => e.UserId == userId && !e.IsDelete)
                .ToListAsync();

            return _mapper.Map<List<ExpenseResponse>>(expenses);
        }

        public async Task<(List<ExpenseResponse> Expenses, int TotalCount)> GetExpensesByFilterAsync(ExpenseQueryRequest query)
        {
            var queryable = _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Include(e => e.Budget)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.Description))
            {
                queryable = queryable.Where(e => e.Description.Contains(query.Description));
            }

            if (query.MinAmount.HasValue)
            {
                queryable = queryable.Where(e => e.Amount >= query.MinAmount.Value);
            }

            if (query.MaxAmount.HasValue)
            {
                queryable = queryable.Where(e => e.Amount <= query.MaxAmount.Value);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(e => e.Date >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(e => e.Date <= query.EndDate.Value);
            }

            if (query.CategoryId.HasValue)
            {
                queryable = queryable.Where(e => e.CategoryId == query.CategoryId.Value);
            }

            if (query.BudgetId.HasValue)
            {
                queryable = queryable.Where(e => e.BudgetId == query.BudgetId.Value);
            }

            // Apply soft delete filter
            queryable = queryable.Where(e => !e.IsDelete);

            // Apply sorting
            if (!string.IsNullOrEmpty(query.Sort))
            {
                switch (query.Sort.ToLower())
                {
                    case "date":
                        queryable = queryable.OrderBy(e => e.Date);
                        break;
                    case "amount":
                        queryable = queryable.OrderBy(e => e.Amount);
                        break;
                    case "date_desc":
                        queryable = queryable.OrderByDescending(e => e.Date);
                        break;
                    case "amount_desc":
                        queryable = queryable.OrderByDescending(e => e.Amount);
                        break;
                    default:
                        queryable = queryable.OrderByDescending(e => e.Id);
                        break;
                }
            }
            else
            {
                queryable = queryable.OrderByDescending(e => e.Id);
            }

            // Get total count
            var totalCount = await queryable.CountAsync();

            // Apply pagination
            var expenses = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (_mapper.Map<List<ExpenseResponse>>(expenses), totalCount);
        }
    }
} 