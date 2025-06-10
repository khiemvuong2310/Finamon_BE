using Finamon.Service.ReponseModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Finamon.Service.Interfaces
{
    public interface IUserActivityService
    {
        Task LogUserActivityAsync(int userId, int useTimeInMinutes);
        Task<IEnumerable<UserActivityResponse>> GetUserActivitiesAsync(int userId);
        Task<UserActivityStatsResponse> GetUserActivityStatsAsync(int userId);
    }
} 