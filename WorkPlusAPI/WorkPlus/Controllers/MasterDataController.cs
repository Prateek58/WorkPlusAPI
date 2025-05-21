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
    }
} 