using WorkPlusAPI.DTOs;

namespace WorkPlusAPI.Services;

public interface IJobWorkService
{
    Task<IEnumerable<UnitDto>> GetUnitsAsync();
    Task<IEnumerable<JobWorkTypeDto>> GetJobWorkTypesAsync();
    Task<IEnumerable<JobDto>> GetJobsAsync(bool isGroup = false);
    Task<IEnumerable<JobWorkDto>> GetJobWorksAsync(JobWorkFilter filter);
    Task<JobWorkSummaryDto> GetJobWorkSummaryAsync(JobWorkFilter filter);
    Task<byte[]> ExportToExcelAsync(JobWorkFilter filter);
    Task<byte[]> ExportToPdfAsync(JobWorkFilter filter);
    Task<IEnumerable<EmployeeDto>> SearchEmployeesAsync(string searchTerm);
} 