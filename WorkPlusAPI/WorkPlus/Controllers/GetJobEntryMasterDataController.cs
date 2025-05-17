using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Service;

namespace WorkPlusAPI.WorkPlus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GetJobEntryMasterDataController : ControllerBase
    {
        private readonly IJobEntryService _jobEntryService;
        private readonly ILogger<GetJobEntryMasterDataController> _logger;

        public GetJobEntryMasterDataController(IJobEntryService jobEntryService, ILogger<GetJobEntryMasterDataController> logger)
        {
            _jobEntryService = jobEntryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<JobEntryMasterDataDTO>> GetMasterData()
        {
            try
            {
                var masterData = await _jobEntryService.GetJobEntryMasterDataAsync();
                
                // Log information about the data being returned
                _logger.LogInformation("Retrieved {WorkerCount} workers, {JobCount} jobs, and {GroupCount} job groups", 
                    masterData.Workers.Count, 
                    masterData.Jobs.Count,
                    masterData.JobGroups.Count);
                
                // Log detailed information about jobs
                if (masterData.Jobs.Any())
                {
                    foreach (var job in masterData.Jobs)
                    {
                        _logger.LogInformation("Job {JobId} ({JobName}): RatePerItem={RatePerItem}, RatePerHour={RatePerHour}, ExpectedHours={ExpectedHours}, ExpectedItemsPerHour={ExpectedItemsPerHour}",
                            job.JobId,
                            job.JobName,
                            job.RatePerItem,
                            job.RatePerHour,
                            job.ExpectedHours,
                            job.ExpectedItemsPerHour);
                    }
                }
                
                return Ok(masterData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting master data: {Message}", ex.Message);
                return StatusCode(500, "An error occurred while retrieving master data");
            }
        }
    }
} 