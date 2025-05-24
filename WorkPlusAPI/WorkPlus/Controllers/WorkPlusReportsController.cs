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
        public async Task<ActionResult<PaginatedJobEntryReportDTO>> GetJobEntriesReport([FromQuery] JobEntryFilter filter)
        {
            try
            {
                var report = await _reportsService.GetFilteredJobEntriesReportAsync(filter);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job entries report with filters: {@Filter}", filter);
                return StatusCode(500, "An error occurred while retrieving the job entries report");
            }
        }

        [HttpGet("JobEntries/FilterOptions")]
        public async Task<ActionResult<JobEntryFilterOptionsDTO>> GetJobEntryFilterOptions()
        {
            try
            {
                var options = await _reportsService.GetJobEntryFilterOptionsAsync();
                return Ok(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job entry filter options");
                return StatusCode(500, "An error occurred while retrieving filter options");
            }
        }

        [HttpGet("JobEntries/ExportColumns")]
        public async Task<ActionResult<ExportColumnsDTO>> GetExportColumns()
        {
            try
            {
                var columns = await _reportsService.GetExportColumnsAsync();
                return Ok(columns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving export columns");
                return StatusCode(500, "An error occurred while retrieving export columns");
            }
        }

        [HttpPost("JobEntries/Export")]
        public async Task<IActionResult> ExportJobEntries([FromBody] ExportRequest request)
        {
            try
            {
                var fileBytes = await _reportsService.ExportJobEntriesAsync(request);
                
                var contentType = request.ExportType.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    "pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };

                var fileExtension = request.ExportType.ToLower() switch
                {
                    "excel" => "xlsx",
                    "csv" => "csv",
                    "pdf" => "pdf",
                    _ => "bin"
                };

                var fileName = $"JobEntriesReport_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting job entries report: {@Request}", request);
                return StatusCode(500, "An error occurred while exporting the report");
            }
        }
    }
} 