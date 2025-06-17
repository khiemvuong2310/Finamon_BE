using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Microsoft.EntityFrameworkCore;
using Finamon.Service.RequestModel.QueryRequest;

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
            // Validate user exists and is not deleted
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDelete);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found or has been deleted");

            // Validate category exists and is not deleted
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDelete);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");

            var expense = _mapper.Map<Expense>(request);
            expense.UserId = userId;
            expense.IsDelete = false;
            expense.Date = DateTime.UtcNow.AddHours(7);
            expense.UpdateDate = DateTime.UtcNow.AddHours(7);

            await _context.Expenses.AddAsync(expense);
            await _context.SaveChangesAsync();

            return await GetExpenseByIdAsync(expense.Id);
        }

        public async Task<ExpenseResponse> UpdateExpenseAsync(int id, ExpenseUpdateRequest request)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDelete);

            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found or has been deleted");

            // Validate category if it's being updated
            if (request.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == request.CategoryId.Value && !c.IsDelete);
                if (!categoryExists)
                    throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found or has been deleted");
            }

            _mapper.Map(request, expense);
            expense.UpdateDate = DateTime.UtcNow.AddHours(7);

            await _context.SaveChangesAsync();
            return await GetExpenseByIdAsync(expense.Id);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                throw new KeyNotFoundException($"Expense with ID {id} not found");
            }

            expense.IsDelete = true;
            expense.UpdateDate = DateTime.UtcNow.AddHours(7);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ExpenseResponse> GetExpenseByIdAsync(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDelete);

            if (expense == null)
                throw new KeyNotFoundException($"Expense with ID {id} not found or has been deleted");

            return _mapper.Map<ExpenseResponse>(expense);
        }

        public async Task<ExpenseDetailResponse> GetExpenseDetailByIdAsync(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDelete);

            if (expense == null)
            {
                throw new Exception("Expense not found");
            }

            return _mapper.Map<ExpenseDetailResponse>(expense);
        }

        public async Task<List<ExpenseResponse>> GetAllExpensesAsync()
        {
            var queryable = _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .AsQueryable();

            queryable = queryable.Where(e => !e.IsDelete);
            queryable = queryable.OrderByDescending(e => e.Id);

            var expenses = await queryable.ToListAsync();
            return _mapper.Map<List<ExpenseResponse>>(expenses);
        }

        public async Task<List<ExpenseResponse>> GetExpensesByUserIdAsync(int userId)
        {
            var expenses = await _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && !e.IsDelete)
                .ToListAsync();

            return _mapper.Map<List<ExpenseResponse>>(expenses);
        }

        public async Task<PaginatedResponse<ExpenseResponse>> GetExpensesByFilterAsync(ExpenseQueryRequest queryRequest)
        {
            var queryable = _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(queryRequest.Name))
            {
                queryable = queryable.Where(e => e.Name.Contains(queryRequest.Name));
            }

            if (!string.IsNullOrEmpty(queryRequest.Description))
            {
                queryable = queryable.Where(e => e.Description.Contains(queryRequest.Description));
            }

            if (queryRequest.MinAmount.HasValue)
            {
                queryable = queryable.Where(e => e.Amount >= queryRequest.MinAmount.Value);
            }

            if (queryRequest.MaxAmount.HasValue)
            {
                queryable = queryable.Where(e => e.Amount <= queryRequest.MaxAmount.Value);
            }

            if (queryRequest.StartDate.HasValue)
            {
                queryable = queryable.Where(e => e.Date >= queryRequest.StartDate.Value);
            }

            if (queryRequest.EndDate.HasValue)
            {
                queryable = queryable.Where(e => e.Date <= queryRequest.EndDate.Value);
            }

            if (queryRequest.CategoryId.HasValue)
            {
                queryable = queryable.Where(e => e.CategoryId == queryRequest.CategoryId.Value);
            }

            // Apply soft delete filter
            if (queryRequest.IsDeleted.HasValue)
            {
                queryable = queryable.Where(e => e.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                queryable = queryable.Where(e => !e.IsDelete);
            }

            // Apply sorting
            if (queryRequest.SortBy.HasValue)
            {
                queryable = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(e => e.Date)
                        : queryable.OrderBy(e => e.Date),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(e => e.UpdateDate)
                        : queryable.OrderBy(e => e.UpdateDate),
                    SortByEnum.Amount => queryRequest.SortDescending
                        ? queryable.OrderByDescending(e => e.Amount)
                        : queryable.OrderBy(e => e.Amount),
                    _ => queryable.OrderByDescending(e => e.Id) // Default sort
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(e => e.Id); // Default sort
            }

            var paginatedExpenses = await PaginatedResponse<Expense>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            var expenseResponses = _mapper.Map<List<ExpenseResponse>>(paginatedExpenses.Items);

            return new PaginatedResponse<ExpenseResponse>(expenseResponses, paginatedExpenses.TotalCount, paginatedExpenses.PageIndex, queryRequest.PageSize);
        }

        public async Task<PaginatedResponse<ExpenseResponse>> GetExpenseByUserIdAsync(int userId, ExpenseQueryRequest queryRequest)
        {
            // Check if user exists and is not deleted
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId && !u.IsDelete);
            if (!userExists)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var queryable = _context.Expenses
                .Include(e => e.User)
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(queryRequest.Name))
            {
                queryable = queryable.Where(e => e.Name.Contains(queryRequest.Name));
            }

            if (!string.IsNullOrEmpty(queryRequest.Description))
            {
                queryable = queryable.Where(e => e.Description.Contains(queryRequest.Description));
            }

            if (queryRequest.MinAmount.HasValue)
            {
                queryable = queryable.Where(e => e.Amount >= queryRequest.MinAmount.Value);
            }

            if (queryRequest.MaxAmount.HasValue)
            {
                queryable = queryable.Where(e => e.Amount <= queryRequest.MaxAmount.Value);
            }

            if (queryRequest.StartDate.HasValue)
            {
                queryable = queryable.Where(e => e.Date >= queryRequest.StartDate.Value);
            }

            if (queryRequest.EndDate.HasValue)
            {
                queryable = queryable.Where(e => e.Date <= queryRequest.EndDate.Value);
            }

            if (queryRequest.CategoryId.HasValue)
            {
                queryable = queryable.Where(e => e.CategoryId == queryRequest.CategoryId.Value);
            }

            // Apply soft delete filter
            if (queryRequest.IsDeleted.HasValue)
            {
                queryable = queryable.Where(e => e.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                queryable = queryable.Where(e => !e.IsDelete);
            }

            // Apply sorting
            if (queryRequest.SortBy.HasValue)
            {
                queryable = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(e => e.Date)
                        : queryable.OrderBy(e => e.Date),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(e => e.UpdateDate)
                        : queryable.OrderBy(e => e.UpdateDate),
                    SortByEnum.Amount => queryRequest.SortDescending
                        ? queryable.OrderByDescending(e => e.Amount)
                        : queryable.OrderBy(e => e.Amount),
                    _ => queryable.OrderByDescending(e => e.Id) // Default sort
                };
            }
            else
            {
                queryable = queryable.OrderByDescending(e => e.Date); // Default sort by date
            }

            var paginatedExpenses = await PaginatedResponse<Expense>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            var expenseResponses = _mapper.Map<List<ExpenseResponse>>(paginatedExpenses.Items);

            return new PaginatedResponse<ExpenseResponse>(expenseResponses, paginatedExpenses.TotalCount, paginatedExpenses.PageIndex, queryRequest.PageSize);
        }
    }
} 