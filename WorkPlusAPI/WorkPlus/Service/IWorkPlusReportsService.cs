using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs.WorkPlusReportsDTOs;

namespace WorkPlusAPI.WorkPlus.Service
{
    public interface IWorkPlusReportsService
    {
        Task<PaginatedJobEntryReportDTO> GetPaginatedJobEntriesReportAsync(int pageNumber, int pageSize);
        Task<PaginatedJobEntryReportDTO> GetFilteredJobEntriesReportAsync(JobEntryFilter filter);
        Task<JobEntryFilterOptionsDTO> GetJobEntryFilterOptionsAsync();
        Task<ExportColumnsDTO> GetExportColumnsAsync();
        Task<byte[]> ExportJobEntriesAsync(ExportRequest request);
        Task<List<JobEntryReportDTO>> GetAllFilteredJobEntriesAsync(JobEntryFilter filter);
    }
} 