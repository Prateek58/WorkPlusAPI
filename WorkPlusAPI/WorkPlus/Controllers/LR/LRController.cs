using Microsoft.AspNetCore.Mvc;
using WorkPlusAPI.WorkPlus.DTOs.LRDTOs;
using WorkPlusAPI.WorkPlus.Service.LR;

namespace WorkPlusAPI.WorkPlus.Controllers.LR
{
    [ApiController]
    [Route("api/[controller]")]
    public class LRController : ControllerBase
    {
        private readonly ILRService _lrService;
        private readonly ILogger<LRController> _logger;

        public LRController(ILRService lrService, ILogger<LRController> logger)
        {
            _lrService = lrService;
            _logger = logger;
        }

        // LR Entries CRUD Operations
        
        [HttpGet("entries")]
        public async Task<ActionResult<IEnumerable<LREntryDTO>>> GetLREntries()
        {
            try
                {
                var entries = await _lrService.GetLREntriesAsync();
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LR entries");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("entries/{id}")]
        public async Task<ActionResult<LREntryDTO>> GetLREntry(int id)
        {
            try
            {
                var entry = await _lrService.GetLREntryByIdAsync(id);
                if (entry == null)
                {
                    return NotFound($"LR entry with ID {id} not found");
                }
                return Ok(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LR entry with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("entries")]
        public async Task<ActionResult<LREntryDTO>> CreateLREntry([FromBody] CreateLREntryDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var entry = await _lrService.CreateLREntryAsync(createDto);
                return CreatedAtAction(nameof(GetLREntry), new { id = entry.EntryId }, entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating LR entry");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("entries/{id}")]
        public async Task<ActionResult> UpdateLREntry(int id, [FromBody] UpdateLREntryDTO updateDto)
        {
            try
            {
                if (id != updateDto.EntryId)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _lrService.UpdateLREntryAsync(updateDto);
                if (!success)
                {
                    return NotFound($"LR entry with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LR entry with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("entries/{id}")]
        public async Task<ActionResult> DeleteLREntry(int id)
        {
            try
            {
                var success = await _lrService.DeleteLREntryAsync(id);
                if (!success)
                {
                    return NotFound($"LR entry with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LR entry with ID {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // Master Data Endpoints

        [HttpGet("master-data/units")]
        public async Task<ActionResult<IEnumerable<UnitDTO>>> GetUnits()
        {
            try
            {
                var units = await _lrService.GetUnitsAsync();
                return Ok(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving units");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("master-data/parties")]
        public async Task<ActionResult<IEnumerable<PartyDTO>>> GetParties()
        {
            try
            {
                var parties = await _lrService.GetPartiesAsync();
                return Ok(parties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parties");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("master-data/transporters")]
        public async Task<ActionResult<IEnumerable<TransporterDTO>>> GetTransporters()
        {
            try
            {
                var transporters = await _lrService.GetTransportersAsync();
                return Ok(transporters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transporters");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("master-data/cities")]
        public async Task<ActionResult<IEnumerable<CityDTO>>> GetCities()
        {
            try
            {
                var cities = await _lrService.GetCitiesAsync();
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cities");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("master-data/cities/search")]
        public async Task<ActionResult<IEnumerable<CityDTO>>> SearchCities([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("Search term is required");
                }

                var cities = await _lrService.SearchCitiesAsync(searchTerm);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching cities with term {SearchTerm}", searchTerm);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("master-data/cities")]
        public async Task<ActionResult<CityDTO>> CreateCity([FromBody] CreateCityDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var city = await _lrService.CreateCityAsync(createDto);
                return CreatedAtAction(nameof(GetCities), city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating city");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("master-data/document-types")]
        public async Task<ActionResult<IEnumerable<DocumentTypeDTO>>> GetDocumentTypes()
        {
            try
            {
                var documentTypes = await _lrService.GetDocumentTypesAsync();
                return Ok(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types");
                return StatusCode(500, "Internal server error");
            }
        }

        // Document Management

        [HttpGet("entries/{lrEntryId}/documents")]
        public async Task<ActionResult<IEnumerable<DocumentDTO>>> GetDocuments(int lrEntryId)
        {
            try
            {
                var documents = await _lrService.GetDocumentsByLREntryIdAsync(lrEntryId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for LR entry {LrEntryId}", lrEntryId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("entries/{lrEntryId}/documents")]
        public async Task<ActionResult<DocumentDTO>> UploadDocument(
            int lrEntryId, 
            [FromForm] int typeId, 
            [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is required");
                }

                // Here you would typically save the file to disk/cloud storage
                // For now, we'll just use the original filename
                var fileName = file.FileName;

                var document = await _lrService.UploadDocumentAsync(lrEntryId, typeId, fileName);
                return CreatedAtAction(nameof(GetDocuments), new { lrEntryId }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for LR entry {LrEntryId}", lrEntryId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("documents/{documentId}")]
        public async Task<ActionResult> DeleteDocument(int documentId)
        {
            try
            {
                var success = await _lrService.DeleteDocumentAsync(documentId);
                if (!success)
                {
                    return NotFound($"Document with ID {documentId} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID {DocumentId}", documentId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 