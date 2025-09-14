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
        Task<RoleDTO> GetRoleAsync(int id);
        #endregion

        #region JobGroups
        Task<IEnumerable<JobGroupDTO>> GetJobGroupsAsync();
        Task<JobGroupDTO> GetJobGroupAsync(int id);
        Task<JobGroupDTO> CreateJobGroupAsync(JobGroupDTO jobGroupDto);
        Task<bool> UpdateJobGroupAsync(JobGroupDTO jobGroupDto);
        Task<bool> DeleteJobGroupAsync(int id);
        #endregion

        #region GroupMembers
        Task<IEnumerable<GroupMemberDTO>> GetGroupMembersAsync();
        Task<IEnumerable<GroupMemberDTO>> GetGroupMembersByGroupAsync(int groupId);
        Task<GroupMemberDTO> GetGroupMemberAsync(int id);
        Task<GroupMemberDTO> CreateGroupMemberAsync(GroupMemberCreateDTO groupMemberDto);
        Task<List<GroupMemberDTO>> CreateGroupMembersBulkAsync(GroupMemberBulkCreateDTO bulkGroupMemberDto);
        Task<bool> DeleteGroupMemberAsync(int id);
        #endregion

        #region Jobs
        Task<IEnumerable<JobDTO>> GetJobsAsync();
        Task<JobDTO> GetJobAsync(int id);
        Task<JobDTO> CreateJobAsync(JobCreateDTO jobDto);
        Task<bool> UpdateJobAsync(JobDTO jobDto);
        Task<bool> DeleteJobAsync(int id);
        Task<IEnumerable<dynamic>> GetJobTypesAsync();
        #endregion

        #region JobTypes
        Task<IEnumerable<JobTypeDTO>> GetAllJobTypesAsync();
        Task<JobTypeDTO> GetJobTypeAsync(int id);
        #endregion

        #region EmployeeTypes
        Task<IEnumerable<EmployeeTypeDTO>> GetEmployeeTypesAsync();
        Task<EmployeeTypeDTO> GetEmployeeTypeAsync(int id);
        #endregion
    }
}