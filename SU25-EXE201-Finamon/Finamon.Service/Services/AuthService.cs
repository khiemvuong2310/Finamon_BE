using Finamon.Repo.UnitOfWork;
using Finamon.Repo.Utils;
using Finamon.Service.Interfaces;
using Finamon_Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Finamon.Service.RequestModel;
using Finamon.Service.ReponseModel;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace Finamon.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailTemplateService _emailTemplateService;
        private const int TOKEN_EXPIRY_HOURS = 24;
        private const int TOKEN_Mobile_EXPIRY_HOURS = 336;
        private const int REFRESH_TOKEN_EXPIRY_DAYS = 7;
        private const string EMAIL_SENDER = "studentexchangeweb@gmail.com";
        private const string EMAIL_PASSWORD = "fwpl wpkw zhqe peyh";
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new();

        public AuthService(
            IConfiguration configuration,
            IUserService userService,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IEmailTemplateService emailTemplateService
            )
        {
            _configuration = configuration;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<BaseResponseForLogin<LoginResponseModel>> AuthenticateAsync(string email, string password, bool? mobile = false)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return new BaseResponseForLogin<LoginResponseModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Email and password are required",
                        Data = null,
                        IsBanned = false
                    };
                }

                var user = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .Where(u => u.Email == email && !u.IsDelete)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new BaseResponseForLogin<LoginResponseModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid email or password",
                        Data = null,
                        IsBanned = false
                    };
                }

                var isCorrect = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (!isCorrect)
                {
                    return new BaseResponseForLogin<LoginResponseModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid email or password",
                        Data = null,
                        IsBanned = false
                    };
                }

                // Check if account is banned
                if (user.Status == false)
                {
                    var bannedUserDetails = await _userService.GetUserByEmailAsync(email);
                    return new BaseResponseForLogin<LoginResponseModel>
                    {
                        Code = StatusCodes.Status403Forbidden,
                        Message = "Your account has been banned. Please check your email for the reason",
                        Data = new LoginResponseModel
                        {
                            User = bannedUserDetails,
                            EmailVerified = true
                        },
                        IsBanned = true
                    };
                }

                // Check if email is verified
                if (!user.EmailVerified)
                {
                    // Generate new verification code and send email
                    var verificationCode = GenerateVerificationCode();
                    _verificationCodes[user.Email] = (verificationCode, DateTime.UtcNow.AddMinutes(30));

                    await SendEmailAsync(
                        user.Email,
                        "Email Verification",
                        await GetVerificationEmailTemplate(verificationCode)
                    );

                    var unverifiedUserDetails = await _userService.GetUserByEmailAsync(email);
                    return new BaseResponseForLogin<LoginResponseModel>
                    {
                        Code = StatusCodes.Status403Forbidden,
                        Message = "Please verify your email before logging in. A new verification code has been sent to your email.",
                        Data = new LoginResponseModel
                        {
                            User = unverifiedUserDetails,
                            EmailVerified = false
                        },
                        IsBanned = false,
                        RequiresVerification = true
                    };
                }

                var userDetails = await _userService.GetUserByEmailAsync(email);
                var userWithRole = await _userService.GetUserByEmailAsync(userDetails.Email);
                var roleNames = userWithRole.UserRoles.Select(ur => ur.RoleName).ToList();

                string token = GenerateJwtToken(userDetails.Email, string.Join(",", roleNames), userDetails.Id, mobile);
                string refreshToken = GenerateRefreshToken();

                // Store refresh token in user record
                user.Token = refreshToken;
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponseForLogin<LoginResponseModel>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Login successful",
                    Data = new LoginResponseModel
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        User = userDetails,
                        EmailVerified = true
                    },
                    IsBanned = false
                };
            }
            catch (Exception ex)
            {
                return new BaseResponseForLogin<LoginResponseModel>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred during authentication: " + ex.Message,
                    Data = null,
                    IsBanned = false
                };
            }
        }

        public string GenerateJwtToken(string email, string roleNames, int userId, bool? mobile)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            // Tạo danh sách claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            // Thêm từng role riêng biệt
            if (!string.IsNullOrEmpty(roleNames))
            {
                foreach (var role in roleNames.Split(',').Select(r => r.Trim()))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(mobile == true ? TOKEN_Mobile_EXPIRY_HOURS : TOKEN_EXPIRY_HOURS),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<BaseResponse<TokenModel>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .Where(u => u.Token == refreshToken && !u.IsDelete)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = "Invalid refresh token",
                    };
                }

                var userDetails = await _userService.GetUserByIdAsync(user.Id);
                var roleNames = userDetails.UserRoles.Select(ur => ur.RoleName).ToList();

                string newToken = GenerateJwtToken(userDetails.Email, string.Join(",", roleNames), userDetails.Id, false);
                string newRefreshToken = GenerateRefreshToken();

                // Update refresh token in database
                user.Token = newRefreshToken;
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Token refreshed successfully",
                    Data = new TokenModel
                    {
                        Token = newToken,
                        RefreshToken = newRefreshToken
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message,
                };
            }
        }

        public async Task<BaseResponse<TokenModel>> RegisterAsync(RegisterRequestModel registerModel)
        {
            try
            {
                var existingUser = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .Where(u => u.Email == registerModel.Email && !u.IsDelete)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = "Email already exists",
                    };
                }

                var user = new User()
                {
                    //Location = registerModel.Location,
                    //UserName = "Customer",
                    Email = registerModel.Email,
                    Password = PasswordTools.HashPassword(registerModel.Password),
                    //Phone = registerModel.Phone,
                    Status = true,
                    EmailVerified = false // Set EmailVerified to false by default
                };

                await _unitOfWork.Repository<User>().InsertAsync(user);
                await _unitOfWork.CommitAsync();

                // Assign default role (Customer)
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = 3, 
                    Status = true
                };


                await _unitOfWork.Repository<UserRole>().InsertAsync(userRole);
                await _unitOfWork.SaveChangesAsync();


                // Generate verification code and send email
                var verificationCode = GenerateVerificationCode();
                _verificationCodes[user.Email] = (verificationCode, DateTime.UtcNow.AddMinutes(30)); // Valid for 30 minutes

                await SendEmailAsync(
                    user.Email,
                    "Email Verification",
                    await GetVerificationEmailTemplate(verificationCode)
                );

                // Do not return a token until email is verified
                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Registration successful. Please check your email for verification code.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Registration failed: " + ex.Message,
                };
            }
        }

        public async Task<BaseResponse<TokenModel>> AdminGenAcc(AdminCreateAccountModel adminCreateAccountModel)
        {
            try
            {
                // Kiểm tra user đã tồn tại chưa
                var existingUser = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .Where(u => u.Email == adminCreateAccountModel.Email && !u.IsDelete)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = "Email already exists",
                    };
                }

                // Tạo user mới
                var user = new User()
                {
                    Location = adminCreateAccountModel.Location,
                    UserName = adminCreateAccountModel.Username,
                    Email = adminCreateAccountModel.Email,
                    Password = PasswordTools.HashPassword("12345678"),
                    Phone = adminCreateAccountModel.Phone,
                    Status = true,
                    EmailVerified = true,
                };

                // Thêm user vào database
                await _unitOfWork.Repository<User>().InsertAsync(user);
                await _unitOfWork.CommitAsync();

                // Lấy Id của user vừa tạo
                int userId = user.Id;

                // Tạo UserRole
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = 1, // Admin role ID
                    Status = true
                };

                // Thêm UserRole vào database
                await _unitOfWork.Repository<UserRole>().InsertAsync(userRole);
                await _unitOfWork.CommitAsync();

                // Gửi email trực tiếp
                await SendEmailAsync(
                    user.Email,
                    "YOUR ENTRY ACCOUNT",
                    await GetAccountEmailTemplate(user.Email, "12345678")
                );

                // Lấy thông tin user và tạo token
                var userWithRole = await _userService.GetUserByEmailAsync(user.Email);
                var roleNames = string.Join(",", userWithRole.UserRoles.Select(ur => ur.RoleName));
                string token = GenerateJwtToken(user.Email, roleNames, userId, false);
                string refreshToken = GenerateRefreshToken();

                // Save refresh token
                user.Token = refreshToken;
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status201Created,
                    Message = "Account created successfully",
                    Data = new TokenModel
                    {
                        Token = token,
                        RefreshToken = refreshToken
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Account creation failed: " + ex.Message,
                };
            }
        }

        public async Task<BaseResponse> SendAccount(int userId)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>().GetById(userId);
                if (user == null)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "User not found"
                    };
                }

                var providePassword = GeneratePassword();
                user.Password = PasswordTools.HashPassword(providePassword);
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                await SendEmailAsync(
                    user.Email,
                    "YOUR ENTRY ACCOUNT",
                    await GetAccountEmailTemplate(user.Email, providePassword)
                );

                return new BaseResponse
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Account credentials sent successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse> ForgotPassword(ForgotPasswordRequest request)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email == request.Email && !x.IsDelete);

                if (user == null)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Email is not matched"
                    };
                }

                var providePassword = GeneratePassword();
                user.Password = PasswordTools.HashPassword(providePassword);

                await SendEmailAsync(
                    user.Email,
                    "WEB EXCHANGE: YOUR RESET PASSWORD",
                    await GetPasswordResetEmailTemplate(user.Email, providePassword)
                );

                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Your reset password has been sent."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred: " + ex.Message
                };
            }
        }

        public string GeneratePassword()
        {
            const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string numericChars = "0123456789";
            const string specialChars = "!@#$%^&*()";

            var allChars = uppercaseChars + lowercaseChars + numericChars + specialChars;
            var random = new Random();

            // Ensure at least one character from each category
            var password = new StringBuilder();
            password.Append(uppercaseChars[random.Next(uppercaseChars.Length)]);
            password.Append(lowercaseChars[random.Next(lowercaseChars.Length)]);
            password.Append(numericChars[random.Next(numericChars.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Fill the rest of the password
            for (int i = 4; i < 12; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            return new string(password.ToString().OrderBy(x => random.Next()).ToArray());
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(EMAIL_SENDER, EMAIL_PASSWORD)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(EMAIL_SENDER),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task<string> GetAccountEmailTemplate(string email, string password)
        {
            return await _emailTemplateService.GetAccountEmailTemplateAsync(email, password);
        }

        private async Task<string> GetPasswordResetEmailTemplate(string email, string password)
        {
            return await _emailTemplateService.GetPasswordResetTemplateAsync(email, password);
        }

        private async Task<string> GetVerificationEmailTemplate(string verificationCode)
        {
            return await _emailTemplateService.GetVerificationEmailTemplateAsync(verificationCode);
        }

        public async Task<BaseResponse> SendVerificationEmailAsync(string email)
        {
            try
            {
                var existingUser = await _unitOfWork.Repository<User>().FindAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = "Email already exists"
                    };
                }

                var verificationCode = GenerateVerificationCode();
                _verificationCodes[email] = (verificationCode, DateTime.UtcNow.AddMinutes(15));

                await SendEmailAsync(
                    email,
                    "Email Verification",
                    await GetVerificationEmailTemplate(verificationCode)
                );

                return new BaseResponse
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Verification code sent successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to send verification code: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse<TokenModel>> VerifyAccountAsync(EmailVerificationModel model)
        {
            try
            {
                if (!_verificationCodes.TryGetValue(model.Email, out var verificationData))
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "No verification code found for this email"
                    };
                }

                if (string.IsNullOrWhiteSpace(verificationData.Code) || string.IsNullOrWhiteSpace(model.VerificationCode))
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Verification code is missing or invalid"
                    };
                }
                //Check verification code bỏ qua chữ hoa và thường
                if (!verificationData.Code.Trim().ToUpper().Equals(model.VerificationCode.Trim().ToUpper()))
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Invalid verification code"
                    };
                }
                //Check DateTime 
                if (DateTime.UtcNow > verificationData.Expiry)
                {
                    _verificationCodes.Remove(model.Email);
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Verification code has expired"
                    };
                }

                _verificationCodes.Remove(model.Email);

                var user = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .Where(u => u.Email == model.Email && !u.IsDelete)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new BaseResponse<TokenModel>
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "User not found"
                    };
                }

                user.EmailVerified = true;
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                var userDetails = await _userService.GetUserByEmailAsync(user.Email);
                var roleNames = userDetails.UserRoles.Select(r => r.RoleName).ToList();

                var token = GenerateJwtToken(userDetails.Email, string.Join(",", roleNames), userDetails.Id, false);
                var refreshToken = GenerateRefreshToken();

                user.Token = refreshToken;
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Email verification successful. You can now log in.",
                    Data = new TokenModel
                    {
                        Token = token,
                        RefreshToken = refreshToken
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<TokenModel>
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Verification failed: " + ex.Message
                };
            }
        }


        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<BaseResponse> SetEmailVerified(string email)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .Where(u => u.Email == email && !u.IsDelete)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "User not found"
                    };
                }

                user.EmailVerified = true;
                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                return new BaseResponse
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Email verified successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "Failed to verify email: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>()
                    .AsQueryable()
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDelete);

                if (user == null)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status404NotFound,
                        Message = "User not found"
                    };
                }

                // Verify current password
                var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);
                if (!isCurrentPasswordValid)
                {
                    return new BaseResponse
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Current password is incorrect"
                    };
                }

                // Hash and save new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedDate = DateTime.UtcNow.AddHours(7);

                await _unitOfWork.Repository<User>().Update(user, user.Id);
                await _unitOfWork.CommitAsync();

                // Send email notification
                await SendEmailAsync(
                    user.Email,
                    "Password Changed Successfully",
                    "Your password has been changed successfully. If you did not make this change, please contact support immediately."
                );

                return new BaseResponse
                {
                    Code = StatusCodes.Status200OK,
                    Message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = StatusCodes.Status500InternalServerError,
                    Message = "An error occurred while changing password: " + ex.Message
                };
            }
        }
    }
}
