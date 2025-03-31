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

            // Start with a query that joins all necessary tables
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
                            // Entry data
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
                            
                            // Employee data
                            entry.EmployeeId,
                            EmployeeName = employee != null ? (employee.FirstName + " " + employee.LastName).Trim() : string.Empty,
                            
                            // Unit data
                            entry.UnitId,
                            UnitName = unit != null ? unit.UnitName : string.Empty,
                            
                            // Work data
                            entry.WorkId,
                            WorkName = work != null ? work.WorkName : string.Empty,
                            
                            // Work type data
                            WorkTypeId = work != null ? (work.WorkTypeId != null ? work.WorkTypeId.ToString() : null) : null,
                            WorkTypeName = workType != null ? workType.TypeName : string.Empty,
                            
                            // Group data
                            GroupId = work != null ? (work.GroupId != null ? work.GroupId.ToString() : null) : null,
                            GroupName = workGroup != null ? workGroup.GroupName : string.Empty
                        };

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                _logger.LogInformation("Filtering by start date: {StartDate}", filter.StartDate.Value);
                query = query.Where(x => x.EntryDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                _logger.LogInformation("Filtering by end date: {EndDate}", filter.EndDate.Value);
                query = query.Where(x => x.EntryDate <= filter.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.JobId))
            {
                _logger.LogInformation("Filtering by job ID: {JobId}", filter.JobId);
                query = query.Where(x => x.WorkId.ToString() == filter.JobId);
            }

            if (!string.IsNullOrEmpty(filter.JobWorkTypeId))
            {
                _logger.LogInformation("Filtering by work type ID: {WorkTypeId}", filter.JobWorkTypeId);
                query = query.Where(x => x.WorkTypeId == filter.JobWorkTypeId);
            }

            if (!string.IsNullOrEmpty(filter.UnitId))
            {
                _logger.LogInformation("Filtering by unit ID: {UnitId}", filter.UnitId);
                // Convert UnitId to string for comparison
                var unitIdString = filter.UnitId;
                query = query.Where(x => x.UnitId.ToString() == unitIdString);
            }

            if (!string.IsNullOrEmpty(filter.EmployeeId))
            {
                _logger.LogInformation("Filtering by employee ID: {EmployeeId}", filter.EmployeeId);
                // Convert EmployeeId to string for comparison
                var employeeIdString = filter.EmployeeId;
                query = query.Where(x => x.EmployeeId.ToString() == employeeIdString);
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

            // Apply sorting
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

            // Get total count
            var total = (int)await query.CountAsync();

            // Apply pagination
            var pageSize = filter.PageSize ?? 10;
            var page = filter.Page ?? 1;
            
            var entries = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to response DTOs
            var jobWorkDtos = entries.Select(x => new JobWorkDto
            {
                EntryId = x.EntryId,
                EntryDate = x.EntryDate,
                JwNo = Convert.ToString(x.JwNo),
                WorkName = x.WorkName,
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

            _logger.LogInformation($"Successfully retrieved {jobWorkDtos.Count} job works");

            return new JobWorkResponse
            {
                Data = jobWorkDtos.Cast<IJobWorkDto>().ToList(),
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
            // Start with a query from JwEntries
            var query = _context.JwEntries.AsQueryable();

            // Apply the same filters as in GetJobWorksAsync
            if (filter.StartDate.HasValue)
            {
                query = query.Where(je => je.EntryDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(je => je.EntryDate <= filter.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.JobId))
            {
                if (int.TryParse(filter.JobId, out int jobId))
                {
                    // For JobId filter, we need to join with works to check the GroupId
                    var worksWithGroupId = _context.JwWorks
                        .Where(w => w.GroupId == jobId)
                        .Select(w => w.WorkId);
                    
                    query = query.Where(je => je.WorkId.HasValue && worksWithGroupId.Contains(je.WorkId.Value));
                }
            }

            if (!string.IsNullOrEmpty(filter.JobWorkTypeId))
            {
                if (sbyte.TryParse(filter.JobWorkTypeId, out sbyte workTypeId))
                {
                    // For WorkTypeId filter, we need to join with works
                    var worksWithType = _context.JwWorks
                        .Where(w => w.WorkTypeId == workTypeId)
                        .Select(w => w.WorkId);
                    
                    query = query.Where(je => je.WorkId.HasValue && worksWithType.Contains(je.WorkId.Value));
                }
            }

            if (!string.IsNullOrEmpty(filter.UnitId))
            {
                if (byte.TryParse(filter.UnitId, out byte unitId))
                {
                    query = query.Where(je => je.UnitId == unitId);
                }
            }

            if (!string.IsNullOrEmpty(filter.EmployeeId))
            {
                if (int.TryParse(filter.EmployeeId, out int employeeId))
                {
                    query = query.Where(je => je.EmployeeId == employeeId);
                }
            }

            if (!string.IsNullOrEmpty(filter.JobType))
            {
                if (filter.JobType == "group")
                {
                    var groupWorkIds = _context.JwWorks
                        .Where(w => w.GroupId != null)
                        .Select(w => w.WorkId);
                    
                    query = query.Where(je => je.WorkId.HasValue && groupWorkIds.Contains(je.WorkId.Value));
                }
                else if (filter.JobType == "work")
                {
                    var nonGroupWorkIds = _context.JwWorks
                        .Where(w => w.GroupId == null)
                        .Select(w => w.WorkId);
                    
                    query = query.Where(je => je.WorkId.HasValue && nonGroupWorkIds.Contains(je.WorkId.Value));
                }
            }

            // Calculate the summary values
            var summary = await query
                .GroupBy(_ => 1) // Group all records together for aggregation
                .Select(g => new JobWorkSummaryDto
                {
                    TotalHours = g.Sum(je => je.QtyHours ?? 0),
                    TotalQuantity = g.Sum(je => je.QtyItems ?? 0),
                    TotalAmount = g.Sum(je => je.TotalAmount ?? 0),
                    TotalRecords = g.Count()
                })
                .FirstOrDefaultAsync() ?? new JobWorkSummaryDto
                {
                    TotalHours = 0,
                    TotalQuantity = 0,
                    TotalAmount = 0,
                    TotalRecords = 0
                };
            
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job work summary: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<byte[]> ExportToExcelAsync(JobWorkFilter filter)
    {
        try
        {
            // Get the job works using the GetJobWorksAsync method to ensure consistent filtering
            var response = await GetJobWorksAsync(new JobWorkFilter
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                JobId = filter.JobId,
                JobWorkTypeId = filter.JobWorkTypeId,
                UnitId = filter.UnitId, 
                EmployeeId = filter.EmployeeId,
                JobType = filter.JobType,
                Page = 1, 
                PageSize = int.MaxValue, 
                SortBy = "entrydate",
                SortOrder = "desc"
            });

            var jobWorks = response.Data;
            
            // Create an Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Job Works");
            
            // Add headers
            worksheet.Cells[1, 1].Value = "Entry ID";
            worksheet.Cells[1, 2].Value = "JW No";
            worksheet.Cells[1, 3].Value = "Date";
            worksheet.Cells[1, 4].Value = "Work";
            worksheet.Cells[1, 5].Value = "Work Type";
            worksheet.Cells[1, 6].Value = "Group";
            worksheet.Cells[1, 7].Value = "Employee";
            worksheet.Cells[1, 8].Value = "Hours";
            worksheet.Cells[1, 9].Value = "Quantity";
            worksheet.Cells[1, 10].Value = "Rate";
            worksheet.Cells[1, 11].Value = "Unit";
            worksheet.Cells[1, 12].Value = "Amount";
            worksheet.Cells[1, 13].Value = "Status";
            worksheet.Cells[1, 14].Value = "Remarks";

            // Format headers
            for (int i = 1; i <= 14; i++)
            {
                worksheet.Cells[1, i].Style.Font.Bold = true;
                worksheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Add data
            int itemCount = jobWorks.Count;
            for (int i = 0; i < itemCount; i++)
            {
                var job = jobWorks[i];
                int row = i + 2; // Start from the second row (after headers)
                
                worksheet.Cells[row, 1].Value = job.EntryId;
                worksheet.Cells[row, 2].Value = job.JwNo;
                worksheet.Cells[row, 3].Value = job.EntryDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 4].Value = job.WorkName;
                worksheet.Cells[row, 5].Value = job.WorkType;
                worksheet.Cells[row, 6].Value = job.GroupName;
                worksheet.Cells[row, 7].Value = job.EmployeeName;
                worksheet.Cells[row, 8].Value = job.QtyHours;
                worksheet.Cells[row, 9].Value = job.QtyItems;
                worksheet.Cells[row, 10].Value = job.RateForJob;
                worksheet.Cells[row, 11].Value = job.UnitName;
                worksheet.Cells[row, 12].Value = job.TotalAmount;
                worksheet.Cells[row, 13].Value = job.IsApproved == true ? "Approved" : "Pending";
                worksheet.Cells[row, 14].Value = job.Remarks;
                
                // Format numeric columns
                worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 10].Style.Numberformat.Format = "₹#,##0.00";
                worksheet.Cells[row, 12].Style.Numberformat.Format = "₹#,##0.00";
            }

            // Auto-fit columns
            for (int i = 1; i <= 14; i++)
            {
                worksheet.Column(i).AutoFit();
            }

            // Return as byte array
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
            // Get the job works using the GetJobWorksAsync method to ensure consistent filtering
            var response = await GetJobWorksAsync(new JobWorkFilter
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                JobId = filter.JobId,
                JobWorkTypeId = filter.JobWorkTypeId,
                UnitId = filter.UnitId, 
                EmployeeId = filter.EmployeeId,
                JobType = filter.JobType,
                Page = 1, 
                PageSize = int.MaxValue, 
                SortBy = "entrydate",
                SortOrder = "desc"
            });

            var jobWorks = response.Data;
            
            // Create a summary for the report
            var summary = await GetJobWorkSummaryAsync(filter);
            
            // Create a PDF document
            using var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            using var memoryStream = new MemoryStream();
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Add title
            var titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 16);
            var title = new iTextSharp.text.Paragraph("Job Works Report", titleFont);
            title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new iTextSharp.text.Paragraph(" "));
            
            // Add date filter information
            var infoFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10);
            var dateInfo = new iTextSharp.text.Paragraph($"Date Range: {filter.StartDate?.ToString("dd/MM/yyyy") ?? "All"} to {filter.EndDate?.ToString("dd/MM/yyyy") ?? "All"}", infoFont);
            document.Add(dateInfo);
            document.Add(new iTextSharp.text.Paragraph(" "));
            
            // Add summary table
            var summaryTable = new iTextSharp.text.pdf.PdfPTable(4);
            summaryTable.WidthPercentage = 100;
            
            // Add summary headers
            var headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 10);
            var regularFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 10);
            
            // Add headers with correct Phrase usage
            var headerCell1 = new iTextSharp.text.pdf.PdfPCell();
            headerCell1.AddElement(new iTextSharp.text.Phrase("Total Hours", headerFont));
            summaryTable.AddCell(headerCell1);
            
            var headerCell2 = new iTextSharp.text.pdf.PdfPCell();
            headerCell2.AddElement(new iTextSharp.text.Phrase("Total Quantity", headerFont));
            summaryTable.AddCell(headerCell2);
            
            var headerCell3 = new iTextSharp.text.pdf.PdfPCell();
            headerCell3.AddElement(new iTextSharp.text.Phrase("Total Amount", headerFont));
            summaryTable.AddCell(headerCell3);
            
            var headerCell4 = new iTextSharp.text.pdf.PdfPCell();
            headerCell4.AddElement(new iTextSharp.text.Phrase("Total Records", headerFont));
            summaryTable.AddCell(headerCell4);
            
            // Add summary data
            var dataCell1 = new iTextSharp.text.pdf.PdfPCell();
            dataCell1.AddElement(new iTextSharp.text.Phrase($"{summary.TotalHours:N2}", regularFont));
            summaryTable.AddCell(dataCell1);
            
            var dataCell2 = new iTextSharp.text.pdf.PdfPCell();
            dataCell2.AddElement(new iTextSharp.text.Phrase($"{summary.TotalQuantity:N2}", regularFont));
            summaryTable.AddCell(dataCell2);
            
            var dataCell3 = new iTextSharp.text.pdf.PdfPCell();
            dataCell3.AddElement(new iTextSharp.text.Phrase($"₹{summary.TotalAmount:N2}", regularFont));
            summaryTable.AddCell(dataCell3);
            
            var dataCell4 = new iTextSharp.text.pdf.PdfPCell();
            dataCell4.AddElement(new iTextSharp.text.Phrase($"{summary.TotalRecords}", regularFont));
            summaryTable.AddCell(dataCell4);
            
            document.Add(summaryTable);
            document.Add(new iTextSharp.text.Paragraph(" "));
            
            // Add main table
            var table = new iTextSharp.text.pdf.PdfPTable(12);
            table.WidthPercentage = 100;
            
            // Add headers
            string[] headers = new[] {
                "Entry ID", "JW No", "Date", "Work", "Work Type", "Group", "Employee", 
                "Hours", "Quantity", "Rate", "Unit", "Amount"
            };
            
            foreach (var header in headers)
            {
                var cell = new iTextSharp.text.pdf.PdfPCell();
                cell.BackgroundColor = new iTextSharp.text.BaseColor(220, 220, 220);
                cell.AddElement(new iTextSharp.text.Phrase(header, headerFont));
                table.AddCell(cell);
            }
            
            // Add data
            int itemCount = jobWorks.Count;
            for (int i = 0; i < itemCount; i++)
            {
                var job = jobWorks[i];
                int row = i + 2; // Start from the second row (after headers)
                
                var cell1 = new iTextSharp.text.pdf.PdfPCell();
                cell1.AddElement(new iTextSharp.text.Phrase($"{job.EntryId}", regularFont));
                table.AddCell(cell1);
                
                var cell2 = new iTextSharp.text.pdf.PdfPCell();
                cell2.AddElement(new iTextSharp.text.Phrase(job.JwNo ?? "", regularFont));
                table.AddCell(cell2);
                
                var cell3 = new iTextSharp.text.pdf.PdfPCell();
                cell3.AddElement(new iTextSharp.text.Phrase(job.EntryDate?.ToString("dd/MM/yyyy") ?? "", regularFont));
                table.AddCell(cell3);
                
                var cell4 = new iTextSharp.text.pdf.PdfPCell();
                cell4.AddElement(new iTextSharp.text.Phrase(job.WorkName ?? "", regularFont));
                table.AddCell(cell4);
                
                var cell5 = new iTextSharp.text.pdf.PdfPCell();
                cell5.AddElement(new iTextSharp.text.Phrase(job.WorkType ?? "", regularFont));
                table.AddCell(cell5);
                
                var cell6 = new iTextSharp.text.pdf.PdfPCell();
                cell6.AddElement(new iTextSharp.text.Phrase(job.GroupName ?? "", regularFont));
                table.AddCell(cell6);
                
                var cell7 = new iTextSharp.text.pdf.PdfPCell();
                cell7.AddElement(new iTextSharp.text.Phrase(job.EmployeeName ?? "", regularFont));
                table.AddCell(cell7);
                
                var cell8 = new iTextSharp.text.pdf.PdfPCell();
                cell8.AddElement(new iTextSharp.text.Phrase($"{job.QtyHours:N2}", regularFont));
                table.AddCell(cell8);
                
                var cell9 = new iTextSharp.text.pdf.PdfPCell();
                cell9.AddElement(new iTextSharp.text.Phrase($"{job.QtyItems:N2}", regularFont));
                table.AddCell(cell9);
                
                var cell10 = new iTextSharp.text.pdf.PdfPCell();
                cell10.AddElement(new iTextSharp.text.Phrase($"₹{job.RateForJob:N2}", regularFont));
                table.AddCell(cell10);
                
                var cell11 = new iTextSharp.text.pdf.PdfPCell();
                cell11.AddElement(new iTextSharp.text.Phrase(job.UnitName ?? "", regularFont));
                table.AddCell(cell11);
                
                var cell12 = new iTextSharp.text.pdf.PdfPCell();
                cell12.AddElement(new iTextSharp.text.Phrase($"₹{job.TotalAmount:N2}", regularFont));
                table.AddCell(cell12);
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