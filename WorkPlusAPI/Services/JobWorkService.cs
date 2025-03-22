using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.Data;
using WorkPlusAPI.DTOs;
using WorkPlusAPI.Models;

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

    public async Task<JobWorkResponse> GetJobWorksAsync(JobWorkFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting GetJobWorksAsync with filter: {@Filter}", filter);

            // Start with the basic query without any navigation properties
            var query = _context.JwWorks.AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                _logger.LogInformation("Filtering by start date: {StartDate}", filter.StartDate.Value);
                query = query.Where(jw => jw.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                _logger.LogInformation("Filtering by end date: {EndDate}", filter.EndDate.Value);
                query = query.Where(jw => jw.CreatedAt <= filter.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.JobId))
            {
                _logger.LogInformation("Filtering by job ID: {JobId}", filter.JobId);
                if (int.TryParse(filter.JobId, out int jobId))
                {
                    query = query.Where(jw => jw.GroupId == jobId);
                }
            }

            if (!string.IsNullOrEmpty(filter.JobWorkTypeId))
            {
                _logger.LogInformation("Filtering by work type ID: {WorkTypeId}", filter.JobWorkTypeId);
                if (sbyte.TryParse(filter.JobWorkTypeId, out sbyte workTypeId))
                {
                    query = query.Where(jw => jw.WorkTypeId == workTypeId);
                }
            }

            if (!string.IsNullOrEmpty(filter.JobType))
            {
                _logger.LogInformation("Filtering by job type: {JobType}", filter.JobType);
                if (filter.JobType == "group")
                {
                    query = query.Where(jw => jw.GroupId.HasValue);
                }
                else if (filter.JobType == "work")
                {
                    query = query.Where(jw => !jw.GroupId.HasValue);
                }
            }

            if (!string.IsNullOrEmpty(filter.UnitId))
            {
                _logger.LogInformation(new EventId(1, "FilteringUnit"), "Filtering by unit ID: {UnitId}", filter.UnitId);
                query = query.Where(jw => jw.UnitId.HasValue && jw.UnitId.Value.ToString() == filter.UnitId);
            }
            if (!string.IsNullOrEmpty(filter.EmployeeId))
            {
                _logger.LogInformation(new EventId(1, "FilteringEmployee"), "Filtering by employee ID: {EmployeeId}", filter.EmployeeId);
                if (int.TryParse(filter.EmployeeId, out int employeeId))
                {
                    query = query.Where(jw => jw.EmployeeId.HasValue && jw.EmployeeId.Value == employeeId);
                }
                else
                {
                    _logger.LogWarning(new EventId(1, "InvalidEmployeeId"), "Invalid employee ID format: {EmployeeId}", filter.EmployeeId);
                }
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "createdat" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(jw => jw.CreatedAt) : query.OrderBy(jw => jw.CreatedAt),
                    "workname" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(jw => jw.WorkName) : query.OrderBy(jw => jw.WorkName),
                    _ => query.OrderByDescending(jw => jw.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(jw => jw.CreatedAt);
            }

            // Get total count
            var total = await query.CountAsync();

            // Apply pagination and select only the columns we need
            var works = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(jw => new
                {
                    jw.WorkId,
                    jw.WorkName,
                    jw.WorkTypeId,
                    jw.TimePerPiece,
                    jw.RatePerPiece,
                    jw.GroupId,
                    jw.CreatedAt
                })
                .ToListAsync();

            // Get the workgroups and worktypes in a separate query
            var workTypeIds = works.Where(w => w.WorkTypeId.HasValue).Select(w => w.WorkTypeId.Value).ToList();
            var workTypes = new Dictionary<sbyte, string>();
            
            if (workTypeIds.Any())
            {
                var workTypesList = await _context.JwWorkTypes
                    .Where(wt => workTypeIds.Contains(wt.WorkTypeId))
                    .Select(wt => new { wt.WorkTypeId, wt.TypeName })
                    .ToListAsync();
                
                foreach (var wt in workTypesList)
                {
                    workTypes[wt.WorkTypeId] = wt.TypeName;
                }
            }

            var groupIds = works.Where(w => w.GroupId.HasValue).Select(w => w.GroupId.Value).ToList();
            var workGroups = new Dictionary<int, string>();
            
            if (groupIds.Any())
            {
                var workGroupsList = await _context.JwWorkGroups
                    .Where(wg => groupIds.Contains(wg.GroupId))
                    .Select(wg => new { wg.GroupId, wg.GroupName })
                    .ToListAsync();
                
                foreach (var wg in workGroupsList)
                {
                    workGroups[wg.GroupId] = wg.GroupName;
                }
            }

            // Create the result items with the expected property names
            var items = works.Select(jw => new
            {
                id = jw.WorkId.ToString(),
                jobId = jw.GroupId?.ToString() ?? string.Empty,
                jobName = jw.GroupId.HasValue && workGroups.ContainsKey(jw.GroupId.Value)
                    ? workGroups[jw.GroupId.Value]
                    : jw.WorkName ?? string.Empty,
                jobWorkTypeId = jw.WorkTypeId?.ToString() ?? string.Empty,
                jobWorkTypeName = jw.WorkTypeId.HasValue && workTypes.ContainsKey(jw.WorkTypeId.Value) 
                    ? workTypes[jw.WorkTypeId.Value] 
                    : string.Empty,
                employeeId = string.Empty,
                employeeName = string.Empty,
                date = jw.CreatedAt ?? DateTime.Now,
                hours = jw.TimePerPiece ?? 0,
                quantity = 1,
                unitId = string.Empty,
                unitName = string.Empty,
                amount = jw.RatePerPiece ?? 0m,
                remarks = jw.WorkName ?? string.Empty
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} job works", items.Count);

            return new JobWorkResponse
            {
                Data = items,
                Total = total
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job works: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<JobWorkSummaryDto> GetJobWorkSummaryAsync(JobWorkFilter filter)
    {
        try
        {
            var jobWorksResponse = await GetJobWorksAsync(filter);
            // Cast the dynamic data to a suitable form
            var jobWorks = jobWorksResponse.Data;
            
            var summary = new JobWorkSummaryDto
            {
                TotalHours = 0, // Calculate from the data if needed
                TotalQuantity = 0, // Calculate from the data if needed 
                TotalAmount = 0, // Calculate from the data if needed
                TotalRecords = jobWorksResponse.Total
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
            var jobWorksResponse = await GetJobWorksAsync(filter);
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
            var jobWorksResponse = await GetJobWorksAsync(filter);
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
                    (e.FirstName != null && e.FirstName.Contains(searchTerm)) ||
                    (e.LastName != null && e.LastName.Contains(searchTerm)))
                .Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId.ToString(),
                    Name = $"{e.FirstName ?? ""} {e.LastName ?? ""}".Trim(),
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