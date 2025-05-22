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
    }
} 