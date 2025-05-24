using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.Archive.DTOs.Archive;
using WorkPlusAPI.Archive.Services.Archive;

namespace WorkPlusAPI.Archive.Controllers.Archive;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LRController : ControllerBase
{
    private readonly ILRService _lrService;
    private readonly ILogger<LRController> _logger;

    public LRController(ILRService lrService, ILogger<LRController> logger)
    {
        _lrService = lrService;
        _logger = logger;
    }

    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<LRUnitDto>>> GetUnits()
    {
        var units = await _lrService.GetUnitsAsync();
        return Ok(units);
    }

    [HttpGet("parties")]
    public async Task<ActionResult<IEnumerable<LRPartyDto>>> GetParties()
    {
        var parties = await _lrService.GetPartiesAsync();
        return Ok(parties);
    }

    [HttpGet("transporters")]
    public async Task<ActionResult<IEnumerable<LRTransporterDto>>> GetTransporters()
    {
        var transporters = await _lrService.GetTransportersAsync();
        return Ok(transporters);
    }

    [HttpGet("cities")]
    public async Task<ActionResult<IEnumerable<LRCityDto>>> GetCities()
    {
        var cities = await _lrService.GetCitiesAsync();
        return Ok(cities);
    }

    [HttpGet("list")]
    public async Task<ActionResult<LRResponse>> GetLREntries([FromQuery] LRFilter filter)
    {
        try
        {
            var lrEntries = await _lrService.GetLREntriesAsync(filter);
            return Ok(lrEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LR entries: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving LR entries");
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<LRSummaryDto>> GetSummary([FromQuery] LRFilter filter)
    {
        try
        {
            var summary = await _lrService.GetLRSummaryAsync(filter);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LR summary: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving the LR summary");
        }
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] LRFilter filter)
    {
        var fileBytes = await _lrService.ExportToExcelAsync(filter);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "lr_entries.xlsx");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportToPdf([FromQuery] LRFilter filter)
    {
        try
        {
            var fileBytes = await _lrService.ExportToPdfAsync(filter);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                _logger.LogError("PDF generation failed: Empty byte array returned");
                return StatusCode(500, "Failed to generate PDF");
            }
            return File(fileBytes, "application/pdf", $"lr_entries_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF: {Message}", ex.Message);
            return StatusCode(500, $"Failed to generate PDF: {ex.Message}");
        }
    }

    [HttpGet("export/summary")]
    public async Task<IActionResult> ExportSummaryToPdf([FromQuery] LRFilter filter)
    {
        try
        {
            var fileBytes = await _lrService.ExportSummaryToPdfAsync(filter);
            if (fileBytes == null || fileBytes.Length == 0)
            {
                _logger.LogError("Summary PDF generation failed: Empty byte array returned");
                return StatusCode(500, "Failed to generate summary PDF");
            }
            return File(fileBytes, "application/pdf", $"lr_summary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary PDF: {Message}", ex.Message);
            return StatusCode(500, $"Failed to generate summary PDF: {ex.Message}");
        }
    }

    [HttpGet("parties/search")]
    public async Task<ActionResult<IEnumerable<LRPartyDto>>> SearchParties([FromQuery] string search)
    {
        try
        {
            var parties = await _lrService.SearchPartiesAsync(search);
            return Ok(parties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching parties");
            return StatusCode(500, "An error occurred while searching parties");
        }
    }
}
