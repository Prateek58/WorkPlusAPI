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
} 