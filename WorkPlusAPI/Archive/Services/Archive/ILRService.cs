using WorkPlusAPI.Archive.DTOs.Archive;

namespace WorkPlusAPI.Archive.Services.Archive;

public interface ILRService
{
    Task<IEnumerable<LRUnitDto>> GetUnitsAsync();
    Task<IEnumerable<LRPartyDto>> GetPartiesAsync();
    Task<IEnumerable<LRTransporterDto>> GetTransportersAsync();
    Task<IEnumerable<LRCityDto>> GetCitiesAsync();
    Task<LRResponse> GetLREntriesAsync(LRFilter filter);
    Task<LRSummaryDto> GetLRSummaryAsync(LRFilter filter);
    Task<byte[]> ExportToExcelAsync(LRFilter filter);
    Task<byte[]> ExportToPdfAsync(LRFilter filter);
    Task<byte[]> ExportSummaryToPdfAsync(LRFilter filter);
    Task<IEnumerable<LRPartyDto>> SearchPartiesAsync(string searchTerm);
} 