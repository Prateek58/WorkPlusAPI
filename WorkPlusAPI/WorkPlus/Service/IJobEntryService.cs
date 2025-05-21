using System.Collections.Generic;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Model;

namespace WorkPlusAPI.WorkPlus.Service
{
    public interface IJobEntryService
    {
        Task<JobEntryMasterDataDTO> GetJobEntryMasterDataAsync();
        Task<JobEntry> CreateJobEntryAsync(JobEntry jobEntry);
        Task<IEnumerable<JobEntryDTO>> GetAllJobEntriesAsync();
        Task<JobEntryDTO> GetJobEntryAsync(int id);
        Task<bool> DeleteJobEntryAsync(int id);
        Task<(IEnumerable<JobEntryDTO> Items, int TotalCount)> GetPaginatedJobEntriesAsync(int pageNumber, int pageSize);
    }
} 