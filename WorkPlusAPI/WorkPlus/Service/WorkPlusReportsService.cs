using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs.WorkPlusReportsDTOs;
using OfficeOpenXml;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
            // Use the new filtered method with empty filter
            var filter = new JobEntryFilter
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return await GetFilteredJobEntriesReportAsync(filter);
        }

        public async Task<PaginatedJobEntryReportDTO> GetFilteredJobEntriesReportAsync(JobEntryFilter filter)
        {
            // Ensure valid pagination parameters
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            // Calculate skip value for pagination
            int skip = (filter.PageNumber - 1) * filter.PageSize;

            // Build the query with filters
            var query = _context.JobEntries
                .Include(je => je.Job)
                .Include(je => je.Worker)
                .Include(je => je.Group)
                .AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                query = query.Where(je => je.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                // Add one day to include the entire end date
                var endDateInclusive = filter.EndDate.Value.AddDays(1);
                query = query.Where(je => je.CreatedAt < endDateInclusive);
            }

            if (!string.IsNullOrEmpty(filter.EntryType))
            {
                query = query.Where(je => je.EntryType == filter.EntryType);
            }

            if (filter.JobId.HasValue)
            {
                query = query.Where(je => je.JobId == filter.JobId.Value);
            }

            if (filter.WorkerId.HasValue)
            {
                query = query.Where(je => je.WorkerId == filter.WorkerId.Value);
            }

            if (filter.GroupId.HasValue)
            {
                query = query.Where(je => je.GroupId == filter.GroupId.Value);
            }

            if (filter.IsPostLunch.HasValue)
            {
                query = query.Where(je => je.IsPostLunch == filter.IsPostLunch.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and select
            var entries = await query
                .OrderByDescending(je => je.CreatedAt)
                .Skip(skip)
                .Take(filter.PageSize)
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

        public async Task<List<JobEntryReportDTO>> GetAllFilteredJobEntriesAsync(JobEntryFilter filter)
        {
            // Build the query with filters (same as above but without pagination)
            var query = _context.JobEntries
                .Include(je => je.Job)
                .Include(je => je.Worker)
                .Include(je => je.Group)
                .AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                query = query.Where(je => je.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                var endDateInclusive = filter.EndDate.Value.AddDays(1);
                query = query.Where(je => je.CreatedAt < endDateInclusive);
            }

            if (!string.IsNullOrEmpty(filter.EntryType))
            {
                query = query.Where(je => je.EntryType == filter.EntryType);
            }

            if (filter.JobId.HasValue)
            {
                query = query.Where(je => je.JobId == filter.JobId.Value);
            }

            if (filter.WorkerId.HasValue)
            {
                query = query.Where(je => je.WorkerId == filter.WorkerId.Value);
            }

            if (filter.GroupId.HasValue)
            {
                query = query.Where(je => je.GroupId == filter.GroupId.Value);
            }

            if (filter.IsPostLunch.HasValue)
            {
                query = query.Where(je => je.IsPostLunch == filter.IsPostLunch.Value);
            }

            // Get all entries without pagination
            var entries = await query
                .OrderByDescending(je => je.CreatedAt)
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

            return entries;
        }

        public async Task<JobEntryFilterOptionsDTO> GetJobEntryFilterOptionsAsync()
        {
            // Get all jobs
            var jobs = await _context.Jobs
                .Include(j => j.JobType)
                .Select(j => new JobOptionDTO
                {
                    Id = j.JobId,
                    Name = j.JobName,
                    JobType = j.JobType.TypeName
                })
                .OrderBy(j => j.Name)
                .ToListAsync();

            // Get all workers
            var workers = await _context.Workers
                .Where(w => w.IsActive == true) // Only active workers
                .Select(w => new WorkerOptionDTO
                {
                    Id = w.WorkerId,
                    FullName = w.FullName,
                    WorkerId = w.WorkerId.ToString()
                })
                .OrderBy(w => w.FullName)
                .ToListAsync();

            // Get all groups
            var groups = await _context.JobGroups
                .Select(g => new GroupOptionDTO
                {
                    Id = g.GroupId,
                    GroupName = g.GroupName
                })
                .OrderBy(g => g.GroupName)
                .ToListAsync();

            // Get distinct entry types
            var entryTypes = await _context.JobEntries
                .Where(je => !string.IsNullOrEmpty(je.EntryType))
                .Select(je => je.EntryType)
                .Distinct()
                .OrderBy(et => et)
                .ToListAsync();

            return new JobEntryFilterOptionsDTO
            {
                Jobs = jobs,
                Workers = workers,
                Groups = groups,
                EntryTypes = entryTypes
            };
        }

        public async Task<ExportColumnsDTO> GetExportColumnsAsync()
        {
            return await Task.FromResult(new ExportColumnsDTO
            {
                AvailableColumns = new List<ColumnDefinition>
                {
                    new ColumnDefinition { Key = "entryId", Label = "ID", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "createdAt", Label = "Date", IsDefault = true, DataType = "date" },
                    new ColumnDefinition { Key = "entryType", Label = "Entry Type", IsDefault = true, DataType = "string" },
                    new ColumnDefinition { Key = "workerName", Label = "Worker Name", IsDefault = true, DataType = "string" },
                    new ColumnDefinition { Key = "groupName", Label = "Group Name", IsDefault = true, DataType = "string" },
                    new ColumnDefinition { Key = "jobName", Label = "Job Name", IsDefault = true, DataType = "string" },
                    new ColumnDefinition { Key = "expectedHours", Label = "Expected Hours", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "hoursTaken", Label = "Hours Taken", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "itemsCompleted", Label = "Items Completed", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "ratePerJob", Label = "Rate (Per Hour/Item)", IsDefault = true, DataType = "currency" },
                    new ColumnDefinition { Key = "productiveHours", Label = "Productive Hours", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "extraHours", Label = "Extra Hours", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "underperformanceHours", Label = "Underperformance Hours", IsDefault = true, DataType = "number" },
                    new ColumnDefinition { Key = "incentiveAmount", Label = "Incentive Amount", IsDefault = true, DataType = "currency" },
                    new ColumnDefinition { Key = "totalAmount", Label = "Total Amount", IsDefault = true, DataType = "currency" },
                    new ColumnDefinition { Key = "isPostLunch", Label = "Shift", IsDefault = true, DataType = "string" },
                    new ColumnDefinition { Key = "remarks", Label = "Remarks", IsDefault = false, DataType = "string" }
                }
            });
        }

        public async Task<byte[]> ExportJobEntriesAsync(ExportRequest request)
        {
            // Get filtered data
            var data = await GetAllFilteredJobEntriesAsync(request.Filter);
            
            // Get selected columns or use all available columns if none selected
            var availableColumns = (await GetExportColumnsAsync()).AvailableColumns;
            var selectedColumns = request.SelectedColumns?.Any() == true 
                ? availableColumns.Where(c => request.SelectedColumns.Contains(c.Key)).ToList()
                : availableColumns.Where(c => c.IsDefault).ToList();

            switch (request.ExportType.ToLower())
            {
                case "excel":
                    return await GenerateExcelAsync(data, selectedColumns);
                case "csv":
                    return await GenerateCsvAsync(data, selectedColumns);
                case "pdf":
                    return await GeneratePdfAsync(data, selectedColumns);
                default:
                    throw new ArgumentException("Invalid export type. Supported types: excel, csv, pdf");
            }
        }

        private async Task<byte[]> GenerateExcelAsync(List<JobEntryReportDTO> data, List<ColumnDefinition> columns)
        {
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Job Entries Report");

                // Add headers
                for (int i = 0; i < columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = columns[i].Label;
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    var row = data[i];
                    for (int j = 0; j < columns.Count; j++)
                    {
                        var value = GetColumnValue(row, columns[j].Key);
                        worksheet.Cells[i + 2, j + 1].Value = value;
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            });
        }

        private async Task<byte[]> GenerateCsvAsync(List<JobEntryReportDTO> data, List<ColumnDefinition> columns)
        {
            return await Task.Run(() =>
            {
                var csv = new StringBuilder();

                // Add headers
                csv.AppendLine(string.Join(",", columns.Select(c => $"\"{c.Label}\"")));

                // Add data
                foreach (var row in data)
                {
                    var values = columns.Select(c => 
                    {
                        var value = GetColumnValue(row, c.Key);
                        return $"\"{(value?.ToString() ?? "").Replace("\"", "\"\"")}\"";
                    });
                    csv.AppendLine(string.Join(",", values));
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            });
        }

        private async Task<byte[]> GeneratePdfAsync(List<JobEntryReportDTO> data, List<ColumnDefinition> columns)
        {
            return await Task.Run(() =>
            {
                using var stream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 0f);
                PdfWriter.GetInstance(document, stream);

                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph("Job Entries Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                document.Add(title);

                // Table
                var table = new PdfPTable(columns.Count)
                {
                    WidthPercentage = 100
                };

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                foreach (var column in columns)
                {
                    var cell = new PdfPCell(new Phrase(column.Label, headerFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                foreach (var row in data)
                {
                    foreach (var column in columns)
                    {
                        var value = GetColumnValue(row, column.Key);
                        var cell = new PdfPCell(new Phrase(value?.ToString() ?? "", dataFont))
                        {
                            Padding = 3
                        };
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();

                return stream.ToArray();
            });
        }

        private object? GetColumnValue(JobEntryReportDTO row, string columnKey)
        {
            return columnKey switch
            {
                "entryId" => row.EntryId,
                "createdAt" => row.CreatedAt?.ToString("MM/dd/yyyy"),
                "entryType" => row.EntryType,
                "workerName" => row.WorkerName,
                "groupName" => row.GroupName,
                "jobName" => row.JobName,
                "expectedHours" => row.ExpectedHours?.ToString("F2"),
                "hoursTaken" => row.HoursTaken?.ToString("F2"),
                "itemsCompleted" => row.ItemsCompleted,
                "ratePerJob" => row.RatePerJob?.ToString("C"),
                "productiveHours" => row.ProductiveHours?.ToString("F2"),
                "extraHours" => row.ExtraHours?.ToString("F2"),
                "underperformanceHours" => row.UnderperformanceHours?.ToString("F2"),
                "incentiveAmount" => row.IncentiveAmount?.ToString("C"),
                "totalAmount" => row.TotalAmount?.ToString("C"),
                "isPostLunch" => row.IsPostLunch ? "Afternoon/Evening" : "Morning",
                "remarks" => row.Remarks,
                _ => null
            };
        }
    }
} 