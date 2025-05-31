using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkPlusAPI.WorkPlus.DTOs.HRDTOs;

public class AttendanceDTO
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string Shift { get; set; } = "Full Day";
    public string Status { get; set; } = "Present";
    public string? HalfDayType { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public string? Remarks { get; set; }
    public int MarkedBy { get; set; }
    public string? MarkedByName { get; set; }
    public DateTime MarkedAt { get; set; }
}

public class CreateAttendanceDTO
{
    public int WorkerId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string? CheckInTime { get; set; }
    public string? CheckOutTime { get; set; }
    public string Status { get; set; } = "Present";
    public string? HalfDayType { get; set; }
    public string? Remarks { get; set; }
    // MarkedBy will be set from authenticated user in controller
}

public class BulkAttendanceDTO
{
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = "Present";
    public string? HalfDayType { get; set; }
    public int[] WorkerIds { get; set; } = Array.Empty<int>();
    public string? Remarks { get; set; }
    // MarkedBy will be set from authenticated user in controller
}

public class AttendanceReportDTO
{
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public int CompOffDays { get; set; }
    public int TotalMarkedDays { get; set; }
}

public class BulkAttendanceResultDTO
{
    public IEnumerable<AttendanceDTO> ProcessedAttendance { get; set; } = new List<AttendanceDTO>();
    public IEnumerable<string> SkippedWorkers { get; set; } = new List<string>();
    public string Message { get; set; } = string.Empty;
    public bool HasWarnings => SkippedWorkers.Any();
} 