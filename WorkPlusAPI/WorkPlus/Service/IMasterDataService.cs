using System.Collections.Generic;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs;

namespace WorkPlusAPI.WorkPlus.Service
{
    public interface IMasterDataService
    {
        #region Workers
        Task<IEnumerable<WorkerDTO>> GetWorkersAsync();
        Task<WorkerDTO> GetWorkerAsync(int id);
        Task<WorkerDTO> CreateWorkerAsync(WorkerDTO workerDto);
        Task<bool> UpdateWorkerAsync(WorkerDTO workerDto);
        Task<bool> DeleteWorkerAsync(int id);
        #endregion
    }
} 