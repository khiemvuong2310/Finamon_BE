using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;

namespace Finamon.Service.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseResponse> CreateExpenseAsync(ExpenseRequest request, int userId);
        Task<ExpenseResponse> UpdateExpenseAsync(int id, ExpenseUpdateRequest request);
        Task<bool> DeleteExpenseAsync(int id);
        Task<ExpenseResponse> GetExpenseByIdAsync(int id);
        Task<ExpenseDetailResponse> GetExpenseDetailByIdAsync(int id);
        Task<List<ExpenseResponse>> GetAllExpensesAsync();
        Task<List<ExpenseResponse>> GetExpensesByUserIdAsync(int userId);
        Task<(List<ExpenseResponse> Expenses, int TotalCount)> GetExpensesByFilterAsync(ExpenseQueryRequest query);
    }
} 