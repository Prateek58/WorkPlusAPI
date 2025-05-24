using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.DTOs.WorkPlusReportsDTOs
{
    public class JobEntryReportDTO
    {
        public int EntryId { get; set; }
        public string JobName { get; set; }
        public string WorkerName { get; set; }
        public string GroupName { get; set; }
        public string EntryType { get; set; }
        public decimal? ExpectedHours { get; set; }
        public decimal? HoursTaken { get; set; }
        public int? ItemsCompleted { get; set; }
        public decimal? RatePerJob { get; set; }
        public decimal? ProductiveHours { get; set; }
        public decimal? ExtraHours { get; set; }
        public decimal? UnderperformanceHours { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool IsPostLunch { get; set; }
        public string Remarks { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class PaginatedJobEntryReportDTO
    {
        public List<JobEntryReportDTO> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class JobEntryFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EntryType { get; set; }
        public int? JobId { get; set; }
        public int? WorkerId { get; set; }
        public int? GroupId { get; set; }
        public bool? IsPostLunch { get; set; }
        public string? Columns { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class JobOptionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string JobType { get; set; }
    }

    public class WorkerOptionDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string WorkerId { get; set; }
    }

    public class GroupOptionDTO
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
    }

    public class JobEntryFilterOptionsDTO
    {
        public List<JobOptionDTO> Jobs { get; set; } = new List<JobOptionDTO>();
        public List<WorkerOptionDTO> Workers { get; set; } = new List<WorkerOptionDTO>();
        public List<GroupOptionDTO> Groups { get; set; } = new List<GroupOptionDTO>();
        public List<string> EntryTypes { get; set; } = new List<string>();
    }

    public class ExportRequest
    {
        public JobEntryFilter Filter { get; set; } = new JobEntryFilter();
        public List<string> SelectedColumns { get; set; } = new List<string>();
        public string ExportType { get; set; } = "excel"; // "excel", "csv", "pdf"
    }

    public class ColumnDefinition
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = true;
        public string DataType { get; set; } = "string"; // "string", "number", "date", "currency"
    }

    public class ExportColumnsDTO
    {
        public List<ColumnDefinition> AvailableColumns { get; set; } = new List<ColumnDefinition>();
    }
} 