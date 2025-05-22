using Microsoft.AspNetCore.Mvc;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Service;

namespace WorkPlus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly ILogger<MasterDataController> _logger;
        private readonly IMasterDataService _masterDataService;

        public MasterDataController(ILogger<MasterDataController> logger, IMasterDataService masterDataService)
        {
            _logger = logger;
            _masterDataService = masterDataService;
        }

        #region Workers
        [HttpGet("workers")]
        public async Task<ActionResult<IEnumerable<WorkerDTO>>> GetWorkers()
        {
            try
            {
                var workers = await _masterDataService.GetWorkersAsync();
                return Ok(workers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workers");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("workers/{id}")]
        public async Task<ActionResult<WorkerDTO>> GetWorker(int id)
        {
            try
            {
                var worker = await _masterDataService.GetWorkerAsync(id);
                if (worker == null)
                    return NotFound();

                return Ok(worker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting worker {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("workers")]
        public async Task<ActionResult<WorkerDTO>> CreateWorker(WorkerDTO worker)
        {
            try
            {
                var createdWorker = await _masterDataService.CreateWorkerAsync(worker);
                return CreatedAtAction(nameof(GetWorker), new { id = createdWorker.WorkerId }, createdWorker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating worker");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("workers/{id}")]
        public async Task<IActionResult> UpdateWorker(int id, WorkerDTO worker)
        {
            try
            {
                if (id != worker.WorkerId)
                    return BadRequest();

                var success = await _masterDataService.UpdateWorkerAsync(worker);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating worker {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("workers/{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            try
            {
                var success = await _masterDataService.DeleteWorkerAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting worker {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            try
            {
                var users = await _masterDataService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            try
            {
                var user = await _masterDataService.GetUserAsync(id);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("users")]
        public async Task<ActionResult<UserDTO>> CreateUser(UserDTO user)
        {
            try
            {
                var createdUser = await _masterDataService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDTO user)
        {
            try
            {
                if (id != user.Id)
                    return BadRequest();

                var success = await _masterDataService.UpdateUserAsync(user);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var success = await _masterDataService.DeleteUserAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Roles
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            try
            {
                var roles = await _masterDataService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("users/{userId}/roles")]
        public async Task<ActionResult<UserRoleDTO>> GetUserRoles(int userId)
        {
            try
            {
                var userRoles = await _masterDataService.GetUserRolesAsync(userId);
                return Ok(userRoles);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignUserRoles(int userId, UserRoleAssignmentDTO assignmentDto)
        {
            try
            {
                if (userId != assignmentDto.UserId)
                    return BadRequest("User ID mismatch");

                var success = await _masterDataService.AssignUserRolesAsync(assignmentDto);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region JobGroups
        [HttpGet("job-groups")]
        public async Task<ActionResult<IEnumerable<JobGroupDTO>>> GetJobGroups()
        {
            try
            {
                var jobGroups = await _masterDataService.GetJobGroupsAsync();
                return Ok(jobGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job groups");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("job-groups/{id}")]
        public async Task<ActionResult<JobGroupDTO>> GetJobGroup(int id)
        {
            try
            {
                var jobGroup = await _masterDataService.GetJobGroupAsync(id);
                if (jobGroup == null)
                    return NotFound();

                return Ok(jobGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job group {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("job-groups")]
        public async Task<ActionResult<JobGroupDTO>> CreateJobGroup(JobGroupDTO jobGroup)
        {
            try
            {
                var createdJobGroup = await _masterDataService.CreateJobGroupAsync(jobGroup);
                return CreatedAtAction(nameof(GetJobGroup), new { id = createdJobGroup.GroupId }, createdJobGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job group");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("job-groups/{id}")]
        public async Task<IActionResult> UpdateJobGroup(int id, JobGroupDTO jobGroup)
        {
            try
            {
                if (id != jobGroup.GroupId)
                    return BadRequest();

                var success = await _masterDataService.UpdateJobGroupAsync(jobGroup);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job group {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("job-groups/{id}")]
        public async Task<IActionResult> DeleteJobGroup(int id)
        {
            try
            {
                var success = await _masterDataService.DeleteJobGroupAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job group {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region GroupMembers
        [HttpGet("group-members")]
        public async Task<ActionResult<IEnumerable<GroupMemberDTO>>> GetGroupMembers()
        {
            try
            {
                var groupMembers = await _masterDataService.GetGroupMembersAsync();
                return Ok(groupMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("job-groups/{groupId}/members")]
        public async Task<ActionResult<IEnumerable<GroupMemberDTO>>> GetGroupMembersByGroup(int groupId)
        {
            try
            {
                var groupMembers = await _masterDataService.GetGroupMembersByGroupAsync(groupId);
                return Ok(groupMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members for group {GroupId}", groupId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("group-members/{id}")]
        public async Task<ActionResult<GroupMemberDTO>> GetGroupMember(int id)
        {
            try
            {
                var groupMember = await _masterDataService.GetGroupMemberAsync(id);
                if (groupMember == null)
                    return NotFound();

                return Ok(groupMember);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group member {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("group-members")]
        public async Task<ActionResult<GroupMemberDTO>> CreateGroupMember(GroupMemberCreateDTO groupMember)
        {
            try
            {
                var createdGroupMember = await _masterDataService.CreateGroupMemberAsync(groupMember);
                return CreatedAtAction(nameof(GetGroupMember), new { id = createdGroupMember.Id }, createdGroupMember);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group member");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("group-members/{id}")]
        public async Task<IActionResult> DeleteGroupMember(int id)
        {
            try
            {
                var success = await _masterDataService.DeleteGroupMemberAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group member {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Jobs
        [HttpGet("jobs")]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetJobs()
        {
            try
            {
                var jobs = await _masterDataService.GetJobsAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("jobs/{id}")]
        public async Task<ActionResult<JobDTO>> GetJob(int id)
        {
            try
            {
                var job = await _masterDataService.GetJobAsync(id);
                if (job == null)
                    return NotFound();

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("jobs")]
        public async Task<ActionResult<JobDTO>> CreateJob(JobCreateDTO job)
        {
            try
            {
                var createdJob = await _masterDataService.CreateJobAsync(job);
                return CreatedAtAction(nameof(GetJob), new { id = createdJob.JobId }, createdJob);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("jobs/{id}")]
        public async Task<IActionResult> UpdateJob(int id, JobDTO job)
        {
            try
            {
                if (id != job.JobId)
                    return BadRequest();

                var success = await _masterDataService.UpdateJobAsync(job);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("jobs/{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var success = await _masterDataService.DeleteJobAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("job-types")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetJobTypes()
        {
            try
            {
                var jobTypes = await _masterDataService.GetJobTypesAsync();
                return Ok(jobTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job types");
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion
    }
} 