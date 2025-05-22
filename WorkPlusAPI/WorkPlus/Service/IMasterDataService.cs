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

        #region Users
        Task<IEnumerable<UserDTO>> GetUsersAsync();
        Task<UserDTO> GetUserAsync(int id);
        Task<UserDTO> CreateUserAsync(UserDTO userDto);
        Task<bool> UpdateUserAsync(UserDTO userDto);
        Task<bool> DeleteUserAsync(int id);
        #endregion

        #region Roles
        Task<IEnumerable<RoleDTO>> GetRolesAsync();
        Task<UserRoleDTO> GetUserRolesAsync(int userId);
        Task<bool> AssignUserRolesAsync(UserRoleAssignmentDTO assignmentDto);
        #endregion
    }
} 