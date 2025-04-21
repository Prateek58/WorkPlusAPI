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
        public string? JwNo { get; set; }
        public decimal? QtyItems { get; set; }
        public decimal? QtyHours { get; set; }
        public decimal? RateForJob { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool? IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? EntryByUserId { get; set; }
        
        // Employee data
        public string? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        
        // Unit data
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        
        // Work data
        public long? WorkId { get; set; }
        public string? WorkName { get; set; }
        
        // Work type data
        public string? WorkTypeId { get; set; }
        public string? WorkTypeName { get; set; }
        
        // Group data
        public string? GroupId { get; set; }
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

            // Build query - this is just defining the query, not executing it yet
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
                        select new 
                        {
                            entry.EntryId,
                            entry.EntryDate,
                            entry.JwNo,
                            entry.QtyItems,
                            entry.QtyHours,
                            entry.RateForJob,
                            entry.TotalAmount,
                            entry.IsApproved,
                            entry.ApprovedBy,
                            entry.ApprovedOn,
                            entry.EntryByUserId,
                            entry.EmployeeId,
                            EmployeeName = employee != null ? (employee.FirstName + " " + employee.LastName).Trim() : string.Empty,
                            entry.UnitId,
                            UnitName = unit != null ? unit.UnitName : string.Empty,
                            entry.WorkId,
                            WorkName = work != null ? work.WorkName : string.Empty,
                            WorkTypeId = work != null ? work.WorkTypeId : null,
                            WorkTypeName = workType != null ? workType.TypeName : string.Empty,
                            GroupId = work != null ? work.GroupId : null,
                            GroupName = workGroup != null ? workGroup.GroupName : string.Empty
                        };

            // Apply filters - still just building the query, not executing
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

            // Apply other filters - still just building the query
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

            // Count query - execute the query but only get count
            var total = await query.CountAsync();
            _logger.LogInformation("Total records before pagination: {Total}", total);

            // Apply pagination and finally materialize the data with ToListAsync
            var pageSize = filter.PageSize ?? 10;
            var page = filter.Page ?? 1;
            
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
                JwNo = Convert.ToString(x.JwNo),
                WorkName = x.WorkName,
                EmployeeId = x.EmployeeId.ToString(),
                EmployeeName = x.EmployeeName,
                UnitName = x.UnitName,
                WorkType = x.WorkTypeName,
                GroupName = x.GroupName,
                QtyItems = x.QtyItems,
                QtyHours = x.QtyHours,
                RateForJob = x.RateForJob,
                TotalAmount = x.TotalAmount,
                IsApproved = x.IsApproved,
                ApprovedBy = Convert.ToString(x.ApprovedBy),
                ApprovedOn = x.ApprovedOn,
                EntryByUserId = Convert.ToString(x.EntryByUserId),
                WorkId = x.WorkId.HasValue ? x.WorkId.ToString() : null,
                Remarks = x.WorkName
            }).ToList();

            _logger.LogInformation("Successfully mapped {0} job works to DTOs", jobWorkDtos.Count);

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
            _logger.LogInformation("Starting GetJobWorkSummaryAsync with filter: {0}", 
                JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("Summary Report type: {0}", 
                isAsOnReport ? "As On Report" : "Date Range Report");

            // Default to today for both dates if not provided
            if (!filter.StartDate.HasValue && !filter.EndDate.HasValue)
            {
                filter.EndDate = DateTime.Today;
                isAsOnReport = true; // Treat this as an "as on" report
                _logger.LogInformation("No dates provided, defaulting to As On Report for today ({0})", 
                    DateTime.Today);
            }
            else if (!filter.EndDate.HasValue)
            {
                filter.EndDate = DateTime.Today;
                _logger.LogInformation("No end date provided, defaulting to today ({0})", 
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
                    EmployeeSummaries = new List<EmployeeSummary>()
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
                    EmployeeSummaries = new List<EmployeeSummary>()
                };
            }
            
            // Group by employee and calculate totals
            var employeeSummaries = filteredJobWorks
                .GroupBy(jw => jw.EmployeeName)
                .Select(group => new EmployeeSummary
                {
                    EmployeeName = group.Key,
                    EmployeeId = group.First().EmployeeId, // Add employee ID from the first record in the group
                    Hours = group.Sum(jw => jw.QtyHours) ?? 0,
                    HoursAmount = group.Sum(jw => (jw.QtyHours ?? 0) * (jw.RateForJob ?? 0)),
                    Quantity = group.Sum(jw => jw.QtyItems) ?? 0,
                    JobAmount = group.Sum(jw => (jw.QtyItems ?? 0) * (jw.RateForJob ?? 0)),
                    Total = group.Sum(jw => (jw.QtyHours ?? 0) * (jw.RateForJob ?? 0)) + group.Sum(jw => (jw.QtyItems ?? 0) * (jw.RateForJob ?? 0)),
                    WorkSummaries = group
                        .GroupBy(jw => jw.WorkName)
                        .Select(workGroup => new WorkSummary
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
            
            _logger.LogInformation("Successfully generated summary with {0} employee summaries", 
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
            _logger.LogInformation("Starting ExportToExcelAsync with filter: {0}", 
                System.Text.Json.JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("Excel Export Report type: {0}", 
                isAsOnReport ? "As On Report" : "Date Range Report");
            
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
                SortOrder = "asc"
            };
            
            // Only set StartDate if this is not an "as on" report
            if (!isAsOnReport && filter.StartDate.HasValue)
            {
                modifiedFilter.StartDate = filter.StartDate;
            }
            
            // Get the job works using the GetJobWorksAsync method to ensure consistent filtering
            var response = await GetJobWorksAsync(modifiedFilter);

            var jobWorks = response.Data;
            
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

            // Add headers (row 3)
            var headers = new[]
            {
                "Sr. No.",
                "EMP. No.",
                "NAME",
                "HRS",
                "Total HRS Amt",
                "Qty",
                "Total Job Amt",
                "Total"
            };

            for (int i = 0; i < headers.Length; i++)
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
                worksheet.Cells[rowIndex, 2].Value = group.Key.EmployeeId;
                worksheet.Cells[rowIndex, 3].Value = group.Key.EmployeeName;
                
                decimal totalHours = group.Sum(j => j.QtyHours ?? 0);
                decimal totalHoursAmount = group.Sum(j => (j.QtyHours ?? 0) * (j.RateForJob ?? 0));
                decimal totalQuantity = group.Sum(j => j.QtyItems ?? 0);
                decimal totalJobAmount = group.Sum(j => (j.QtyItems ?? 0) * (j.RateForJob ?? 0));
                decimal grandTotal = totalHoursAmount + totalJobAmount;

                worksheet.Cells[rowIndex, 4].Value = totalHours;
                worksheet.Cells[rowIndex, 5].Value = totalHoursAmount;
                worksheet.Cells[rowIndex, 6].Value = totalQuantity;
                worksheet.Cells[rowIndex, 7].Value = totalJobAmount;
                worksheet.Cells[rowIndex, 8].Value = grandTotal;

                // Format numeric columns
                worksheet.Cells[rowIndex, 4].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0.00";

                // Add borders
                for (int i = 1; i <= 8; i++)
                {
                    worksheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                rowIndex++;
                srNo++;
            }

            // Add totals row
            worksheet.Cells[rowIndex, 1].Value = "Total";
            worksheet.Cells[rowIndex, 1, rowIndex, 3].Merge = true;
            worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
            worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Calculate and add totals
            var totals = jobWorks.GroupBy(x => 1).Select(g => new
            {
                TotalHours = g.Sum(j => j.QtyHours ?? 0),
                TotalHoursAmount = g.Sum(j => (j.QtyHours ?? 0) * (j.RateForJob ?? 0)),
                TotalQuantity = g.Sum(j => j.QtyItems ?? 0),
                TotalJobAmount = g.Sum(j => (j.QtyItems ?? 0) * (j.RateForJob ?? 0)),
                GrandTotal = g.Sum(j => ((j.QtyHours ?? 0) * (j.RateForJob ?? 0)) + ((j.QtyItems ?? 0) * (j.RateForJob ?? 0)))
            }).First();

            worksheet.Cells[rowIndex, 4].Value = totals.TotalHours;
            worksheet.Cells[rowIndex, 5].Value = totals.TotalHoursAmount;
            worksheet.Cells[rowIndex, 6].Value = totals.TotalQuantity;
            worksheet.Cells[rowIndex, 7].Value = totals.TotalJobAmount;
            worksheet.Cells[rowIndex, 8].Value = totals.GrandTotal;

            // Format totals row
            for (int i = 4; i <= 8; i++)
            {
                worksheet.Cells[rowIndex, i].Style.Font.Bold = true;
                worksheet.Cells[rowIndex, i].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowIndex, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Add a separate total row
            rowIndex++;
            worksheet.Cells[rowIndex, 1, rowIndex, 7].Merge = true;
            worksheet.Cells[rowIndex, 8].Value = $"Total: {totals.GrandTotal:N2}";
            worksheet.Cells[rowIndex, 8].Style.Font.Bold = true;
            worksheet.Cells[rowIndex, 8].Style.Numberformat.Format = "#,##0.00";
            worksheet.Cells[rowIndex, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[rowIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            // Auto-fit columns
            for (int i = 1; i <= 8; i++)
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
            _logger.LogInformation("Starting ExportToPdfAsync with filter: {0}", 
                System.Text.Json.JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("PDF Export Report type: {0}", 
                isAsOnReport ? "As On Report" : "Date Range Report");
            
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
                SortOrder = "asc"
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

            var jobWorks = response.Data;
            
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

            // Create the main table
            var table = new PdfPTable(8);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 5, 8, 20, 10, 12, 10, 12, 12 });
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            string[] headers = { "Sr. No.", "EMP. No.", "NAME", "HRS", "Total HRS Amt", "Qty", "Total Job Amt", "Total" };
            
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
            decimal totalHoursAll = 0;
            decimal totalHoursAmtAll = 0;
            decimal totalQtyAll = 0;
            decimal totalJobAmtAll = 0;
            decimal grandTotalAll = 0;

            // Add data rows
            foreach (var group in groupedData)
            {
                decimal totalHours = group.Sum(j => j.QtyHours ?? 0);
                decimal totalHoursAmount = group.Sum(j => (j.QtyHours ?? 0) * (j.RateForJob ?? 0));
                decimal totalQuantity = group.Sum(j => j.QtyItems ?? 0);
                decimal totalJobAmount = group.Sum(j => (j.QtyItems ?? 0) * (j.RateForJob ?? 0));
                decimal rowTotal = totalHoursAmount + totalJobAmount;

                // Update running totals
                totalHoursAll += totalHours;
                totalHoursAmtAll += totalHoursAmount;
                totalQtyAll += totalQuantity;
                totalJobAmtAll += totalJobAmount;
                grandTotalAll += rowTotal;

                // Add row data
                AddCell(table, srNo.ToString(), regularFont, Element.ALIGN_CENTER);
                AddCell(table, group.Key.EmployeeId, regularFont, Element.ALIGN_CENTER);
                AddCell(table, group.Key.EmployeeName, regularFont, Element.ALIGN_LEFT);
                AddCell(table, totalHours.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                AddCell(table, totalHoursAmount.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                AddCell(table, totalQuantity.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                AddCell(table, totalJobAmount.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                AddCell(table, rowTotal.ToString("N2"), regularFont, Element.ALIGN_RIGHT);

                srNo++;
            }

            // Add total row
            var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            AddCell(table, "Total", totalFont, Element.ALIGN_CENTER, colspan: 3);
            AddCell(table, totalHoursAll.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
            AddCell(table, totalHoursAmtAll.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
            AddCell(table, totalQtyAll.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
            AddCell(table, totalJobAmtAll.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
            AddCell(table, grandTotalAll.ToString("N2"), totalFont, Element.ALIGN_RIGHT);

            // Add a separate total row with all cells merged except the last one
            AddCell(table, "", totalFont, Element.ALIGN_RIGHT, colspan: 7);
            AddCell(table, $"Total: {grandTotalAll:N2}", totalFont, Element.ALIGN_RIGHT);

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
        try
        {
            _logger.LogInformation("Starting ExportSummaryToPdfAsync with filter: {0}", 
                System.Text.Json.JsonSerializer.Serialize(filter));
            
            // Check if this is an "as on" report (only end date provided)
            bool isAsOnReport = !filter.StartDate.HasValue && filter.EndDate.HasValue;
            
            _logger.LogInformation("Summary PDF Export Report type: {0}", 
                isAsOnReport ? "As On Report" : "Date Range Report");
            
            // Default to today for end date if not provided
            if (!filter.EndDate.HasValue)
            {
                filter.EndDate = DateTime.Today;
                _logger.LogInformation("No end date provided, defaulting to today ({0})", 
                    DateTime.Today);
            }
        
            // Get the job works summary - this already handles filtering out empty employee names
            var summary = await GetJobWorkSummaryAsync(filter);
            
            if (summary == null)
            {
                _logger.LogWarning("No summary data available for export");
                throw new InvalidOperationException("No data available for PDF summary generation");
            }
            
            if (summary.EmployeeSummaries == null || !summary.EmployeeSummaries.Any())
            {
                _logger.LogWarning("No employee summary data available for export");
                throw new InvalidOperationException("No employee data available for PDF summary generation");
            }
            
            // Create a PDF document in landscape orientation
            using var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
            using var memoryStream = new MemoryStream();
            
            try
            {
                var writer = PdfWriter.GetInstance(document, memoryStream);
                
                // Add page number event handler
                writer.PageEvent = new PageNumberHandler();
                
                document.Open();
                
                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var title = new Paragraph($"JOB WORK SUMMARY REPORT", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                
                // Add date range subtitle
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                string dateRangeText = string.Empty;
                
                try 
                {
                    if (isAsOnReport)
                    {
                        // For "as on" reports
                        dateRangeText = $"AS ON {filter.EndDate:dd-MMM-yyyy}";
                    }
                    else if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        // For date range reports
                        dateRangeText = $"Period: {filter.StartDate:dd-MMM-yyyy} TO {filter.EndDate:dd-MMM-yyyy}";
                    }
                    else
                    {
                        // Fallback
                        dateRangeText = $"AS ON {DateTime.Today:dd-MMM-yyyy}";
                    }
                }
                catch 
                {
                    // Fallback to a simple date format if formatting fails
                    dateRangeText = $"Period: {filter.StartDate?.ToString("yyyy-MM-dd") ?? "N/A"} TO {filter.EndDate?.ToString("yyyy-MM-dd") ?? "N/A"}";
                }
                
                var subtitle = new Paragraph(dateRangeText, subtitleFont);
                subtitle.Alignment = Element.ALIGN_CENTER;
                subtitle.SpacingAfter = 20f;
                document.Add(subtitle);
                
                // Create the main table
                var table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 5, 8, 20, 10, 12, 10, 12, 12 });
                table.SpacingBefore = 10f;
                table.SpacingAfter = 10f;

                // Add headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                string[] headers = { "Sr. No.", "EMP. No.", "NAME", "HRS", "Total HRS Amt", "Qty", "Total Job Amt", "Total" };
                
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

                var regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                int srNo = 1;

                // Add data rows
                foreach (var employeeSummary in summary.EmployeeSummaries)
                {
                    // Add row data
                    AddCell(table, srNo.ToString(), regularFont, Element.ALIGN_CENTER);
                    AddCell(table, employeeSummary.EmployeeId ?? "N/A", regularFont, Element.ALIGN_CENTER);
                    AddCell(table, employeeSummary.EmployeeName ?? "Unknown", regularFont, Element.ALIGN_LEFT);
                    AddCell(table, employeeSummary.Hours.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    AddCell(table, employeeSummary.HoursAmount.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    AddCell(table, employeeSummary.Quantity.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    AddCell(table, employeeSummary.JobAmount.ToString("N2"), regularFont, Element.ALIGN_RIGHT);
                    AddCell(table, employeeSummary.Total.ToString("N2"), regularFont, Element.ALIGN_RIGHT);

                    srNo++;
                }

                // Add total row
                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                AddCell(table, "Total", totalFont, Element.ALIGN_CENTER, colspan: 3);
                AddCell(table, summary.TotalHours.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
                AddCell(table, summary.TotalHoursAmount.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
                AddCell(table, summary.TotalQuantity.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
                AddCell(table, summary.TotalJobAmount.ToString("N2"), totalFont, Element.ALIGN_RIGHT);
                AddCell(table, summary.GrandTotal.ToString("N2"), totalFont, Element.ALIGN_RIGHT);

                // Add a separate total row with all cells merged except the last one
                AddCell(table, "", totalFont, Element.ALIGN_RIGHT, colspan: 7);
                AddCell(table, $"Total: {summary.GrandTotal:N2}", totalFont, Element.ALIGN_RIGHT);

                document.Add(table);
                
                // Add footer with totals
                var footerTable = new PdfPTable(2);
                footerTable.WidthPercentage = 50;
                footerTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                footerTable.SpacingBefore = 20f;
                
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                
                // Add total records
                var cell1 = new PdfPCell(new Phrase("Total Records:", footerFont));
                cell1.Border = Rectangle.NO_BORDER;
                cell1.HorizontalAlignment = Element.ALIGN_LEFT;
                footerTable.AddCell(cell1);
                
                var cell2 = new PdfPCell(new Phrase(summary.TotalRecords.ToString(), footerFont));
                cell2.Border = Rectangle.NO_BORDER;
                cell2.HorizontalAlignment = Element.ALIGN_RIGHT;
                footerTable.AddCell(cell2);
                
                document.Add(footerTable);
                
                // Add generation timestamp
                var timestampFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var timestamp = new Paragraph($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}", timestampFont);
                timestamp.Alignment = Element.ALIGN_CENTER;
                timestamp.SpacingBefore = 10f;
                document.Add(timestamp);
                
                document.Close();
                
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PDF document: {Message}", ex.Message);
                
                // Ensure document is closed
                if (document.IsOpen())
                {
                    document.Close();
                }
                
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting summary to PDF: {Message}", ex.Message);
            throw;
        }
    }

    private void AddCell(PdfPTable table, string text, Font font, int alignment, int colspan = 1)
    {
        var cell = new PdfPCell(new Phrase(text, font))
        {
            HorizontalAlignment = alignment,
            VerticalAlignment = Element.ALIGN_MIDDLE,
            Padding = 5,
            Colspan = colspan
        };
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