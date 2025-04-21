using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.Data;
using WorkPlusAPI.DTOs;
using WorkPlusAPI.Models;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using OfficeOpenXml.Style;
using System.Text.Json;

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

    // Define a class to represent the query result
    private class JobWorkQueryResult
    {
        // Entry data
        public long EntryId { get; set; }
        public DateTime? EntryDate { get; set; }
        public int? JwNo { get; set; }
        public decimal? QtyItems { get; set; }
        public decimal? QtyHours { get; set; }
        public decimal? RateForJob { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool? IsApproved { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public int? EntryByUserId { get; set; }
        
        // Employee data
        public short? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        
        // Unit data
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        
        // Work data
        public long? WorkId { get; set; }
        public string? WorkName { get; set; }
        
        // Work type data
        public short? WorkTypeId { get; set; }
        public string? WorkTypeName { get; set; }
        
        // Group data
        public short? GroupId { get; set; }
        public string? GroupName { get; set; }
    }

    public async Task<JobWorkResponse> GetJobWorksAsync(JobWorkFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting GetJobWorksAsync with filter: {@Filter}", filter);

            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("Report type: {ReportType}", isAsOnReport ? "As On Report" : "Date Range Report");

            // Build the base query using IQueryable with concrete type (not dynamic)
            var query = from entry in _context.JwEntries
                        join work in _context.JwWorks on entry.WorkId equals work.WorkId into workJoin
                        from work in workJoin.DefaultIfEmpty()
                        join employee in _context.JwEmployees on entry.EmployeeId equals employee.EmployeeId into empJoin
                        from employee in empJoin.DefaultIfEmpty()
                        join unit in _context.DboUnits on entry.UnitId equals unit.UnitId into unitJoin
                        from unit in unitJoin.DefaultIfEmpty()
                        join workType in _context.JwWorkTypes on work.WorkTypeId equals workType.WorkTypeId into typeJoin
                        from workType in typeJoin.DefaultIfEmpty()
                        join workGroup in _context.JwWorkGroups on work.GroupId equals workGroup.GroupId into groupJoin
                        from workGroup in groupJoin.DefaultIfEmpty()
                        select new JobWorkQueryResult
                        {
                            EntryId = entry.EntryId,
                            EntryDate = entry.EntryDate,
                            JwNo = entry.JwNo,
                            QtyItems = entry.QtyItems,
                            QtyHours = entry.QtyHours,
                            RateForJob = entry.RateForJob,
                            TotalAmount = entry.TotalAmount,
                            IsApproved = entry.IsApproved,
                            ApprovedBy = entry.ApprovedBy,
                            ApprovedOn = entry.ApprovedOn,
                            EntryByUserId = entry.EntryByUserId,
                            EmployeeId = (short?)entry.EmployeeId,
                            EmployeeName = employee != null ? (employee.FirstName + " " + employee.LastName).Trim() : string.Empty,
                            UnitId = entry.UnitId,
                            UnitName = unit != null ? unit.UnitName : string.Empty,
                            WorkId = entry.WorkId,
                            WorkName = work != null ? work.WorkName : string.Empty,
                            WorkTypeId = work != null ? work.WorkTypeId : null,
                            WorkTypeName = workType != null ? workType.TypeName : string.Empty,
                            GroupId = work != null ? (short?)work.GroupId : null,
                            GroupName = workGroup != null ? workGroup.GroupName : string.Empty
                        };

            // Apply filters - all of these operations are deferred, not executing the query yet
            // For "as on" reports, we don't filter by start date (show all data up to end date)
            if (filter.StartDate.HasValue && !isAsOnReport)
            {
                _logger.LogInformation("Filtering by start date: {StartDate}", filter.StartDate.Value);
                query = query.Where(x => x.EntryDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                _logger.LogInformation("Filtering by end date: {EndDate}", filter.EndDate.Value);
                query = query.Where(x => x.EntryDate <= filter.EndDate.Value);
            }

            // Apply other filters
            if (!string.IsNullOrEmpty(filter.JobId))
            {
                _logger.LogInformation("Filtering by job ID: {JobId}", filter.JobId);
                query = query.Where(x => x.WorkId.ToString() == filter.JobId);
            }

            if (!string.IsNullOrEmpty(filter.JobWorkTypeId))
            {
                _logger.LogInformation("Filtering by work type ID: {WorkTypeId}", filter.JobWorkTypeId);
                query = query.Where(x => x.WorkTypeId.ToString() == filter.JobWorkTypeId);
            }

            if (!string.IsNullOrEmpty(filter.UnitId))
            {
                _logger.LogInformation("Filtering by unit ID: {UnitId}", filter.UnitId);
                var unitIdInt = int.Parse(filter.UnitId);
                query = query.Where(x => x.UnitId == unitIdInt);
            }

            if (!string.IsNullOrEmpty(filter.EmployeeId))
            {
                _logger.LogInformation("Filtering by employee ID: {EmployeeId}", filter.EmployeeId);
                var employeeIdInt = short.Parse(filter.EmployeeId);
                query = query.Where(x => x.EmployeeId == employeeIdInt);
            }

            if (!string.IsNullOrEmpty(filter.JobType))
            {
                _logger.LogInformation("Filtering by job type: {JobType}", filter.JobType);
                if (filter.JobType == "group")
                {
                    query = query.Where(x => x.GroupId != null);
                }
                else if (filter.JobType == "work")
                {
                    query = query.Where(x => x.GroupId == null);
                }
            }

            // Apply sorting - still just building the query
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "entrydate" => filter.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(x => x.EntryDate) 
                        : query.OrderBy(x => x.EntryDate),
                    "totalamount" => filter.SortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(x => x.TotalAmount) 
                        : query.OrderBy(x => x.TotalAmount),
                    "employeename" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.EmployeeName)
                        : query.OrderBy(x => x.EmployeeName),
                    "workname" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.WorkName)
                        : query.OrderBy(x => x.WorkName),
                    "qtyitems" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.QtyItems)
                        : query.OrderBy(x => x.QtyItems),
                    "qtyhours" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.QtyHours)
                        : query.OrderBy(x => x.QtyHours),
                    "rateforjob" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.RateForJob)
                        : query.OrderBy(x => x.RateForJob),
                    "unitname" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.UnitName)
                        : query.OrderBy(x => x.UnitName),
                    "worktype" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.WorkTypeName)
                        : query.OrderBy(x => x.WorkTypeName),
                    "groupname" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.GroupName)
                        : query.OrderBy(x => x.GroupName),
                    "jwno" => filter.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(x => x.JwNo)
                        : query.OrderBy(x => x.JwNo),
                    _ => query.OrderByDescending(x => x.EntryDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.EntryDate);
            }

            // Get the total count first using the same filtered query - this runs a COUNT query in SQL
            var total = await query.CountAsync();
            _logger.LogInformation("Total records before pagination: {Total}", total);

            // Apply pagination and finally materialize the data with ToListAsync
            var pageSize = filter.PageSize ?? 10;
            var page = filter.Page ?? 1;
            
            // This is where the query is actually executed with pagination
            var entries = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Records retrieved after pagination: {Count}", entries.Count);

            // Map to response DTOs
            var jobWorkDtos = entries.Select(x => new JobWorkDto
            {
                EntryId = x.EntryId,
                EntryDate = x.EntryDate,
                JwNo = x.JwNo?.ToString(),
                WorkName = x.WorkName,
                EmployeeId = x.EmployeeId?.ToString(),
                EmployeeName = x.EmployeeName,
                UnitName = x.UnitName,
                WorkType = x.WorkTypeName,
                GroupName = x.GroupName,
                QtyItems = x.QtyItems,
                QtyHours = x.QtyHours,
                RateForJob = x.RateForJob,
                TotalAmount = x.TotalAmount,
                IsApproved = x.IsApproved,
                ApprovedBy = x.ApprovedBy?.ToString(),
                ApprovedOn = x.ApprovedOn,
                EntryByUserId = x.EntryByUserId?.ToString(),
                WorkId = x.WorkId?.ToString(),
                Remarks = x.WorkName
            }).ToList();

            _logger.LogInformation("Successfully mapped {Count} job works to DTOs", jobWorkDtos.Count);

            return new JobWorkResponse
            {
                Data = jobWorkDtos,
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
            _logger.LogInformation("Starting GetJobWorkSummaryAsync with filter: {@Filter}", 
                JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("Summary Report type: {ReportType}", 
                isAsOnReport ? "As On Report" : "Date Range Report");

            // Default to today for both dates if not provided
            if (!filter.StartDate.HasValue && !filter.EndDate.HasValue)
            {
                filter.EndDate = DateTime.Today;
                isAsOnReport = true; // Treat this as an "as on" report
                _logger.LogInformation("No dates provided, defaulting to As On Report for today: {Date}", 
                    DateTime.Today);
            }
            else if (!filter.EndDate.HasValue)
            {
                filter.EndDate = DateTime.Today;
                _logger.LogInformation("No end date provided, defaulting to today: {Date}", 
                    DateTime.Today);
            }
            
            // Include records up to the specified end date
            var modifiedFilter = new JobWorkFilter
            {
                JobId = filter.JobId,
                JobWorkTypeId = filter.JobWorkTypeId,
                UnitId = filter.UnitId,
                EmployeeId = filter.EmployeeId,
                JobType = filter.JobType,
                SortBy = filter.SortBy,
                SortOrder = filter.SortOrder,
                Page = 1,
                PageSize = int.MaxValue,
                EndDate = filter.EndDate
            };
            
            // Only set StartDate if this is not an "as on" report
            if (!isAsOnReport && filter.StartDate.HasValue)
            {
                modifiedFilter.StartDate = filter.StartDate;
            }
            
            // Get all job works using the modified filter
            var jobWorksResult = await GetJobWorksAsync(modifiedFilter);
            
            if (jobWorksResult?.Data == null || !jobWorksResult.Data.Any())
            {
                _logger.LogWarning("No job works found for summary generation");
                return new JobWorkSummaryDto
                {
                    TotalHours = 0,
                    TotalHoursAmount = 0,
                    TotalQuantity = 0,
                    TotalJobAmount = 0,
                    GrandTotal = 0,
                    TotalRecords = 0,
                    EmployeeSummaries = new List<DTOs.EmployeeSummary>()
                };
            }
            
            // Filter out entries with empty employee names before grouping
            var filteredJobWorks = jobWorksResult.Data.Where(jw => !string.IsNullOrWhiteSpace(jw.EmployeeName)).ToList();
            
            if (!filteredJobWorks.Any())
            {
                _logger.LogWarning("No job works with valid employee names found");
                return new JobWorkSummaryDto
                {
                    TotalHours = 0,
                    TotalHoursAmount = 0,
                    TotalQuantity = 0,
                    TotalJobAmount = 0,
                    GrandTotal = 0,
                    TotalRecords = 0,
                    EmployeeSummaries = new List<DTOs.EmployeeSummary>()
                };
            }
            
            // Group by employee and calculate totals
            var employeeSummaries = filteredJobWorks
                .GroupBy(jw => jw.EmployeeName)
                .Select(group => new DTOs.EmployeeSummary
                {
                    EmployeeName = group.Key,
                    EmployeeId = group.First().EmployeeId?.ToString() ?? string.Empty,
                    Hours = group.Sum(jw => jw.QtyHours ?? 0),
                    HoursAmount = group.Sum(jw => (jw.QtyHours ?? 0) * (jw.RateForJob ?? 0)),
                    Quantity = group.Sum(jw => jw.QtyItems ?? 0),
                    JobAmount = group.Sum(jw => (jw.QtyItems ?? 0) * (jw.RateForJob ?? 0)),
                    Total = group.Sum(jw => (jw.QtyHours ?? 0) * (jw.RateForJob ?? 0)) + group.Sum(jw => (jw.QtyItems ?? 0) * (jw.RateForJob ?? 0)),
                    WorkSummaries = group
                        .GroupBy(jw => jw.WorkName)
                        .Select(workGroup => new DTOs.WorkSummary
                        {
                            WorkName = workGroup.Key,
                            TotalHours = workGroup.Sum(jw => jw.QtyHours),
                            TotalAmount = workGroup.Sum(jw => jw.TotalAmount),
                            TotalItems = workGroup.Sum(jw => jw.QtyItems)
                        })
                        .ToList()
                })
                .OrderBy(es => es.EmployeeName) // Sort by employee name
                .ToList();
                
            // Calculate overall totals
            var totalHours = employeeSummaries.Sum(es => es.Hours);
            var totalHoursAmount = employeeSummaries.Sum(es => es.HoursAmount);
            var totalQuantity = employeeSummaries.Sum(es => es.Quantity);
            var totalJobAmount = employeeSummaries.Sum(es => es.JobAmount);
            var grandTotal = employeeSummaries.Sum(es => es.Total);
            
            _logger.LogInformation("Successfully generated summary with {Count} employee summaries", 
                employeeSummaries.Count);
            
            return new JobWorkSummaryDto
            {
                TotalHours = totalHours,
                TotalHoursAmount = totalHoursAmount,
                TotalQuantity = totalQuantity,
                TotalJobAmount = totalJobAmount,
                GrandTotal = grandTotal,
                TotalRecords = filteredJobWorks.Count,
                EmployeeSummaries = employeeSummaries
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating job work summary: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<byte[]> ExportToExcelAsync(JobWorkFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting ExportToExcelAsync with filter: {@Filter}", 
                System.Text.Json.JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("Excel Export Report type: {ReportType}", 
                isAsOnReport ? "As On Report" : "Date Range Report");
            
            // Parse selected columns if provided
            IEnumerable<string> selectedColumns = new List<string>();
            if (!string.IsNullOrEmpty(filter.Columns))
            {
                selectedColumns = filter.Columns.Split(',').Select(c => c.Trim());
                _logger.LogInformation("Using selected columns: {Columns}", filter.Columns);
            }
            
            // Prepare filter for getJobWorksAsync
            var modifiedFilter = new JobWorkFilter
            {
                EndDate = filter.EndDate,
                JobId = filter.JobId,
                JobWorkTypeId = filter.JobWorkTypeId,
                UnitId = filter.UnitId, 
                EmployeeId = filter.EmployeeId,
                JobType = filter.JobType,
                Page = 1, 
                PageSize = int.MaxValue, 
                SortBy = "employeename",
                SortOrder = "asc",
                Columns = filter.Columns
            };
            
            // Only set StartDate if this is not an "as on" report
            if (!isAsOnReport && filter.StartDate.HasValue)
            {
                modifiedFilter.StartDate = filter.StartDate;
            }
            
            // Get the job works using the GetJobWorksAsync method to ensure consistent filtering
            var response = await GetJobWorksAsync(modifiedFilter);

            // Filter out job works with empty employee names
            var jobWorks = response.Data.Where(jw => !string.IsNullOrWhiteSpace(jw.EmployeeName)).ToList();
            
            if (!jobWorks.Any())
            {
                _logger.LogWarning("No job works with valid employee names found for export");
                // Return a simple Excel file with a message
                using var emptyPackage = new ExcelPackage();
                var emptyWorksheet = emptyPackage.Workbook.Worksheets.Add("Job Works");
                emptyWorksheet.Cells[1, 1].Value = "No data available";
                emptyWorksheet.Cells[1, 1].Style.Font.Bold = true;
                return emptyPackage.GetAsByteArray();
            }
            
            // Create an Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Job Works");

            // Add title
            string titleText;
            if (isAsOnReport)
            {
                titleText = $"JOB WORK SUMMARY AS ON {filter.EndDate:dd-MMM-yyyy}";
            }
            else
            {
                titleText = $"JOB WORK SUMMARY {filter.StartDate:dd-MMM-yyyy} TO {filter.EndDate:dd-MMM-yyyy}";
            }
            
            worksheet.Cells[1, 1].Value = titleText;
            worksheet.Cells[1, 1, 1, 12].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Add page number
            worksheet.Cells[1, 13].Value = "PAGE: 1";
            worksheet.Cells[1, 13].Style.Font.Bold = true;

            // Define all possible headers with their property names
            var allHeaders = new Dictionary<string, string>
            {
                { "unitName", "Unit" },
                { "entryDate", "Date" },
                { "workType", "Type" },
                { "workName", "Job" },
                { "qtyHours", "Hours" },
                { "rateForJob", "Rate/day" },
                { "qtyItems", "Quantity" },
                { "totalAmount", "Amount" },
                { "employeeName", "Employee" },
                { "jwNo", "Job Work No." },
                { "groupName", "Group" }, 
                { "isApproved", "Is Approved" },
                { "remarks", "Remarks" }
            };
            
            // Determine which headers to include
            var headersToInclude = selectedColumns.Any() 
                ? allHeaders.Where(h => selectedColumns.Contains(h.Key)).ToDictionary(h => h.Key, h => h.Value)
                : new Dictionary<string, string>
                {
                    { "unitName", "Unit" },
                    { "entryDate", "Date" },
                    { "workType", "Type" },
                    { "workName", "Job" },
                    { "qtyHours", "Hours" },
                    { "rateForJob", "Rate/day" },
                    { "qtyItems", "Quantity" },
                    { "totalAmount", "Amount" }
                };
                
            // Ensure employeeName is included and make it the first column after Sr. No.
            if (headersToInclude.ContainsKey("employeeName")) {
                var employeeHeader = headersToInclude["employeeName"];
                headersToInclude.Remove("employeeName");
                
                // Create new dictionary with employeeName first, then the rest
                var reorderedHeaders = new Dictionary<string, string>
                {
                    { "employeeName", employeeHeader }
                };
                
                foreach (var header in headersToInclude)
                {
                    reorderedHeaders.Add(header.Key, header.Value);
                }
                
                headersToInclude = reorderedHeaders;
            }
                
            // Add "Sr. No." as the first column
            var headers = new List<string> { "Sr. No." };
            headers.AddRange(headersToInclude.Values);

            // Write headers to Excel
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Add data
            int rowIndex = 4;
            int srNo = 1;
            var groupedData = jobWorks
                .GroupBy(j => new { 
                    EmployeeId = j.EmployeeId ?? "0", 
                    EmployeeName = j.EmployeeName ?? "Unknown"
                })
                .OrderBy(g => g.Key.EmployeeName);

            foreach (var group in groupedData)
            {
                worksheet.Cells[rowIndex, 1].Value = srNo;
                
                // Column index (after Sr. No. column)
                int colIndex = 2;
                
                foreach (var header in headersToInclude)
                {
                    var propertyName = header.Key;
                    
                    // Handle special cases for calculated values
                    if (propertyName == "unitName")
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = group.First().UnitName;
                    }
                    else if (propertyName == "entryDate")
                    {
                        // Group might have multiple entry dates, show the range or the first one
                        var firstEntryDate = group.Min(j => j.EntryDate);
                        worksheet.Cells[rowIndex, colIndex].Value = firstEntryDate;
                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "dd/MM/yyyy";
                    }
                    else if (propertyName == "employeeName")
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = group.Key.EmployeeName;
                    }
                    else if (propertyName == "workType")
                    {
                        // Take the first work type or show "Multiple" if there are different types
                        var workTypes = group.Select(j => j.WorkType).Distinct();
                        worksheet.Cells[rowIndex, colIndex].Value = workTypes.Count() == 1 
                            ? workTypes.First() 
                            : "Multiple";
                    }
                    else if (propertyName == "workName")
                    {
                        // Take the first work name or show "Multiple" if there are different names
                        var workNames = group.Select(j => j.WorkName).Distinct();
                        worksheet.Cells[rowIndex, colIndex].Value = workNames.Count() == 1 
                            ? workNames.First() 
                            : "Multiple Jobs";
                    }
                    else if (propertyName == "qtyHours")
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = group.Sum(j => j.QtyHours ?? 0);
                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "#,##0.00";
                    }
                    else if (propertyName == "rateForJob")
                    {
                        // Calculate average rate or show as is
                        worksheet.Cells[rowIndex, colIndex].Value = group.Average(j => j.RateForJob ?? 0) * 8; // Daily rate
                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "#,##0.00";
                    }
                    else if (propertyName == "qtyItems")
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = group.Sum(j => j.QtyItems ?? 0);
                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "#,##0.00";
                    }
                    else if (propertyName == "totalAmount")
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = group.Sum(j => j.TotalAmount ?? 0);
                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "#,##0.00";
                    }
                    else if (propertyName == "jwNo")
                    {
                        var jwNos = group.Select(j => j.JwNo).Distinct();
                        worksheet.Cells[rowIndex, colIndex].Value = jwNos.Count() == 1 
                            ? jwNos.First() 
                            : "Multiple";
                    }
                    else if (propertyName == "groupName")
                    {
                        var groupNames = group.Select(j => j.GroupName).Distinct();
                        worksheet.Cells[rowIndex, colIndex].Value = groupNames.Count() == 1 
                            ? groupNames.First() 
                            : "Multiple";
                    }
                    else if (propertyName == "isApproved")
                    {
                        var allApproved = group.All(j => j.IsApproved == true);
                        var noneApproved = group.All(j => j.IsApproved == false);
                        worksheet.Cells[rowIndex, colIndex].Value = allApproved ? "Yes" : (noneApproved ? "No" : "Partial");
                    }
                    else if (propertyName == "remarks")
                    {
                        // Take the first remarks or show truncated if there are different remarks
                        var remarks = group.Select(j => j.Remarks).Distinct();
                        worksheet.Cells[rowIndex, colIndex].Value = remarks.Count() == 1 
                            ? remarks.First() 
                            : "Multiple remarks...";
                    }
                    
                    worksheet.Cells[rowIndex, colIndex].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    colIndex++;
                }

                rowIndex++;
                srNo++;
            }

            // Auto-fit columns
            for (int i = 1; i <= headers.Count; i++)
            {
                worksheet.Column(i).AutoFit();
            }

            // Set print area and page setup
            worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
            worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
            worksheet.PrinterSettings.FitToPage = true;

            return package.GetAsByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Excel: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<byte[]> ExportToPdfAsync(JobWorkFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting ExportToPdfAsync with filter: {@Filter}", 
                System.Text.Json.JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("PDF Export Report type: {ReportType}", 
                isAsOnReport ? "As On Report" : "Date Range Report");
            
            // Parse selected columns if provided
            IEnumerable<string> selectedColumns = new List<string>();
            if (!string.IsNullOrEmpty(filter.Columns))
            {
                selectedColumns = filter.Columns.Split(',').Select(c => c.Trim());
                _logger.LogInformation("Using selected columns: {Columns}", filter.Columns);
            }
            
            // Prepare filter for getJobWorksAsync
            var modifiedFilter = new JobWorkFilter
            {
                EndDate = filter.EndDate,
                JobId = filter.JobId,
                JobWorkTypeId = filter.JobWorkTypeId,
                UnitId = filter.UnitId, 
                EmployeeId = filter.EmployeeId,
                JobType = filter.JobType,
                Page = 1, 
                PageSize = int.MaxValue, 
                SortBy = "employeename",
                SortOrder = "asc",
                Columns = filter.Columns
            };
            
            // Only set StartDate if this is not an "as on" report
            if (!isAsOnReport && filter.StartDate.HasValue)
            {
                modifiedFilter.StartDate = filter.StartDate;
            }
            
            // Get the job works using the GetJobWorksAsync method
            var response = await GetJobWorksAsync(modifiedFilter);

            if (response?.Data == null)
            {
                throw new InvalidOperationException("No data available for PDF generation");
            }

            // Filter out job works with empty employee names
            var jobWorks = response.Data.Where(jw => !string.IsNullOrWhiteSpace(jw.EmployeeName)).ToList();
            
            if (!jobWorks.Any())
            {
                _logger.LogWarning("No job works with valid employee names found for export");
                
                // Create a simple PDF with a message
                using var emptyDocument = new Document(PageSize.A4, 50, 50, 50, 50);
                using var emptyMemoryStream = new MemoryStream();
                var emptyWriter = PdfWriter.GetInstance(emptyDocument, emptyMemoryStream);
                emptyDocument.Open();
                var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var paragraph = new Paragraph("No data available", font);
                paragraph.Alignment = Element.ALIGN_CENTER;
                emptyDocument.Add(paragraph);
                emptyDocument.Close();
                return emptyMemoryStream.ToArray();
            }
            
            // Create a PDF document in landscape orientation
            using var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
            using var memoryStream = new MemoryStream();
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            // Add page number event handler
            writer.PageEvent = new PageNumberHandler();
            
            document.Open();
            
            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            string titleText;
            if (isAsOnReport)
            {
                titleText = $"JOB WORK SUMMARY AS ON {filter.EndDate:dd-MMM-yyyy}";
            }
            else
            {
                titleText = $"JOB WORK SUMMARY {filter.StartDate:dd-MMM-yyyy} TO {filter.EndDate:dd-MMM-yyyy}";
            }
            
            var title = new Paragraph(titleText, titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20f;
            document.Add(title);

            // Define all possible columns with their property names
            var allColumns = new Dictionary<string, string>
            {
                { "unitName", "Unit" },
                { "entryDate", "Date" },
                { "workType", "Type" },
                { "workName", "Job" },
                { "qtyHours", "Hours" },
                { "rateForJob", "Rate/day" },
                { "qtyItems", "Quantity" },
                { "totalAmount", "Amount" },
                { "employeeName", "Employee" },
                { "jwNo", "Job Work No." },
                { "groupName", "Group" }, 
                { "isApproved", "Is Approved" },
                { "remarks", "Remarks" }
            };
            
            // Determine which columns to include
            var columnsToInclude = selectedColumns.Any() 
                ? allColumns.Where(c => selectedColumns.Contains(c.Key)).ToDictionary(c => c.Key, c => c.Value)
                : new Dictionary<string, string>
                {
                    { "unitName", "Unit" },
                    { "entryDate", "Date" },
                    { "workType", "Type" },
                    { "workName", "Job" },
                    { "qtyHours", "Hours" },
                    { "rateForJob", "Rate/day" },
                    { "qtyItems", "Quantity" },
                    { "totalAmount", "Amount" }
                };
            
            // Ensure employeeName is included and make it the first column after Sr. No.
            if (columnsToInclude.ContainsKey("employeeName")) {
                var employeeHeader = columnsToInclude["employeeName"];
                columnsToInclude.Remove("employeeName");
                
                // Create new dictionary with employeeName first, then the rest
                var reorderedColumns = new Dictionary<string, string>
                {
                    { "employeeName", employeeHeader }
                };
                
                foreach (var column in columnsToInclude)
                {
                    reorderedColumns.Add(column.Key, column.Value);
                }
                
                columnsToInclude = reorderedColumns;
            }
                
            // Add "Sr. No." as the first column
            var headers = new List<string> { "Sr. No." };
            headers.AddRange(columnsToInclude.Values);
            
            // Create the main table with appropriate number of columns
            var table = new PdfPTable(headers.Count);
            table.WidthPercentage = 100;
            
            // Set column widths based on included columns
            // Default width for Sr. No.
            var widths = new List<float> { 5 };
            
            foreach (var col in columnsToInclude.Keys)
            {
                switch (col)
                {
                    case "employeeName":
                        widths.Add(15);
                        break;
                    case "groupName":
                        widths.Add(10);
                        break;
                    case "unitName":
                        widths.Add(8);
                        break;
                    case "entryDate":
                        widths.Add(8);
                        break;
                    case "workType":
                        widths.Add(8);
                        break;
                    case "workName":
                        widths.Add(15);
                        break;
                    case "qtyHours":
                        widths.Add(8);
                        break;
                    case "rateForJob":
                        widths.Add(8);
                        break;
                    case "qtyItems":
                        widths.Add(8);
                        break;
                    case "totalAmount":
                        widths.Add(10);
                        break;
                    case "jwNo":
                        widths.Add(10);
                        break;
                    case "isApproved":
                        widths.Add(8);
                        break;
                    case "remarks":
                        widths.Add(15);
                        break;
                    default:
                        widths.Add(10);
                        break;
                }
            }
            
            table.SetWidths(widths.ToArray());
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.BackgroundColor = new BaseColor(240, 240, 240);
                cell.PaddingTop = 5f;
                cell.PaddingBottom = 5f;
                cell.MinimumHeight = 25f;
                table.AddCell(cell);
            }

            // Group and sort data by employee
            var groupedData = jobWorks
                .GroupBy(j => new { 
                    EmployeeId = j.EmployeeId ?? "0",
                    EmployeeName = j.EmployeeName ?? "Unknown"
                })
                .OrderBy(g => g.Key.EmployeeName);

            var regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            int srNo = 1;

            // Add data rows
            foreach (var group in groupedData)
            {
                decimal totalHours = group.Sum(j => j.QtyHours ?? 0);
                decimal totalQuantity = group.Sum(j => j.QtyItems ?? 0);
                decimal rowTotal = group.Sum(j => j.TotalAmount ?? 0);

                // Add serial number
                AddCell(table, srNo.ToString(), regularFont, Element.ALIGN_CENTER);
                
                // Add selected columns
                foreach (var column in columnsToInclude.Keys)
                {
                    if (column == "unitName")
                    {
                        AddCell(table, group.First().UnitName ?? "", regularFont, 
                            Element.ALIGN_LEFT);
                    }
                    else if (column == "entryDate")
                    {
                        // Group might have multiple entry dates, show the range or the first one
                        var firstEntryDate = group.Min(j => j.EntryDate);
                        AddCell(table, firstEntryDate?.ToString("dd/MM/yyyy") ?? "", regularFont, 
                            Element.ALIGN_CENTER);
                    }
                    else if (column == "employeeName")
                    {
                        AddCell(table, group.Key.EmployeeName, regularFont, Element.ALIGN_LEFT);
                    }
                    else if (column == "workType")
                    {
                        // Take the first work type or show "Multiple" if there are different types
                        var workTypes = group.Select(j => j.WorkType).Distinct();
                        AddCell(table, workTypes.Count() == 1 
                            ? workTypes.First() ?? ""
                            : "Multiple", regularFont, Element.ALIGN_LEFT);
                    }
                    else if (column == "workName")
                    {
                        // Take the first work name or show "Multiple" if there are different names
                        var workNames = group.Select(j => j.WorkName).Distinct();
                        AddCell(table, workNames.Count() == 1 
                            ? workNames.First() ?? "" 
                            : "Multiple Jobs", regularFont, Element.ALIGN_LEFT);
                    }
                    else if (column == "qtyHours")
                    {
                        AddCell(table, totalHours.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    }
                    else if (column == "rateForJob")
                    {
                        // Calculate average rate or show as is
                        var avgRate = group.Average(j => j.RateForJob ?? 0) * 8; // Daily rate
                        AddCell(table, avgRate.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    }
                    else if (column == "qtyItems")
                    {
                        AddCell(table, totalQuantity.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    }
                    else if (column == "totalAmount")
                    {
                        AddCell(table, rowTotal.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    }
                    else if (column == "jwNo")
                    {
                        var jwNos = group.Select(j => j.JwNo).Distinct();
                        AddCell(table, jwNos.Count() == 1 
                            ? jwNos.First() ?? "" 
                            : "Multiple", regularFont, Element.ALIGN_LEFT);
                    }
                    else if (column == "groupName")
                    {
                        var groupNames = group.Select(j => j.GroupName).Distinct();
                        AddCell(table, groupNames.Count() == 1 
                            ? groupNames.First() ?? "" 
                            : "Multiple", regularFont, Element.ALIGN_LEFT);
                    }
                    else if (column == "isApproved")
                    {
                        var allApproved = group.All(j => j.IsApproved == true);
                        var noneApproved = group.All(j => j.IsApproved == false);
                        AddCell(table, allApproved ? "Yes" : (noneApproved ? "No" : "Partial"), 
                            regularFont, Element.ALIGN_CENTER);
                    }
                    else if (column == "remarks")
                    {
                        // Take the first remarks or show truncated if there are different remarks
                        var remarks = group.Select(j => j.Remarks).Distinct();
                        AddCell(table, remarks.Count() == 1 
                            ? remarks.First() ?? ""
                            : "Multiple remarks...", regularFont, Element.ALIGN_LEFT);
                    }
                }

                srNo++;
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to PDF: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<byte[]> ExportSummaryToPdfAsync(JobWorkFilter filter)
    {
        _logger.LogInformation("Starting ExportSummaryToPdfAsync with filter: {@Filter}", filter);
        
        bool isAsOnReport = filter.StartDate == null && filter.EndDate != null;
        _logger.LogInformation(isAsOnReport ? "Generating As On Report" : "Generating Date Range Report");
        
        if (filter.EndDate == null)
        {
            filter.EndDate = DateTime.Today;
            _logger.LogInformation("No end date provided, defaulting to today: {EndDate}", filter.EndDate);
        }
        
        var modifiedFilter = new JobWorkFilter
        {
            EndDate = filter.EndDate,
            Columns = filter.Columns
        };
        
        // Only add StartDate if it's not an "As On" report
        if (!isAsOnReport && filter.StartDate.HasValue)
        {
            modifiedFilter.StartDate = filter.StartDate;
            _logger.LogInformation("Using date range from {StartDate} to {EndDate}", 
                modifiedFilter.StartDate, modifiedFilter.EndDate);
        }
        else
        {
            _logger.LogInformation("As On Report for date: {EndDate}", modifiedFilter.EndDate);
        }
        
        var summaryData = await GetJobWorkSummaryAsync(modifiedFilter);
        var reportTitle = isAsOnReport
            ? $"Job Work Summary As On {filter.EndDate:dd/MM/yyyy}"
            : $"Job Work Summary From {filter.StartDate:dd/MM/yyyy} To {filter.EndDate:dd/MM/yyyy}";
        
        List<string> selectedColumnIds = new List<string>();
        if (!string.IsNullOrEmpty(filter.Columns))
        {
            try
            {
                // Check if the columns are JSON format or comma-separated
                if (filter.Columns.StartsWith("["))
                {
                    selectedColumnIds = JsonSerializer.Deserialize<List<string>>(filter.Columns);
                }
                else
                {
                    // Handle comma-separated format
                    selectedColumnIds = filter.Columns.Split(',').Select(c => c.Trim()).ToList();
                }
                
                _logger.LogInformation("Original selected columns: {SelectedColumns}", string.Join(", ", selectedColumnIds));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse selected columns, defaulting to all columns");
                selectedColumnIds = new List<string>();
            }
        }
        
        // Map frontend column IDs to backend column IDs
        var columnMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "unit", "unitName" },
            { "date", "entryDate" },
            { "type", "workType" },
            { "job", "workName" },
            { "hours", "qtyHours" },
            { "rate/day", "rateForJob" },
            { "quantity", "qtyItems" },
            { "amount", "totalAmount" },
            { "employee", "employeeName" }
        };
        
        // Map the selected columns to backend column IDs
        var mappedColumnIds = selectedColumnIds.Select(id => 
            columnMappings.TryGetValue(id, out var mappedId) ? mappedId : id).ToList();
        
        _logger.LogInformation("Mapped selected columns: {MappedColumns}", string.Join(", ", mappedColumnIds));
        
        // If no columns selected, use default columns
        if (mappedColumnIds.Count == 0)
        {
            mappedColumnIds = new List<string>
            {
                "employeeName", "qtyHours", "totalAmount", "qtyItems"
            };
            _logger.LogInformation("No columns selected, using default columns");
        }
        
        // Always include employee name and ensure it's the first column
        if (mappedColumnIds.Contains("employeeName"))
        {
            // Remove it from its current position
            mappedColumnIds.Remove("employeeName");
        }
        // Add it at the beginning
        mappedColumnIds.Insert(0, "employeeName");
        _logger.LogInformation("Ensuring employeeName column is first for identification");
        
        using (var ms = new MemoryStream())
        {
            try
            {
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 50f, 10f);
                var writer = PdfWriter.GetInstance(document, ms);
                
                document.Open();
                
                // Add logo if available
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                if (File.Exists(logoPath))
                {
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(100f, 100f);
                    logo.Alignment = Element.ALIGN_LEFT;
                    document.Add(logo);
                    document.Add(new Paragraph(" "));
                }
                
                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph(reportTitle, titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(" "));
                
                if (summaryData.EmployeeSummaries.Any())
                {
                    // Column definitions with their corresponding display names and widths
                    var columnDefinitions = new Dictionary<string, (string DisplayName, float Width)>
                    {
                        { "employeeName", ("Employee", 3f) },
                        { "unitName", ("Unit", 1.5f) },
                        { "entryDate", ("Date", 1.5f) },
                        { "workType", ("Type", 1.5f) },
                        { "workName", ("Job", 2f) },
                        { "qtyHours", ("Hours", 1.5f) },
                        { "rateForJob", ("Rate/day", 1.5f) },
                        { "qtyItems", ("Quantity", 1.5f) },
                        { "totalAmount", ("Amount", 2f) },
                        { "hoursAmount", ("Hours Amount", 2f) },
                        { "jobAmount", ("Job Amount", 2f) }
                    };
                    
                    _logger.LogInformation("Using columns for PDF: {Columns}", 
                        string.Join(", ", mappedColumnIds.Select(id => 
                            columnDefinitions.ContainsKey(id) ? columnDefinitions[id].DisplayName : id)));
                    
                    // Calculate total relative width of selected columns
                    float totalWidth = mappedColumnIds.Sum(id => 
                        columnDefinitions.ContainsKey(id) ? columnDefinitions[id].Width : 1f);
                    
                    // Create PDF table with the correct number of columns
                    var table = new PdfPTable(mappedColumnIds.Count);
                    table.WidthPercentage = 100;
                    
                    // Set relative column widths
                    float[] columnWidths = mappedColumnIds.Select(id => 
                        columnDefinitions.ContainsKey(id) ? columnDefinitions[id].Width / totalWidth : 1f / totalWidth).ToArray();
                    
                    table.SetWidths(columnWidths);
                    
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                    
                    // Add headers based on selected columns
                    foreach (var columnId in mappedColumnIds)
                    {
                        if (columnDefinitions.ContainsKey(columnId))
                        {
                            AddHeaderCell(table, columnDefinitions[columnId].DisplayName, headerFont);
                        }
                        else
                        {
                            // Convert to Title Case for display
                            var headerText = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(columnId);
                            AddHeaderCell(table, headerText, headerFont);
                        }
                    }
                    
                    // Add data rows
                    foreach (var employee in summaryData.EmployeeSummaries.Where(e => !string.IsNullOrEmpty(e.EmployeeName)))
                    {
                        foreach (var columnId in mappedColumnIds)
                        {
                            switch (columnId.ToLowerInvariant())
                            {
                                case "employeename":
                                    AddCell(table, employee.EmployeeName, cellFont, Element.ALIGN_LEFT);
                                    break;
                                case "qtyhours":
                                    AddCell(table, employee.Hours.ToString("F2"), cellFont, Element.ALIGN_RIGHT);
                                    break;
                                case "totalamount":
                                    AddCell(table, employee.Total.ToString("C2"), cellFont, Element.ALIGN_RIGHT);
                                    break;
                                case "qtyitems":
                                    AddCell(table, employee.Quantity.ToString("F2"), cellFont, Element.ALIGN_RIGHT);
                                    break;
                                case "hoursamount":
                                    AddCell(table, employee.HoursAmount.ToString("C2"), cellFont, Element.ALIGN_RIGHT);
                                    break;
                                case "jobamount":
                                    AddCell(table, employee.JobAmount.ToString("C2"), cellFont, Element.ALIGN_RIGHT);
                                    break;
                                // For summary fields with aggregated values
                                case "unitname":
                                case "entrydate":
                                case "worktype":
                                case "workname":
                                case "rateforjob":
                                case "jwno":
                                case "groupname":
                                case "isapproved":
                                case "remarks":
                                    AddCell(table, "---", cellFont, Element.ALIGN_CENTER);
                                    break;
                                default:
                                    AddCell(table, "", cellFont, Element.ALIGN_CENTER);
                                    break;
                            }
                        }
                    }
                    
                    // Add totals row
                    foreach (var columnId in mappedColumnIds)
                    {
                        switch (columnId.ToLowerInvariant())
                        {
                            case "employeename":
                                AddCell(table, "Total", headerFont, Element.ALIGN_LEFT);
                                break;
                            case "qtyhours":
                                AddCell(table, summaryData.TotalHours.ToString("F2"), headerFont, Element.ALIGN_RIGHT);
                                break;
                            case "totalamount":
                                AddCell(table, summaryData.GrandTotal.ToString("C2"), headerFont, Element.ALIGN_RIGHT);
                                break;
                            case "qtyitems":
                                AddCell(table, summaryData.TotalQuantity.ToString("F2"), headerFont, Element.ALIGN_RIGHT);
                                break;
                            case "hoursamount":
                                AddCell(table, summaryData.TotalHoursAmount.ToString("C2"), headerFont, Element.ALIGN_RIGHT);
                                break;
                            case "jobamount":
                                AddCell(table, summaryData.TotalJobAmount.ToString("C2"), headerFont, Element.ALIGN_RIGHT);
                                break;
                            default:
                                AddCell(table, "", headerFont, Element.ALIGN_CENTER);
                                break;
                        }
                    }
                    
                    document.Add(table);
                }
                else
                {
                    var noDataFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var noDataParagraph = new Paragraph("No data available for the selected criteria.", noDataFont);
                    noDataParagraph.Alignment = Element.ALIGN_CENTER;
                    document.Add(noDataParagraph);
                }
                
                document.Close();
                writer.Close();
                
                _logger.LogInformation("Successfully generated PDF summary report");
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF summary report");
                throw;
            }
        }
    }
    
    private void AddHeaderCell(PdfPTable table, string text, Font font)
    {
        var cell = new PdfPCell(new Phrase(text, font));
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        cell.BackgroundColor = new BaseColor(240, 240, 240);
        cell.Padding = 3;
        cell.MinimumHeight = 18f;
        table.AddCell(cell);
    }

    private void AddCell(PdfPTable table, string text, Font font, int alignment, int colspan = 1)
    {
        var cell = new PdfPCell(new Phrase(text, font));
        cell.HorizontalAlignment = alignment;
        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
        cell.Padding = 3;
        cell.Colspan = colspan;
        table.AddCell(cell);
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

// Page number handler class
public class PageNumberHandler : PdfPageEventHelper
{
    public override void OnEndPage(PdfWriter writer, Document document)
    {
        var font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var pageNumber = writer.PageNumber;
        var text = $"PAGE: {pageNumber}";
        
        ColumnText.ShowTextAligned(
            writer.DirectContent,
            Element.ALIGN_RIGHT,
            new Phrase(text, font),
            document.PageSize.Width - document.RightMargin,
            document.PageSize.Height - document.TopMargin + 10,
            0
        );
    }
} 