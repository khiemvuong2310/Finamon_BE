using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IBudgetAlertService
    {
        Task<IEnumerable<BudgetAlertResponse>> GetAllAlertsAsync(BudgetAlertQueryRequest query);
        Task<BudgetAlertResponse> GetAlertByIdAsync(int id);
        Task<IEnumerable<BudgetAlertResponse>> GetAlertsByBudgetIdAsync(int budgetId);
        Task<BudgetAlertResponse> CreateAlertAsync(BudgetAlertRequestModel request);
        Task<BudgetAlertResponse> UpdateAlertAsync(int id, BudgetAlertRequestModel request);
        Task DeleteAlertAsync(int id);
    }
} 