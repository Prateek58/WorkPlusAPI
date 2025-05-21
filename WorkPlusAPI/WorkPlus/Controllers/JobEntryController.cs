using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Model;
using WorkPlusAPI.WorkPlus.Service;

namespace WorkPlusAPI.WorkPlus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobEntryController : ControllerBase
    {
        private readonly IJobEntryService _jobEntryService;
        private readonly ILogger<JobEntryController> _logger;

        public JobEntryController(IJobEntryService jobEntryService, ILogger<JobEntryController> logger)
        {
            _jobEntryService = jobEntryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobEntryDTO>>> GetAllJobEntries()
        {
            try
            {
                var entries = await _jobEntryService.GetAllJobEntriesAsync();
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job entries");
                return StatusCode(500, "An error occurred while retrieving job entries");
            }
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<object>> GetPaginatedJobEntries([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _jobEntryService.GetPaginatedJobEntriesAsync(pageNumber, pageSize);
                return Ok(new { items, totalCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated job entries");
                return StatusCode(500, "An error occurred while retrieving job entries");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobEntryDTO>> GetJobEntry(int id)
        {
            try
            {
                var entry = await _jobEntryService.GetJobEntryAsync(id);
                if (entry == null)
                {
                    return NotFound($"Job entry with ID {id} not found");
                }
                return Ok(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job entry with ID {EntryId}", id);
                return StatusCode(500, $"An error occurred while retrieving job entry with ID {id}");
            }
        }

        // Main job entry creation endpoint
        [HttpPost]
        public async Task<ActionResult<JobEntryDTO>> CreateJobEntry([FromBody] CreateJobEntryDTO createDto)
        {
            try
            {
                if (createDto == null)
                {
                    return BadRequest("Job entry data is required");
                }

                // Validate the entry
                if (createDto.EntryType == "Individual" && !createDto.WorkerId.HasValue)
                {
                    return BadRequest("Worker ID is required for individual entries");
                }
                else if (createDto.EntryType == "Group" && !createDto.GroupId.HasValue)
                {
                    return BadRequest("Group ID is required for group entries");
                }

                // Map DTO to model
                var jobEntry = new JobEntry
                {
                    JobId = createDto.JobId,
                    EntryType = createDto.EntryType,
                    WorkerId = createDto.WorkerId,
                    GroupId = createDto.GroupId,
                    IsPostLunch = createDto.IsPostLunch,
                    ItemsCompleted = createDto.ItemsCompleted,
                    HoursTaken = createDto.HoursTaken,
                    RatePerJob = createDto.RatePerJob,
                    ExpectedHours = createDto.ExpectedHours,
                    Remarks = createDto.Remarks,
                    IsFinalized = false,
                    CreatedAt = createDto.EntryDate ?? DateTime.Now // Use provided date or default to now
                };

                // Set the created by user ID from the authenticated user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    jobEntry.CreatedBy = userId;
                }
                else
                {
                    // Default to user ID 1 if claim not found (for testing purposes)
                    jobEntry.CreatedBy = 1;
                    _logger.LogWarning("User ID claim not found when creating job entry, defaulting to 1");
                }

                var createdEntry = await _jobEntryService.CreateJobEntryAsync(jobEntry);
                var entryDto = await _jobEntryService.GetJobEntryAsync(createdEntry.EntryId);
                
                return CreatedAtAction(nameof(GetJobEntry), new { id = createdEntry.EntryId }, entryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job entry");
                return StatusCode(500, $"An error occurred while creating the job entry: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobEntry(int id)
        {
            try
            {
                var result = await _jobEntryService.DeleteJobEntryAsync(id);
                if (!result)
                {
                    return NotFound($"Job entry with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job entry with ID {EntryId}", id);
                return StatusCode(500, $"An error occurred while deleting job entry with ID {id}");
            }
        }
    }
} 