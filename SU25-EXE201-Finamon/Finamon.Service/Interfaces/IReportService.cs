using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportResponse>> GetAllReportsAsync(ReportQueryRequest query);
        Task<ReportResponse> GetReportByIdAsync(int id);
        Task<ReportResponse> CreateReportAsync(ReportRequestModel request);
        Task<ReportResponse> UpdateReportAsync(int id, ReportRequestModel request);
        Task DeleteReportAsync(int id);
    }
} 