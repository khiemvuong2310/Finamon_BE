using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponseForLogin<LoginResponseModel>>> Login([FromBody] LoginModel model)
        {
            var response = await _authService.AuthenticateAsync(model.Email, model.Password);
            return Ok(response);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<TokenModel>>> Register([FromBody] RegisterRequestModel model)
        {
            var response = await _authService.RegisterAsync(model);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<TokenModel>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPassword(request);
            return Ok(response);
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<TokenModel>>> VerifyEmail([FromBody] EmailVerificationModel model)
        {
            var response = await _authService.VerifyAccountAsync(model);
            return Ok(response);
        }

        [HttpPost("send-verification-email")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse>> SendVerificationEmail([FromBody] string email)
        {
            var response = await _authService.SendVerificationEmailAsync(email);
            return Ok(response);
        }

        [HttpPost("admin/create-account")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<TokenModel>>> AdminCreateAccount([FromBody] AdminCreateAccountModel model)
        {
            var response = await _authService.AdminGenAcc(model);
            return Ok(response);
        }

        [HttpPost("admin/send-account/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse>> SendAccount(int userId)
        {
            var response = await _authService.SendAccount(userId);
            return Ok(response);
        }

        [HttpPost("set-email-verified")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse>> SetEmailVerified([FromBody] string email)
        {
            var response = await _authService.SetEmailVerified(email);
            return Ok(response);
        }
    }
} 