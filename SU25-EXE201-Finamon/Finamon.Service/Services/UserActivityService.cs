using Finamon_Data;
using Finamon_Data.Entities;
using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finamon.Service.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly AppDbContext _context;
        private readonly Random _random;

        public UserActivityService(AppDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task LogUserActivityAsync(int userId)
        {
            // Validate if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId && !u.IsDelete);
            if (!userExists)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            // Random từ 1 đến 8 giờ 
            int useTimeInMinutes = _random.Next(60, 481);

            var activity = new UserActivity
            {
                UserId = userId,
                UseTime = useTimeInMinutes,
                CreatedDate = DateTime.UtcNow.AddHours(7)
            };

            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserActivityResponse>> GetUserActivitiesAsync(int userId)
        {
            var activities = await _context.UserActivities
                .Where(ua => ua.UserId == userId && !ua.IsDelete)
                .OrderByDescending(ua => ua.CreatedDate)
                .Select(ua => new UserActivityResponse
                {
                    Id = ua.Id,
                    UserId = ua.UserId,
                    UseTime = ua.UseTime,
                    CreatedDate = ua.CreatedDate
                })
                .ToListAsync();

            return activities;
        }

        public async Task<UserActivityStatsResponse> GetUserActivityStatsAsync(int userId)
        {
            var activities = await _context.UserActivities
                .Where(ua => ua.UserId == userId && !ua.IsDelete)
                .ToListAsync();

            if (!activities.Any())
            {
                return new UserActivityStatsResponse
                {
                    TotalActivities = 0,
                    TotalMinutes = 0,
                    AverageMinutes = 0,
                    LastActivity = DateTime.MinValue
                };
            }

            return new UserActivityStatsResponse
            {
                TotalActivities = activities.Count,
                TotalMinutes = activities.Sum(a => a.UseTime),
                AverageMinutes = activities.Average(a => a.UseTime),
                LastActivity = activities.Max(a => a.CreatedDate)
            };
        }
    }
} 