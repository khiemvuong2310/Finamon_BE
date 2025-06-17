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
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool VerifyPassword(string storedHash, string password)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }
                return true;
            }
        }

        private bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return false;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);
            //bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasLetter 
                && hasDigit 
                //&& hasSpecial
                ;
        }

        public async Task<UserResponse> CreateUserAsync(UserRequest request)
        {
            // Validate email and username
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(request.UserName))
                throw new ArgumentException("Username is required");

            // Validate password
            if (!ValidatePassword(request.Password))
                throw new ArgumentException("Password must be at least 6 characters long and contain letters, numbers");

            // Check for existing user
            var existingUser = await _context.Users
                .AnyAsync(u => (u.Email == request.Email.Trim() || u.UserName == request.UserName.Trim()) && !u.IsDelete);

            if (existingUser)
                throw new InvalidOperationException("User with this email or username already exists");

            // Validate role
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
                throw new ArgumentException($"Invalid role ID: {request.RoleId}");

            var user = _mapper.Map<User>(request);
            user.Email = request.Email.Trim();
            user.UserName = request.UserName.Trim();
            
            // Handle image upload
            if (request.ImageFile != null)
            {
                try
                {
                    var imageUrl = await _firebaseStorageService.UploadImageAsync(request.ImageFile);
                    user.Image = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to upload image: {ex.Message}");
                }
            }

            // Set user properties
            user.Password = HashPassword(request.Password);
            user.CreatedDate = DateTime.UtcNow.AddHours(7);
            user.UpdatedDate = DateTime.UtcNow.AddHours(7);
            user.Status = true;
            user.EmailVerified = false;
            user.IsDelete = false;

            // Create user
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Create user role
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = request.RoleId,
                Status = true,
                CreatedDate = DateTime.UtcNow.AddHours(7)
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id);
        }

        public async Task<UserResponse> UpdateUserAsync(int id, UserUpdateRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found or has been deleted");

            // Validate email uniqueness if being updated
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email.Trim() && u.Id != id && !u.IsDelete);
                if (emailExists)
                    throw new InvalidOperationException("Email is already in use");
                user.Email = request.Email.Trim();
            }

            // Validate username uniqueness if being updated
            if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
            {
                var usernameExists = await _context.Users
                    .AnyAsync(u => u.UserName == request.UserName.Trim() && u.Id != id && !u.IsDelete);
                if (usernameExists)
                    throw new InvalidOperationException("Username is already in use");
                user.UserName = request.UserName.Trim();
            }

            // Handle image upload
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
                    user.Image = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to update image: {ex.Message}");
                }
            }

            // Update other properties
            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone.Trim();
            if (!string.IsNullOrWhiteSpace(request.Location))
                user.Location = request.Location.Trim();
            if (!string.IsNullOrWhiteSpace(request.Country))
                user.Country = request.Country.Trim();
            if (request.Status.HasValue)
                user.Status = request.Status.Value;

            user.UpdatedDate = DateTime.UtcNow.AddHours(7);

            // Update role if provided
            if (request.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(request.RoleId.Value);
                if (role == null)
                    throw new ArgumentException($"Invalid role ID: {request.RoleId.Value}");

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
                        CreatedDate = DateTime.UtcNow.AddHours(7)
                    };
                    await _context.UserRoles.AddAsync(newUserRole);
                }
            }

            await _context.SaveChangesAsync();
            return await GetUserByIdAsync(user.Id);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Expenses.Where(e => !e.IsDelete))
                .Include(u => u.Categories.Where(c => !c.IsDelete))
                .Include(u => u.Reports.Where(r => !r.IsDelete))
                .Include(u => u.ChatSessions.Where(cs => !cs.IsDelete))
                .Include(u => u.UserMemberships.Where(um => !um.IsDelete))
                .Include(u => u.Budgets.Where(b => !b.IsDelete))
                .Include(u => u.Comments.Where(c => !c.IsDelete))
                .Include(u => u.UserActivities.Where(ua => !ua.IsDelete))
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found or has been deleted");

            // Check for active memberships
            if (user.UserMemberships.Any(um => um.EndDate > DateTime.UtcNow.AddHours(7)))
                throw new InvalidOperationException("Cannot delete user with active memberships");

            // Soft delete user and related entities
            user.IsDelete = true;
            user.Status = false;
            user.UpdatedDate = DateTime.UtcNow.AddHours(7);

            // Soft delete user roles
            foreach (var userRole in user.UserRoles)
            {
                userRole.Status = false;
            }

            // Soft delete related entities
            foreach (var expense in user.Expenses)
            {
                expense.IsDelete = true;
                expense.UpdateDate = DateTime.UtcNow.AddHours(7);
            }

            foreach (var category in user.Categories)
            {
                category.IsDelete = true;
                category.UpdatedDate = DateTime.UtcNow.AddHours(7);
            }

            foreach (var report in user.Reports)
            {
                report.IsDelete = true;
                report.UpdatedDate = DateTime.UtcNow.AddHours(7);
            }

            foreach (var chatSession in user.ChatSessions)
            {
                chatSession.IsDelete = true;
                chatSession.UpdatedDate = DateTime.UtcNow.AddHours(7);
            }

            foreach (var userMembership in user.UserMemberships)
            {
                userMembership.IsDelete = true;
            }

            foreach (var budget in user.Budgets)
            {
                budget.IsDelete = true;
                budget.UpdatedDate = DateTime.UtcNow.AddHours(7);
            }

            foreach (var comment in user.Comments)
            {
                comment.IsDelete = true;
                comment.UpdatedDate = DateTime.UtcNow.AddHours(7);
            }

            foreach (var activity in user.UserActivities)
            {
                activity.IsDelete = true;
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
            if (!string.IsNullOrEmpty(queryRequest.Country))
            {
                queryable = queryable.Where(u => u.Country != null && u.Country.Contains(queryRequest.Country));
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
            if (queryRequest.SortBy.HasValue)
            {
                queryable = queryRequest.SortBy.Value switch
                {
                    SortByEnum.CreatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(u => u.CreatedDate)
                        : queryable.OrderBy(u => u.CreatedDate),
                    SortByEnum.UpdatedDate => queryRequest.SortDescending
                        ? queryable.OrderByDescending(u => u.UpdatedDate)
                        : queryable.OrderBy(u => u.UpdatedDate),
                    _ => queryable.OrderByDescending(u => u.CreatedDate)
                };
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

        public async Task<UserResponse> UpdateUserImageAsync(int id, IFormFile imageFile)
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
                throw new UnauthorizedAccessException("You are not authorized to update this user's image");
            }

            var user = await _context.Set<User>()
                .Include(u => u.UserRoles.Where(ur => ur.Status))
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDelete);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (imageFile == null)
            {
                throw new ArgumentException("Image file is required");
            }

            try
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.Image))
                {
                    await _firebaseStorageService.DeleteImageAsync(user.Image);
                }

                // Upload new image
                var imageUrl = await _firebaseStorageService.UploadImageAsync(imageFile);
                user.Image = imageUrl;
                user.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return await GetUserByIdAsync(user.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user image: {ex.Message}");
            }
        }
    }
} 