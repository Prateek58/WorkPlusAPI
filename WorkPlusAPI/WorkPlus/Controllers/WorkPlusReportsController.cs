using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs.WorkPlusReportsDTOs;
using WorkPlusAPI.WorkPlus.Service;

namespace WorkPlusAPI.WorkPlus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkPlusReportsController : ControllerBase
    {
        private readonly IWorkPlusReportsService _reportsService;
        private readonly ILogger<WorkPlusReportsController> _logger;

        public WorkPlusReportsController(IWorkPlusReportsService reportsService, ILogger<WorkPlusReportsController> logger)
        {
            _reportsService = reportsService;
            _logger = logger;
        }

        [HttpGet("JobEntries")]
        public async Task<ActionResult<PaginatedJobEntryReportDTO>> GetJobEntriesReport([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var report = await _reportsService.GetPaginatedJobEntriesReportAsync(pageNumber, pageSize);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job entries report");
                return StatusCode(500, "An error occurred while retrieving the job entries report");
            }
        }
    }
} 