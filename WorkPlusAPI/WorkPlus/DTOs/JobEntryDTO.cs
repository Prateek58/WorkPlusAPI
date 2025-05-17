using System.Collections.Generic;
using System;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class WorkerDTO
    {
        public int WorkerId { get; set; }
        public string FullName { get; set; }
    }

    public class JobDTO
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public decimal? RatePerItem { get; set; }
        public decimal? RatePerHour { get; set; }
        public decimal? ExpectedHours { get; set; }
        public int? ExpectedItemsPerHour { get; set; }
        public decimal? IncentiveBonusRate { get; set; }
        public decimal? PenaltyRate { get; set; }
        public string IncentiveType { get; set; }
    }

    public class JobGroupDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class JobEntryMasterDataDTO
    {
        public List<WorkerDTO> Workers { get; set; } = new List<WorkerDTO>();
        public List<JobDTO> Jobs { get; set; } = new List<JobDTO>();
        public List<JobGroupDTO> JobGroups { get; set; } = new List<JobGroupDTO>();
    }

    public class JobEntryDTO
    {
        public int EntryId { get; set; }
        public int JobId { get; set; }
        public string JobName { get; set; }
        public string EntryType { get; set; }
        public int? WorkerId { get; set; }
        public string WorkerName { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsPostLunch { get; set; }
        public int? ItemsCompleted { get; set; }
        public decimal? HoursTaken { get; set; }
        public decimal RatePerJob { get; set; }
        public decimal? ExpectedHours { get; set; }
        public decimal? ProductiveHours { get; set; }
        public decimal? ExtraHours { get; set; }
        public decimal? UnderperformanceHours { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Remarks { get; set; }
        public bool IsFinalized { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateJobEntryDTO
    {
        public int JobId { get; set; }
        public string EntryType { get; set; }
        public int? WorkerId { get; set; }
        public int? GroupId { get; set; }
        public bool IsPostLunch { get; set; }
        public int? ItemsCompleted { get; set; }
        public decimal? HoursTaken { get; set; }
        public decimal RatePerJob { get; set; }
        public decimal? ExpectedHours { get; set; }
        public string Remarks { get; set; }
        public DateTime? EntryDate { get; set; }
    }
} 