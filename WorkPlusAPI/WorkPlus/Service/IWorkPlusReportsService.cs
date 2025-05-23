using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs.WorkPlusReportsDTOs;

namespace WorkPlusAPI.WorkPlus.Service
{
    public interface IWorkPlusReportsService
    {
        Task<PaginatedJobEntryReportDTO> GetPaginatedJobEntriesReportAsync(int pageNumber, int pageSize);
    }
} 