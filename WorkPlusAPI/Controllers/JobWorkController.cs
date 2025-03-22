using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkPlusAPI.Services;
using WorkPlusAPI.DTOs;

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
    public async Task<ActionResult<IEnumerable<JobWorkDto>>> GetJobWorks([FromQuery] JobWorkFilter filter)
    {
        var jobWorks = await _jobWorkService.GetJobWorksAsync(filter);
        return Ok(jobWorks);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<JobWorkSummaryDto>> GetSummary([FromQuery] JobWorkFilter filter)
    {
        var summary = await _jobWorkService.GetJobWorkSummaryAsync(filter);
        return Ok(summary);
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
        var fileBytes = await _jobWorkService.ExportToPdfAsync(filter);
        return File(fileBytes, "application/pdf", "jobworks.pdf");
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