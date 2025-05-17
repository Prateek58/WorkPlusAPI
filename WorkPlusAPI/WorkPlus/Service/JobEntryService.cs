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

                // Log the SQL query for debugging
                var jobsQuery = _context.Jobs
                    .AsQueryable();
                
                _logger.LogInformation("SQL Query: {SQL}", jobsQuery.ToQueryString());

                var jobs = await jobsQuery
                    .Select(j => new JobDTO
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
                    })
                    .ToListAsync();
                
                // Log the job data for debugging
                foreach (var job in jobs)
                {
                    _logger.LogInformation(
                        "Job {JobId} loaded: JobName={JobName}, RatePerItem={RatePerItem}, RatePerHour={RatePerHour}, " +
                        "ExpectedHours={ExpectedHours}, ExpectedItemsPerHour={ExpectedItemsPerHour}",
                        job.JobId, job.JobName, job.RatePerItem, job.RatePerHour, job.ExpectedHours, job.ExpectedItemsPerHour);
                }

                var jobGroups = await _context.JobGroups
                    .Select(g => new JobGroupDTO
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName
                    })
                    .ToListAsync();

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
                _logger.LogInformation("Creating new job entry: {JobEntry}", 
                    System.Text.Json.JsonSerializer.Serialize(jobEntry));
                
                // Set creation time
                jobEntry.CreatedAt = DateTime.Now;
                
                // Use the simplified incentive calculation
                await CalculateSimpleIncentiveAsync(jobEntry);
                
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
                        RatePerJob = je.RatePerJob,
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
                
                if (jobEntry == null)
                {
                    return null;
                }
                
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
                    RatePerJob = jobEntry.RatePerJob,
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
                if (jobEntry == null)
                {
                    return false;
                }
                
                // Also delete related workers for this entry
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
        
        // Simple and direct incentive calculation
        private async Task CalculateSimpleIncentiveAsync(JobEntry jobEntry)
        {
            var job = await _context.Jobs.FindAsync(jobEntry.JobId);
            if (job == null) return;
            
            // Set default values
            jobEntry.ProductiveHours = 0;
            jobEntry.ExtraHours = 0;
            jobEntry.UnderperformanceHours = 0;
            jobEntry.IncentiveAmount = 0;
            jobEntry.TotalAmount = 0;
            
            // Set expected hours from job if not provided
            jobEntry.ExpectedHours ??= job.ExpectedHours;
            
            decimal expected = jobEntry.ExpectedHours ?? 0;
            decimal actual = 0;
            
            // Determine actual output based on job type
            if (job.RatePerHour.HasValue && jobEntry.HoursTaken.HasValue)
            {
                actual = jobEntry.HoursTaken.Value;
                jobEntry.ProductiveHours = Math.Min(actual, expected);
                
                // Extra hours or underperformance
                if (actual > expected)
                    jobEntry.ExtraHours = actual - expected;
                else
                    jobEntry.UnderperformanceHours = expected - actual;
                    
                // Base amount
                jobEntry.TotalAmount = actual * jobEntry.RatePerJob;
            }
            else if (job.RatePerItem.HasValue && jobEntry.ItemsCompleted.HasValue)
            {
                decimal expectedItems = job.ExpectedItemsPerHour.HasValue ? 
                    job.ExpectedItemsPerHour.Value * expected : 0;
                    
                actual = jobEntry.ItemsCompleted.Value;
                
                // Adjust hours based on performance
                if (job.ExpectedItemsPerHour.HasValue && job.ExpectedItemsPerHour.Value > 0)
                {
                    if (actual > expectedItems)
                        jobEntry.ExtraHours = (actual - expectedItems) / job.ExpectedItemsPerHour.Value;
                    else
                        jobEntry.UnderperformanceHours = (expectedItems - actual) / job.ExpectedItemsPerHour.Value;
                }
                
                // Base amount
                jobEntry.TotalAmount = actual * jobEntry.RatePerJob;
            }
            
            // Log values
            _logger.LogWarning("SIMPLE CALCULATION - Expected: {Expected}, Actual: {Actual}, Difference: {Diff}", 
                expected, actual, actual - expected);
                
            // Calculate incentive
            if (actual > expected && job.IncentiveBonusRate.HasValue)
            {
                _logger.LogWarning("OVERPERFORMING - Calculating BONUS");
                decimal extraUnits = actual - expected;
                decimal bonusRate = job.IncentiveBonusRate.Value;
                
                // Calculate bonus based on incentive type
                if (job.IncentiveType == "Percentage")
                {
                    decimal extraPay = extraUnits * jobEntry.RatePerJob;
                    jobEntry.IncentiveAmount = (extraPay * bonusRate) / 100m;
                }
                else  // Default to PerUnit
                {
                    jobEntry.IncentiveAmount = extraUnits * bonusRate;
                }
                
                _logger.LogWarning("BONUS CALCULATION - ExtraUnits: {Extra}, Rate: {Rate}, Amount: {Amount}",
                    extraUnits, bonusRate, jobEntry.IncentiveAmount);
            }
            else if (actual < expected && job.PenaltyRate.HasValue)
            {
                _logger.LogWarning("UNDERPERFORMING - Calculating PENALTY");
                decimal shortfall = expected - actual;
                decimal penaltyRate = job.PenaltyRate.Value;
                
                // Calculate penalty (negative incentive)
                decimal rawPenalty = shortfall * penaltyRate;
                
                // Cap the penalty at 50% of total amount
                decimal maxPenalty = (jobEntry.TotalAmount ?? 0) * 0.5m;
                jobEntry.IncentiveAmount = -1 * Math.Min(rawPenalty, maxPenalty);
                
                _logger.LogWarning("PENALTY CALCULATION - Shortfall: {Short}, Rate: {Rate}, Raw: {Raw}, Max: {Max}, Final: {Final}",
                    shortfall, penaltyRate, rawPenalty, maxPenalty, jobEntry.IncentiveAmount);
            }
            
            // Add incentive to total
            jobEntry.TotalAmount += jobEntry.IncentiveAmount ?? 0;
            
            _logger.LogWarning("FINAL AMOUNTS - Base: {Base}, Incentive: {Incentive}, Total: {Total}",
                jobEntry.TotalAmount - (jobEntry.IncentiveAmount ?? 0), 
                jobEntry.IncentiveAmount, 
                jobEntry.TotalAmount);
        }
    }
} 