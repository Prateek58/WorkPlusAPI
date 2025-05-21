using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Model;

namespace WorkPlusAPI.WorkPlus.Service
{
    public class JobEntryService : IJobEntryService
    {
        private readonly WorkPlusContext _context;
        private readonly ILogger<JobEntryService> _logger;

        public JobEntryService(WorkPlusContext context, ILogger<JobEntryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<JobEntryMasterDataDTO> GetJobEntryMasterDataAsync()
        {
            try
            {
                var workers = await _context.Workers
                    .Where(w => w.IsActive == true)
                    .Select(w => new WorkerDTO
                    {
                        WorkerId = w.WorkerId,
                        FullName = w.FullName
                    })
                    .ToListAsync();

                var jobsQuery = _context.Jobs.AsQueryable();
                _logger.LogInformation("SQL Query: {SQL}", jobsQuery.ToQueryString());

                var jobs = await jobsQuery.Select(j => new JobDTO
                {
                    JobId = j.JobId,
                    JobName = j.JobName,
                    RatePerItem = j.RatePerItem,
                    RatePerHour = j.RatePerHour,
                    ExpectedHours = j.ExpectedHours,
                    ExpectedItemsPerHour = j.ExpectedItemsPerHour,
                    IncentiveBonusRate = j.IncentiveBonusRate,
                    PenaltyRate = j.PenaltyRate,
                    IncentiveType = j.IncentiveType
                }).ToListAsync();

                foreach (var job in jobs)
                {
                    _logger.LogInformation(
                        "Job {JobId} loaded: JobName={JobName}, RatePerItem={RatePerItem}, RatePerHour={RatePerHour}, ExpectedHours={ExpectedHours}, ExpectedItemsPerHour={ExpectedItemsPerHour}",
                        job.JobId, job.JobName, job.RatePerItem, job.RatePerHour, job.ExpectedHours, job.ExpectedItemsPerHour);
                }

                var jobGroups = await _context.JobGroups.Select(g => new JobGroupDTO
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName
                }).ToListAsync();

                return new JobEntryMasterDataDTO
                {
                    Workers = workers,
                    Jobs = jobs,
                    JobGroups = jobGroups
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job entry master data");
                throw;
            }
        }

        public async Task<JobEntry> CreateJobEntryAsync(JobEntry jobEntry)
        {
            try
            {
                _logger.LogInformation("Creating new job entry: {JobEntry}", System.Text.Json.JsonSerializer.Serialize(jobEntry));
                jobEntry.CreatedAt = DateTime.Now;

                var job = await _context.Jobs
                    .Include(j => j.JobType)
                    .FirstOrDefaultAsync(j => j.JobId == jobEntry.JobId);

                if (job == null) throw new Exception("Job not found");

                IIncentiveCalculator calculator = job.JobType.TypeName == "Hourly"
                    ? new HourlyIncentiveCalculator(_logger)
                    : new ItemBasedIncentiveCalculator(_logger);

                // Fix: Only use expected_hours from job if not already provided
                if (!jobEntry.ExpectedHours.HasValue || jobEntry.ExpectedHours.Value <= 0)
                {
                    jobEntry.ExpectedHours = job.ExpectedHours;
                }

                calculator.Calculate(job, jobEntry);

                _context.JobEntries.Add(jobEntry);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created job entry with ID: {EntryId}", jobEntry.EntryId);
                return jobEntry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job entry");
                throw;
            }
        }

        public async Task<IEnumerable<JobEntryDTO>> GetAllJobEntriesAsync()
        {
            try
            {
                return await _context.JobEntries
                    .Include(je => je.Job)
                    .Include(je => je.Worker)
                    .Include(je => je.Group)
                    .OrderByDescending(je => je.CreatedAt)
                    .Select(je => new JobEntryDTO
                    {
                        EntryId = je.EntryId,
                        JobId = je.JobId,
                        JobName = je.Job.JobName,
                        EntryType = je.EntryType,
                        WorkerId = je.WorkerId,
                        WorkerName = je.Worker.FullName,
                        GroupId = je.GroupId,
                        GroupName = je.Group.GroupName,
                        IsPostLunch = je.IsPostLunch,
                        ItemsCompleted = je.ItemsCompleted,
                        HoursTaken = je.HoursTaken,
                        RatePerJob = je.RatePerJob??0,
                        ExpectedHours = je.ExpectedHours,
                        ProductiveHours = je.ProductiveHours,
                        ExtraHours = je.ExtraHours,
                        UnderperformanceHours = je.UnderperformanceHours,
                        IncentiveAmount = je.IncentiveAmount,
                        TotalAmount = je.TotalAmount,
                        Remarks = je.Remarks,
                        IsFinalized = je.IsFinalized ?? false,
                        CreatedAt = je.CreatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all job entries");
                throw;
            }
        }

        public async Task<JobEntryDTO> GetJobEntryAsync(int id)
        {
            try
            {
                var jobEntry = await _context.JobEntries
                    .Include(je => je.Job)
                    .Include(je => je.Worker)
                    .Include(je => je.Group)
                    .FirstOrDefaultAsync(je => je.EntryId == id);

                if (jobEntry == null) return null;

                return new JobEntryDTO
                {
                    EntryId = jobEntry.EntryId,
                    JobId = jobEntry.JobId,
                    JobName = jobEntry.Job.JobName,
                    EntryType = jobEntry.EntryType,
                    WorkerId = jobEntry.WorkerId,
                    WorkerName = jobEntry.Worker?.FullName,
                    GroupId = jobEntry.GroupId,
                    GroupName = jobEntry.Group?.GroupName,
                    IsPostLunch = jobEntry.IsPostLunch,
                    ItemsCompleted = jobEntry.ItemsCompleted,
                    HoursTaken = jobEntry.HoursTaken,
                    RatePerJob = jobEntry.RatePerJob ?? 0,
                    ExpectedHours = jobEntry.ExpectedHours,
                    ProductiveHours = jobEntry.ProductiveHours,
                    ExtraHours = jobEntry.ExtraHours,
                    UnderperformanceHours = jobEntry.UnderperformanceHours,
                    IncentiveAmount = jobEntry.IncentiveAmount,
                    TotalAmount = jobEntry.TotalAmount,
                    Remarks = jobEntry.Remarks,
                    IsFinalized = jobEntry.IsFinalized ?? false,
                    CreatedAt = jobEntry.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job entry with ID: {EntryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteJobEntryAsync(int id)
        {
            try
            {
                var jobEntry = await _context.JobEntries.FindAsync(id);
                if (jobEntry == null) return false;

                var relatedWorkers = await _context.JobEntryWorkers
                    .Where(jew => jew.EntryId == id)
                    .ToListAsync();

                if (relatedWorkers.Any())
                {
                    _context.JobEntryWorkers.RemoveRange(relatedWorkers);
                }

                _context.JobEntries.Remove(jobEntry);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted job entry with ID: {EntryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job entry with ID: {EntryId}", id);
                throw;
            }
        }

        public async Task<(IEnumerable<JobEntryDTO> Items, int TotalCount)> GetPaginatedJobEntriesAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.JobEntries
                    .Include(je => je.Job)
                    .Include(je => je.Worker)
                    .Include(je => je.Group)
                    .OrderByDescending(je => je.CreatedAt);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(je => new JobEntryDTO
                    {
                        EntryId = je.EntryId,
                        JobId = je.JobId,
                        JobName = je.Job.JobName,
                        EntryType = je.EntryType,
                        WorkerId = je.WorkerId,
                        WorkerName = je.Worker.FullName,
                        GroupId = je.GroupId,
                        GroupName = je.Group.GroupName,
                        IsPostLunch = je.IsPostLunch,
                        ItemsCompleted = je.ItemsCompleted,
                        HoursTaken = je.HoursTaken,
                        RatePerJob = je.RatePerJob ?? 0,
                        ExpectedHours = je.ExpectedHours,
                        ProductiveHours = je.ProductiveHours,
                        ExtraHours = je.ExtraHours,
                        UnderperformanceHours = je.UnderperformanceHours,
                        IncentiveAmount = je.IncentiveAmount,
                        TotalAmount = je.TotalAmount,
                        Remarks = je.Remarks,
                        IsFinalized = je.IsFinalized ?? false,
                        CreatedAt = je.CreatedAt
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated job entries");
                throw;
            }
        }
    }

    public interface IIncentiveCalculator
    {
        void Calculate(Job job, JobEntry entry);
    }

    public class HourlyIncentiveCalculator : IIncentiveCalculator
    {
        private readonly ILogger _logger;
        public HourlyIncentiveCalculator(ILogger logger) => _logger = logger;

        public void Calculate(Job job, JobEntry entry)
        {
            // Get actual and expected hours
            decimal actual = entry.HoursTaken ?? 0;
            decimal expected = entry.ExpectedHours ?? job.ExpectedHours ?? 0;
            entry.ExpectedHours ??= expected;
            
            // Calculate productive, extra, and underperformance hours
            entry.ProductiveHours = Math.Min(actual, expected);
            entry.ExtraHours = Math.Max(actual - expected, 0);
            entry.UnderperformanceHours = Math.Max(expected - actual, 0);
            
            // Set rate per job if not already set
            entry.RatePerJob ??= job.RatePerHour;
            
            // Calculate base amount
            entry.TotalAmount = actual * (entry.RatePerJob ?? 0);

            // Calculate incentive or penalty
            decimal extraUnits = actual - expected;
            if (extraUnits > 0 && job.IncentiveBonusRate.HasValue)
            {
                // For percentage-based incentive
                if (job.IncentiveType == "Percentage")
                {
                    decimal bonusRatePerHour = (entry.RatePerJob ?? 0) * job.IncentiveBonusRate.Value / 100;
                    entry.IncentiveAmount = extraUnits * bonusRatePerHour;
                }
                else
                {
                    // Fixed rate bonus
                    entry.IncentiveAmount = extraUnits * job.IncentiveBonusRate.Value;
                }
            }
            else if (extraUnits < 0 && job.PenaltyRate.HasValue)
            {
                // Calculate penalty amount
                decimal penalty = Math.Abs(extraUnits) * job.PenaltyRate.Value;
                // Cap penalty at 50% of base amount
                decimal maxPenalty = (entry.TotalAmount ?? 0) * 0.5m;
                entry.IncentiveAmount = -1 * Math.Min(penalty, maxPenalty);
            }

            // Add incentive (positive or negative) to total amount
            entry.TotalAmount += entry.IncentiveAmount ?? 0;
        }
    }

    public class ItemBasedIncentiveCalculator : IIncentiveCalculator
    {
        private readonly ILogger _logger;
        public ItemBasedIncentiveCalculator(ILogger logger) => _logger = logger;

        public void Calculate(Job job, JobEntry entry)
        {
            // Get the expected hours from entry or default to job's expected hours
            decimal expected = entry.ExpectedHours ?? job.ExpectedHours ?? 0;
            decimal actual = entry.ItemsCompleted ?? 0;
            entry.ExpectedHours ??= expected;
            entry.RatePerJob ??= job.RatePerItem;

            // Calculate expected items based on expected hours and items per hour
            decimal expectedItems = expected * (job.ExpectedItemsPerHour ?? 0);
            decimal extraItems = Math.Max(actual - expectedItems, 0);
            decimal underItems = Math.Max(expectedItems - actual, 0);

            // Calculate extra hours or underperformance hours if expected items per hour is defined
            if ((job.ExpectedItemsPerHour ?? 0) > 0)
            {
                entry.ExtraHours = extraItems / job.ExpectedItemsPerHour.Value;
                entry.UnderperformanceHours = underItems / job.ExpectedItemsPerHour.Value;
            }

            // Calculate productive hours as expected minus underperformance
            entry.ProductiveHours = expected - (entry.UnderperformanceHours ?? 0);
            
            // Base amount calculation
            entry.TotalAmount = actual * (entry.RatePerJob ?? 0);

            // Calculate incentive for extra items or penalty for under items
            if (extraItems > 0 && job.IncentiveBonusRate.HasValue)
            {
                // For percentage-based incentive, apply the percentage to the rate per item
                if (job.IncentiveType == "Percentage")
                {
                    decimal bonusRatePerItem = (entry.RatePerJob ?? 0) * job.IncentiveBonusRate.Value / 100;
                    entry.IncentiveAmount = extraItems * bonusRatePerItem;
                }
                else
                {
                    // Fixed rate bonus
                    entry.IncentiveAmount = extraItems * job.IncentiveBonusRate.Value;
                }
            }
            else if (underItems > 0 && job.PenaltyRate.HasValue)
            {
                // Calculate penalty amount
                decimal penalty = underItems * job.PenaltyRate.Value;
                // Cap penalty at 50% of base amount
                decimal maxPenalty = (entry.TotalAmount ?? 0) * 0.5m;
                entry.IncentiveAmount = -1 * Math.Min(penalty, maxPenalty);
            }

            // Add incentive (positive or negative) to total amount
            entry.TotalAmount += entry.IncentiveAmount ?? 0;
        }
    }

}
