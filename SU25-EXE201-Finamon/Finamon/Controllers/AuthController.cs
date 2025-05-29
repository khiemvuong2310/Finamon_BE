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
        public async Task<ActionResult<BaseResponseForLogin<LoginResponseModel>>> Login([FromForm] LoginModel model)
        {
            var response = await _authService.AuthenticateAsync(model.Email, model.Password, model.Mobile);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponseForLogin<LoginResponseModel>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<TokenModel>>> Register([FromForm] RegisterRequestModel model)
        {
            var response = await _authService.RegisterAsync(model);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<TokenModel>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<TokenModel>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<TokenModel>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPassword(request);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<TokenModel>>> VerifyEmail([FromBody] EmailVerificationModel model)
        {
            var response = await _authService.VerifyAccountAsync(model);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<TokenModel>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("send-verification-email")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse>> SendVerificationEmail([FromBody] string email)
        {
            var response = await _authService.SendVerificationEmailAsync(email);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("admin/create-account")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<TokenModel>>> AdminCreateAccount([FromBody] AdminCreateAccountModel model)
        {
            var response = await _authService.AdminGenAcc(model);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse<TokenModel>
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("admin/send-account/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse>> SendAccount(int userId)
        {
            var response = await _authService.SendAccount(userId);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }

        [HttpPost("set-email-verified")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse>> SetEmailVerified([FromBody] string email)
        {
            var response = await _authService.SetEmailVerified(email);
            if (response.Code.HasValue)
            {
                return StatusCode(response.Code.Value, response);
            }
            return StatusCode(500, new BaseResponse
            {
                Code = 500,
                Message = "Unexpected error occurred."
            });
        }
    }
} 