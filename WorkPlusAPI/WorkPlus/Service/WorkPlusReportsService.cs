using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs.WorkPlusReportsDTOs;

namespace WorkPlusAPI.WorkPlus.Service
{
    public class WorkPlusReportsService : IWorkPlusReportsService
    {
        private readonly WorkPlusContext _context;

        public WorkPlusReportsService(WorkPlusContext context)
        {
            _context = context;
        }

        public async Task<PaginatedJobEntryReportDTO> GetPaginatedJobEntriesReportAsync(int pageNumber, int pageSize)
        {
            // Ensure valid pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Calculate skip value for pagination
            int skip = (pageNumber - 1) * pageSize;

            // Get total count of entries
            var totalCount = await _context.JobEntries.CountAsync();

            // Get paginated job entries with related data
            var entries = await _context.JobEntries
                .Include(je => je.Job)
                .Include(je => je.Worker)
                .Include(je => je.Group)
                .OrderByDescending(je => je.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(je => new JobEntryReportDTO
                {
                    EntryId = je.EntryId,
                    JobName = je.Job.JobName,
                    WorkerName = je.Worker != null ? je.Worker.FullName : null,
                    GroupName = je.Group != null ? je.Group.GroupName : null,
                    EntryType = je.EntryType,
                    ExpectedHours = je.ExpectedHours,
                    HoursTaken = je.HoursTaken,
                    ItemsCompleted = je.ItemsCompleted,
                    RatePerJob = je.RatePerJob,
                    ProductiveHours = je.ProductiveHours,
                    ExtraHours = je.ExtraHours,
                    UnderperformanceHours = je.UnderperformanceHours,
                    IncentiveAmount = je.IncentiveAmount,
                    TotalAmount = je.TotalAmount,
                    IsPostLunch = je.IsPostLunch,
                    Remarks = je.Remarks,
                    CreatedAt = je.CreatedAt
                })
                .ToListAsync();

            // Create and return paginated result
            return new PaginatedJobEntryReportDTO
            {
                Items = entries,
                TotalCount = totalCount
            };
        }
    }
} 