using WorkPlusAPI.Archive.DTOs.Archive;

namespace WorkPlusAPI.Archive.Services.Archive;

public interface IJobWorkService
{
    Task<IEnumerable<UnitDto>> GetUnitsAsync();
    Task<IEnumerable<JobWorkTypeDto>> GetJobWorkTypesAsync();
    Task<IEnumerable<JobDto>> GetJobsAsync(bool isGroup = false);
    Task<JobWorkResponse> GetJobWorksAsync(JobWorkFilter filter);
    Task<JobWorkSummaryDto> GetJobWorkSummaryAsync(JobWorkFilter filter);
    Task<byte[]> ExportToExcelAsync(JobWorkFilter filter);
    Task<byte[]> ExportToPdfAsync(JobWorkFilter filter);
    Task<byte[]> ExportSummaryToPdfAsync(JobWorkFilter filter);
    Task<IEnumerable<EmployeeDto>> SearchEmployeesAsync(string searchTerm);
}