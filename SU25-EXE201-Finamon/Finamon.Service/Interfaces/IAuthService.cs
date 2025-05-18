using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;

namespace Finamon.Service.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<TokenModel>> AdminGenAcc(AdminCreateAccountModel adminCreateAccountModel);
        Task<BaseResponseForLogin<LoginResponseModel>> AuthenticateAsync(string email, string password);
        Task<BaseResponse> ForgotPassword(ForgotPasswordRequest request);
        string GenerateJwtToken(string username, string roleName, int userId);
        string GeneratePassword();
        Task<BaseResponse<TokenModel>> RefreshTokenAsync(string refreshToken);
        Task<BaseResponse<TokenModel>> RegisterAsync(RegisterRequestModel registerModel);
        Task<BaseResponse> SendAccount(int userId);
        Task<BaseResponse> SendVerificationEmailAsync(string email);
        Task<BaseResponse> SetEmailVerified(string email);
        Task<BaseResponse<TokenModel>> VerifyAccountAsync(EmailVerificationModel model);
    }
}