using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPlusAPI.Services;
using WorkPlusAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WorkPlusAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobWorkController : ControllerBase
{
    private readonly IJobWorkService _jobWorkService;
    private readonly ILogger<JobWorkController> _logger;

    public JobWorkController(IJobWorkService jobWorkService, ILogger<JobWorkController> logger)
    {
        _jobWorkService = jobWorkService;
        _logger = logger;
    }

    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<UnitDto>>> GetUnits()
    {
        var units = await _jobWorkService.GetUnitsAsync();
        return Ok(units);
    }

    [HttpGet("job-work-types")]
    public async Task<ActionResult<IEnumerable<JobWorkTypeDto>>> GetJobWorkTypes()
    {
        var types = await _jobWorkService.GetJobWorkTypesAsync();
        return Ok(types);
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs([FromQuery] bool isGroup = false)
    {
        var jobs = await _jobWorkService.GetJobsAsync(isGroup);
        return Ok(jobs);
    }

    [HttpGet("list")]
    public async Task<ActionResult<JobWorkResponse>> GetJobWorks([FromQuery] JobWorkFilter filter)
    {
        try
        {
            var jobWorks = await _jobWorkService.GetJobWorksAsync(filter);
            return Ok(jobWorks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job works: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving job works");
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<JobWorkSummaryDto>> GetSummary([FromQuery] JobWorkFilter filter)
    {
        try
        {
            var summary = await _jobWorkService.GetJobWorkSummaryAsync(filter);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job work summary: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving the job work summary");
        }
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] JobWorkFilter filter)
    {
        var fileBytes = await _jobWorkService.ExportToExcelAsync(filter);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "jobworks.xlsx");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportToPdf([FromQuery] JobWorkFilter filter)
    {
        try
        {
            var fileBytes = await _jobWorkService.ExportToPdfAsync(filter);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                _logger.LogError("PDF generation failed: Empty byte array returned");
                return StatusCode(500, "Failed to generate PDF");
            }
            return File(fileBytes, "application/pdf", $"jobworks_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF: {Message}", ex.Message);
            return StatusCode(500, $"Failed to generate PDF: {ex.Message}");
        }
    }

    [HttpGet("export/summary")]
    public async Task<IActionResult> ExportSummaryToPdf([FromQuery] JobWorkFilter filter)
    {
        try
        {
            var fileBytes = await _jobWorkService.ExportSummaryToPdfAsync(filter);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                _logger.LogError("Summary PDF generation failed: Empty byte array returned");
                return StatusCode(500, "Failed to generate summary PDF");
            }
            return File(fileBytes, "application/pdf", $"jobwork_summary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary PDF: {Message}", ex.Message);
            return StatusCode(500, $"Failed to generate summary PDF: {ex.Message}");
        }
    }

    [HttpGet("employees")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> SearchEmployees([FromQuery] string search)
    {
        try
        {
            var employees = await _jobWorkService.SearchEmployeesAsync(search);
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees");
            return StatusCode(500, "An error occurred while searching employees");
        }
    }
} 