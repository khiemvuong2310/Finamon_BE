using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.RequestModel;
using Finamon.Service.RequestModel.QueryRequest;
using Finamon.Service.ReponseModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace Finamon.Service.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFirebaseStorageService _firebaseStorageService;

        public UserService(
            AppDbContext context, 
            IMapper mapper, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            IFirebaseStorageService firebaseStorageService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _firebaseStorageService = firebaseStorageService;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<UserResponse> CreateUserAsync(UserRequest request)
        {
            var existingUser = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email || u.UserName == request.UserName);

            if (existingUser != null)
            {
                throw new Exception("User with this email or username already exists");
            }

            var user = _mapper.Map<User>(request);
            
            // Handle image upload if provided
            if (request.ImageFile != null)
            {
                try
                {
                    var imageUrl = await _firebaseStorageService.UploadImageAsync(request.ImageFile);
                    user.Image = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to upload image: {ex.Message}");
                }
            }

            user.Password = HashPassword(request.Password);
            user.CreatedDate = DateTime.Now;
            user.UpdatedDate = DateTime.Now;
            user.Status = true;
            user.EmailVerified = true;

            await _context.Set<User>().AddAsync(user);
            await _context.SaveChangesAsync();

            // Tạo UserRole mặc định là Customer (RoleId = 3)
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 3, // Customer role
                Status = true,
                CreatedDate = DateTime.Now
            };

            await _context.Set<UserRole>().AddAsync(userRole);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id);
        }

        public async Task<UserResponse> UpdateUserAsync(int id, UserUpdateRequest request)
        {
            var user = await _context.Set<User>()
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Handle image upload if provided
            if (request.ImageFile != null)
            {
                try
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.Image))
                    {
                        await _firebaseStorageService.DeleteImageAsync(user.Image);
                    }

                    // Upload new image
                    var imageUrl = await _firebaseStorageService.UploadImageAsync(request.ImageFile);
                    request.Image = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to upload image: {ex.Message}");
                }
            }

            _mapper.Map(request, user);
            user.UpdatedDate = DateTime.Now;

            // Cập nhật Role nếu có
            if (request.RoleId.HasValue)
            {
                var existingUserRole = user.UserRoles.FirstOrDefault();
                if (existingUserRole != null)
                {
                    existingUserRole.RoleId = request.RoleId.Value;
                    existingUserRole.Status = true;
                }
                else
                {
                    var newUserRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = request.RoleId.Value,
                        Status = true,
                        CreatedDate = DateTime.Now
                    };
                    await _context.Set<UserRole>().AddAsync(newUserRole);
                }
            }

            await _context.SaveChangesAsync();
            return await GetUserByIdAsync(user.Id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Set<User>()
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            user.IsDelete = true;
            user.UpdatedDate = DateTime.Now;

            // Cập nhật trạng thái của UserRole
            foreach (var userRole in user.UserRoles)
            {
                userRole.Status = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserResponse> GetUserByIdAsync(int id)
        {
            // Get the current user's ID from the claims
            var currentUserIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            // Parse the current user's ID
            if (!int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }

            // Check if the requested ID matches the current user's ID
            if (currentUserId != id)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this user's information");
            }

            var user = await _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            var activeRole = user.UserRoles.FirstOrDefault(ur => ur.Status)?.Role;
            if (activeRole != null)
            {
                userResponse.RoleId = activeRole.Id;
                userResponse.RoleName = activeRole.Name;
            }

            return userResponse;
        }

        public async Task<UserDetailResponse> GetUserDetailByIdAsync(int id)
        {
            var user = await _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Expenses)
                .Include(u => u.Reports)
                .Include(u => u.ChatSessions)
                .Include(u => u.UserMemberships)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var userDetail = _mapper.Map<UserDetailResponse>(user);
            var activeRole = user.UserRoles.FirstOrDefault(ur => ur.Status)?.Role;
            if (activeRole != null)
            {
                userDetail.RoleId = activeRole.Id;
                userDetail.RoleName = activeRole.Name;
            }

            // Tính toán các thống kê
            userDetail.TotalExpenses = user.Expenses?.Count ?? 0;
            userDetail.TotalExpenseAmount = user.Expenses?.Sum(e => e.Amount) ?? 0;
            userDetail.TotalReports = user.Reports?.Count ?? 0;
            userDetail.TotalChatSessions = user.ChatSessions?.Count ?? 0;
            userDetail.HasActiveMembership = user.UserMemberships?.Any(m => m.EndDate > DateTime.Now) ?? false;
            return userDetail;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDelete)
                .ToListAsync();

            var userResponses = new List<UserResponse>();
            foreach (var user in users)
            {
                var userResponse = _mapper.Map<UserResponse>(user);
                var activeRole = user.UserRoles.FirstOrDefault(ur => ur.Status)?.Role;
                if (activeRole != null)
                {
                    userResponse.RoleId = activeRole.Id;
                    userResponse.RoleName = activeRole.Name;
                }
                userResponses.Add(userResponse);
            }

            return userResponses;
        }

        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            var user = await _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDelete);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            var activeRole = user.UserRoles.FirstOrDefault(ur => ur.Status)?.Role;
            if (activeRole != null)
            {
                userResponse.RoleId = activeRole.Id;
                userResponse.RoleName = activeRole.Name;
            }

            return userResponse;
        }

        public async Task<UserResponse> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == username && !u.IsDelete);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var userResponse = _mapper.Map<UserResponse>(user);
            var activeRole = user.UserRoles.FirstOrDefault(ur => ur.Status)?.Role;
            if (activeRole != null)
            {
                userResponse.RoleId = activeRole.Id;
                userResponse.RoleName = activeRole.Name;
            }

            return userResponse;
        }

        public async Task<PaginatedResponse<UserResponse>> GetUsersByFilterAsync(UserQueryRequest queryRequest)
        {
            var queryable = _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(queryRequest.Username))
            {
                queryable = queryable.Where(u => u.UserName.Contains(queryRequest.Username));
            }

            if (!string.IsNullOrEmpty(queryRequest.Email))
            {
                queryable = queryable.Where(u => u.Email.Contains(queryRequest.Email));
            }

            if (!string.IsNullOrEmpty(queryRequest.Phone))
            {
                queryable = queryable.Where(u => u.Phone != null && u.Phone.Contains(queryRequest.Phone));
            }

            if (!string.IsNullOrEmpty(queryRequest.Location))
            {
                queryable = queryable.Where(u => u.Location != null && u.Location.Contains(queryRequest.Location));
            }

            if (queryRequest.Status.HasValue)
            {
                queryable = queryable.Where(u => u.Status == queryRequest.Status.Value);
            }

            if (queryRequest.RoleId.HasValue)
            {
                queryable = queryable.Where(u => u.UserRoles.Any(ur => ur.RoleId == queryRequest.RoleId.Value && ur.Status));
            }

            if (queryRequest.EmailVerified.HasValue)
            {
                queryable = queryable.Where(u => u.EmailVerified == queryRequest.EmailVerified.Value);
            }

            // Xử lý soft delete
            if (queryRequest.IsDeleted.HasValue)
            {
                queryable = queryable.Where(u => u.IsDelete == queryRequest.IsDeleted.Value);
            }
            else
            {
                queryable = queryable.Where(u => !u.IsDelete);
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(queryRequest.SortBy))
            {
                switch (queryRequest.SortBy.ToLower())
                {
                    case "username":
                        queryable = queryRequest.SortDescending ? queryable.OrderByDescending(u => u.UserName) : queryable.OrderBy(u => u.UserName);
                        break;
                    case "email":
                        queryable = queryRequest.SortDescending ? queryable.OrderByDescending(u => u.Email) : queryable.OrderBy(u => u.Email);
                        break;
                    case "createddate":
                        queryable = queryRequest.SortDescending ? queryable.OrderByDescending(u => u.CreatedDate) : queryable.OrderBy(u => u.CreatedDate);
                        break;
                    default:
                        queryable = queryRequest.SortDescending ? queryable.OrderByDescending(u => u.CreatedDate) : queryable.OrderBy(u => u.CreatedDate);
                        break;
                }
            }
            else
            {
                queryable = queryable.OrderByDescending(u => u.CreatedDate); // Default sort
            }

            var paginatedUsers = await PaginatedResponse<User>.CreateAsync(queryable, queryRequest.PageNumber, queryRequest.PageSize);
            
            var userResponses = new List<UserResponse>();
            foreach (var user in paginatedUsers.Items)
            {
                var userResponse = _mapper.Map<UserResponse>(user);
                var activeRole = user.UserRoles.FirstOrDefault(ur => ur.Status)?.Role;
                if (activeRole != null)
                {
                    userResponse.RoleId = activeRole.Id;
                    userResponse.RoleName = activeRole.Name;
                }
                userResponses.Add(userResponse);
            }

            return new PaginatedResponse<UserResponse>(userResponses, paginatedUsers.TotalCount, paginatedUsers.PageIndex, queryRequest.PageSize);
        }
    }
} 