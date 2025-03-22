using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.Data;
using WorkPlusAPI.DTOs;

namespace WorkPlusAPI.Services;

public class JobWorkService : IJobWorkService
{
    private readonly ArchiveContext _context;
    private readonly ILogger<JobWorkService> _logger;

    public JobWorkService(ArchiveContext context, ILogger<JobWorkService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UnitDto>> GetUnitsAsync()
    {
        try
        {
            var units = await _context.DboUnits
                .Select(u => new UnitDto
                {
                    Id = u.UnitId.ToString(),
                    Name = u.UnitName
                })
                .ToListAsync();
            return units;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting units");
            throw;
        }
    }

    public async Task<IEnumerable<JobWorkTypeDto>> GetJobWorkTypesAsync()
    {
        try
        {
            var types = await _context.JwWorkTypes
                .Select(t => new JobWorkTypeDto
                {
                    Id = t.WorkTypeId.ToString(),
                    Name = t.TypeName,
                    Description = t.TypeName // Using TypeName as description since there's no separate description field
                })
                .ToListAsync();
            return types;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job work types");
            throw;
        }
    }

    public async Task<IEnumerable<JobDto>> GetJobsAsync(bool isGroup = false)
    {
        try
        {
            if (isGroup)
            {
                // Get work groups
                var groups = await _context.JwWorkGroups
                    .Select(j => new JobDto
                    {
                        Id = j.GroupId.ToString(),
                        Name = j.GroupName,
                        Description = j.GroupName,
                        IsGroup = true,
                        ParentId = null
                    })
                    .ToListAsync();
                return groups;
            }
            else
            {
                // Get individual works
                var works = await _context.JwWorks
                    .Select(j => new JobDto
                    {
                        Id = j.WorkId.ToString(),
                        Name = j.WorkName,
                        Description = j.WorkName,
                        IsGroup = false,
                        ParentId = j.GroupId.HasValue ? j.GroupId.Value.ToString() : null
                    })
                    .ToListAsync();
                return works;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs");
            throw;
        }
    }

    public async Task<IEnumerable<JobWorkDto>> GetJobWorksAsync(JobWorkFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting GetJobWorksAsync with filter: {@Filter}", filter);

            var query = _context.JwWorks
                .Include(jw => jw.WorkGroup)
                .Include(jw => jw.WorkType)
                .Include(jw => jw.Employee)
                .Include(jw => jw.Unit)
                .AsQueryable();

            // Handle job type filter
            if (!string.IsNullOrEmpty(filter.JobType))
            {
                _logger.LogInformation(new EventId(1, "FilteringJobType"), "Filtering by job type: {JobType}", filter.JobType);
                if (filter.JobType == "group")
                {
                    query = query.Where(jw => jw.GroupId.HasValue);
                }
                else if (filter.JobType == "work")
                {
                    query = query.Where(jw => !jw.GroupId.HasValue);
                }
            }

            if (filter.StartDate.HasValue)
            {
                _logger.LogInformation(new EventId(1, "FilteringStartDate"), "Filtering by start date: {StartDate}", filter.StartDate.Value);
                query = query.Where(jw => jw.CreatedAt >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                _logger.LogInformation(new EventId(1, "FilteringEndDate"), "Filtering by end date: {EndDate}", filter.EndDate.Value);
                query = query.Where(jw => jw.CreatedAt <= filter.EndDate.Value);
            }
            if (!string.IsNullOrEmpty(filter.JobId))
            {
                _logger.LogInformation(new EventId(1, "FilteringJobId"), "Filtering by job ID: {JobId}", filter.JobId);
                query = query.Where(jw => jw.GroupId.HasValue && jw.GroupId.Value.ToString() == filter.JobId);
            }
            if (!string.IsNullOrEmpty(filter.JobWorkTypeId))
            {
                _logger.LogInformation(new EventId(1, "FilteringWorkType"), "Filtering by work type ID: {WorkTypeId}", filter.JobWorkTypeId);
                query = query.Where(jw => jw.WorkTypeId.HasValue && jw.WorkTypeId.Value.ToString() == filter.JobWorkTypeId);
            }
            if (!string.IsNullOrEmpty(filter.UnitId))
            {
                _logger.LogInformation(new EventId(1, "FilteringUnit"), "Filtering by unit ID: {UnitId}", filter.UnitId);
                query = query.Where(jw => jw.UnitId.HasValue && jw.UnitId.Value.ToString() == filter.UnitId);
            }
            if (!string.IsNullOrEmpty(filter.EmployeeId))
            {
                _logger.LogInformation("Filtering by employee ID: {EmployeeId}", filter.EmployeeId);
                if (int.TryParse(filter.EmployeeId, out int employeeId))
                {
                    query = query.Where(jw => jw.EmployeeId.HasValue && jw.EmployeeId.Value == employeeId);
                }
                else
                {
                    _logger.LogWarning("Invalid employee ID format: {EmployeeId}", filter.EmployeeId);
                }
            }

            var jobWorks = await query
                .Select(jw => new JobWorkDto
                {
                    Id = jw.WorkId.ToString(),
                    JobId = jw.GroupId.HasValue ? jw.GroupId.Value.ToString() : string.Empty,
                    JobName = jw.WorkGroup != null ? jw.WorkGroup.GroupName : string.Empty,
                    JobWorkTypeId = jw.WorkTypeId.HasValue ? jw.WorkTypeId.Value.ToString() : string.Empty,
                    JobWorkTypeName = jw.WorkType != null ? jw.WorkType.TypeName : string.Empty,
                    EmployeeId = jw.EmployeeId.HasValue ? jw.EmployeeId.Value.ToString() : string.Empty,
                    EmployeeName = jw.Employee != null ? $"{jw.Employee.FirstName} {jw.Employee.LastName}".Trim() : string.Empty,
                    Date = jw.CreatedAt ?? DateTime.Now,
                    Hours = jw.TimePerPiece ?? 0,
                    Quantity = 1,
                    UnitId = jw.UnitId.HasValue ? jw.UnitId.Value.ToString() : string.Empty,
                    UnitName = jw.Unit != null ? jw.Unit.UnitName : string.Empty,
                    Amount = jw.RatePerPiece ?? 0,
                    Remarks = jw.WorkName ?? string.Empty
                })
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} job works", jobWorks.Count);
            return jobWorks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job works. Filter: {@Filter}", filter);
            throw;
        }
    }

    public async Task<JobWorkSummaryDto> GetJobWorkSummaryAsync(JobWorkFilter filter)
    {
        try
        {
            var jobWorks = await GetJobWorksAsync(filter);
            var summary = new JobWorkSummaryDto
            {
                TotalHours = jobWorks.Sum(jw => jw.Hours),
                TotalQuantity = jobWorks.Sum(jw => jw.Quantity),
                TotalAmount = jobWorks.Sum(jw => jw.Amount),
                TotalRecords = jobWorks.Count()
            };
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job work summary");
            throw;
        }
    }

    public async Task<byte[]> ExportToExcelAsync(JobWorkFilter filter)
    {
        try
        {
            var jobWorks = await GetJobWorksAsync(filter);
            // TODO: Implement Excel export
            throw new NotImplementedException("Excel export functionality not implemented yet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting job works to Excel");
            throw;
        }
    }

    public async Task<byte[]> ExportToPdfAsync(JobWorkFilter filter)
    {
        try
        {
            var jobWorks = await GetJobWorksAsync(filter);
            // TODO: Implement PDF export
            throw new NotImplementedException("PDF export functionality not implemented yet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting job works to PDF");
            throw;
        }
    }

    public async Task<IEnumerable<EmployeeDto>> SearchEmployeesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Enumerable.Empty<EmployeeDto>();
            }

            var employees = await _context.JwEmployees
                .Where(e => 
                    e.EmployeeId.ToString().Contains(searchTerm) || 
                    e.FirstName.Contains(searchTerm) ||
                    e.LastName.Contains(searchTerm))
                .Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId.ToString(),
                    Name = $"{e.FirstName} {e.LastName}".Trim(),
                    EmployeeId = e.EmployeeId.ToString()
                })
                .Take(10)
                .ToListAsync();

            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees");
            throw;
        }
    }
} 