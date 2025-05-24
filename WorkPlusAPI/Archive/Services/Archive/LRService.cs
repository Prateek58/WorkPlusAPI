using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.Data;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml.Style;
using System.Text.Json;
using WorkPlusAPI.Archive.DTOs.Archive;

namespace WorkPlusAPI.Archive.Services.Archive;

public class LRService : ILRService
{
    private readonly ArchiveContext _context;
    private readonly ILogger<LRService> _logger;

    public LRService(ArchiveContext context, ILogger<LRService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<LRUnitDto>> GetUnitsAsync()
    {
        try
        {
            var units = await _context.DboUnits
                .Select(u => new LRUnitDto
                {
                    UnitId = (byte)u.UnitId,
                    UnitName = u.UnitName ?? string.Empty
                })
                .ToListAsync();
            return units;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting units");
            throw;
        }
    }

    public async Task<IEnumerable<LRPartyDto>> GetPartiesAsync()
    {
        try
        {
            var parties = await _context.LrParties
                .Select(p => new LRPartyDto
                {
                    PartyId = p.PartyId,
                    PartyName = p.PartyName,
                    Address1 = p.Address1,
                    Address2 = p.Address2,
                    CityId = p.CityId,
                    CityName = p.CityName,
                    PinCode = p.PinCode,
                    MobilePhone = p.MobilePhone,
                    Telephone = p.Telephone,
                    Email = p.Email
                })
                .ToListAsync();
            return parties;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parties");
            throw;
        }
    }

    public async Task<IEnumerable<LRTransporterDto>> GetTransportersAsync()
    {
        try
        {
            var transporters = await _context.LrTransporters
                .Select(t => new LRTransporterDto
                {
                    TransporterId = t.TransporterId,
                    TransporterName = t.TransporterName
                })
                .ToListAsync();
            return transporters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transporters");
            throw;
        }
    }

    public async Task<IEnumerable<LRCityDto>> GetCitiesAsync()
    {
        try
        {
            // Get distinct cities from lr_entries
            var cities = await _context.LrEntries
                .Where(e => !string.IsNullOrEmpty(e.CityName))
                .Select(e => new { e.CityId, e.CityName })
                .Distinct()
                .Select(c => new LRCityDto
                {
                    CityId = c.CityId ?? 0,
                    CityName = c.CityName ?? string.Empty
                })
                .ToListAsync();
            return cities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cities");
            throw;
        }
    }

    // Define a class to represent the query result
    private class LRQueryResult
    {
        public long EntryId { get; set; }
        public byte? UnitId { get; set; }
        public string? UnitName { get; set; }
        public long? PartyId { get; set; }
        public string? PartyName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public string? BillAmount { get; set; }
        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? LrNo { get; set; }
        public DateTime? LrDate { get; set; }
        public decimal? LrAmount { get; set; }
        public decimal? Freight { get; set; }
        public decimal? OtherExpenses { get; set; }
        public decimal? TotalFreight { get; set; }
        public decimal? LrWeight { get; set; }
        public decimal? RatePerQtl { get; set; }
        public string? TruckNo { get; set; }
        public decimal? LrQty { get; set; }
        public decimal? Wces { get; set; }
        public decimal? LooseEs { get; set; }
        public decimal? FMills { get; set; }
        public decimal? Hopper { get; set; }
        public decimal? Spares { get; set; }
        public decimal? Others { get; set; }
        public decimal? TotalQty { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string? ReceivedBy { get; set; }
        public string? ReceivedNotes { get; set; }
        public bool? IsEmailSent { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public async Task<LRResponse> GetLREntriesAsync(LRFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting GetLREntriesAsync with filter: {@Filter}", filter);

            // Build the base query using IQueryable with concrete type
            var query = from entry in _context.LrEntries
                        join party in _context.LrParties on entry.PartyId equals party.PartyId into partyJoin
                        from party in partyJoin.DefaultIfEmpty()
                        join transporter in _context.LrTransporters on entry.TransporterId equals transporter.TransporterId into transporterJoin
                        from transporter in transporterJoin.DefaultIfEmpty()
                        join unit in _context.DboUnits on entry.UnitId equals unit.UnitId into unitJoin
                        from unit in unitJoin.DefaultIfEmpty()
                        select new LRQueryResult
                        {
                            EntryId = entry.EntryId,
                            UnitId = entry.UnitId.HasValue ? (byte?)entry.UnitId.Value : null,
                            UnitName = unit != null ? unit.UnitName : string.Empty,
                            PartyId = entry.PartyId,
                            PartyName = party != null ? party.PartyName : string.Empty,
                            CityId = entry.CityId,
                            CityName = entry.CityName,
                            BillNo = entry.BillNo,
                            BillDate = entry.BillDate,
                            BillAmount = entry.BillAmount,
                            TransporterId = entry.TransporterId,
                            TransporterName = transporter != null ? transporter.TransporterName : string.Empty,
                            LrNo = entry.LrNo,
                            LrDate = entry.LrDate,
                            LrAmount = entry.LrAmount,
                            Freight = entry.Freight,
                            OtherExpenses = entry.OtherExpenses,
                            TotalFreight = entry.TotalFreight,
                            LrWeight = entry.LrWeight,
                            RatePerQtl = entry.RatePerQtl,
                            TruckNo = entry.TruckNo,
                            LrQty = entry.LrQty,
                            Wces = entry.Wces,
                            LooseEs = entry.LooseEs,
                            FMills = entry.FMills,
                            Hopper = entry.Hopper,
                            Spares = entry.Spares,
                            Others = entry.Others,
                            TotalQty = entry.TotalQty,
                            ReceivedOn = entry.ReceivedOn,
                            ReceivedBy = entry.ReceivedBy,
                            ReceivedNotes = entry.ReceivedNotes,
                            IsEmailSent = entry.IsEmailSent,
                            CreatedAt = entry.CreatedAt
                        };

            // Apply filters
            if (filter.StartDate.HasValue)
            {
                _logger.LogInformation("Filtering by start date: {StartDate}", filter.StartDate.Value);
                query = query.Where(x => x.LrDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                _logger.LogInformation("Filtering by end date: {EndDate}", filter.EndDate.Value);
                query = query.Where(x => x.LrDate <= filter.EndDate.Value);
            }

            if (filter.UnitId.HasValue)
            {
                _logger.LogInformation("Filtering by unit ID: {UnitId}", filter.UnitId.Value);
                query = query.Where(x => x.UnitId == filter.UnitId.Value);
            }

            if (filter.PartyId.HasValue)
            {
                _logger.LogInformation("Filtering by party ID: {PartyId}", filter.PartyId.Value);
                query = query.Where(x => x.PartyId == filter.PartyId.Value);
            }

            if (filter.TransporterId.HasValue)
            {
                _logger.LogInformation("Filtering by transporter ID: {TransporterId}", filter.TransporterId.Value);
                query = query.Where(x => x.TransporterId == filter.TransporterId.Value);
            }

            if (filter.CityId.HasValue)
            {
                _logger.LogInformation("Filtering by city ID: {CityId}", filter.CityId.Value);
                query = query.Where(x => x.CityId == filter.CityId.Value);
            }

            if (!string.IsNullOrEmpty(filter.BillNo))
            {
                _logger.LogInformation("Filtering by bill number: {BillNo}", filter.BillNo);
                query = query.Where(x => x.BillNo != null && x.BillNo.Contains(filter.BillNo));
            }

            if (!string.IsNullOrEmpty(filter.LrNo))
            {
                _logger.LogInformation("Filtering by LR number: {LrNo}", filter.LrNo);
                query = query.Where(x => x.LrNo != null && x.LrNo.Contains(filter.LrNo));
            }

            if (!string.IsNullOrEmpty(filter.TruckNo))
            {
                _logger.LogInformation("Filtering by truck number: {TruckNo}", filter.TruckNo);
                query = query.Where(x => x.TruckNo != null && x.TruckNo.Contains(filter.TruckNo));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            _logger.LogInformation("Total matching records: {TotalCount}", totalCount);

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "billdate":
                        query = filter.SortOrder?.ToLower() == "asc" 
                            ? query.OrderBy(x => x.BillDate)
                            : query.OrderByDescending(x => x.BillDate);
                        break;
                    case "lrdate":
                        query = filter.SortOrder?.ToLower() == "asc" 
                            ? query.OrderBy(x => x.LrDate)
                            : query.OrderByDescending(x => x.LrDate);
                        break;
                    case "billno":
                        query = filter.SortOrder?.ToLower() == "asc" 
                            ? query.OrderBy(x => x.BillNo)
                            : query.OrderByDescending(x => x.BillNo);
                        break;
                    case "lrno":
                        query = filter.SortOrder?.ToLower() == "asc" 
                            ? query.OrderBy(x => x.LrNo)
                            : query.OrderByDescending(x => x.LrNo);
                        break;
                    case "partyname":
                        query = filter.SortOrder?.ToLower() == "asc" 
                            ? query.OrderBy(x => x.PartyName)
                            : query.OrderByDescending(x => x.PartyName);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.LrDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.LrDate);
            }

            // Apply pagination
            if (filter.Page.HasValue && filter.PageSize.HasValue)
            {
                var skip = (filter.Page.Value - 1) * filter.PageSize.Value;
                query = query.Skip(skip).Take(filter.PageSize.Value);
            }

            // Execute query and map to DTOs
            var queryResults = await query.ToListAsync();
            _logger.LogInformation("Retrieved {Count} records from database", queryResults.Count);

            var lrEntries = queryResults.Select(result => new LREntryDto
            {
                EntryId = result.EntryId,
                UnitId = result.UnitId,
                UnitName = result.UnitName,
                PartyId = result.PartyId,
                PartyName = result.PartyName,
                CityId = result.CityId,
                CityName = result.CityName,
                BillNo = result.BillNo,
                BillDate = result.BillDate,
                BillAmount = result.BillAmount,
                TransporterId = result.TransporterId,
                TransporterName = result.TransporterName,
                LrNo = result.LrNo,
                LrDate = result.LrDate,
                LrAmount = result.LrAmount,
                Freight = result.Freight,
                OtherExpenses = result.OtherExpenses,
                TotalFreight = result.TotalFreight,
                LrWeight = result.LrWeight,
                RatePerQtl = result.RatePerQtl,
                TruckNo = result.TruckNo,
                LrQty = result.LrQty,
                Wces = result.Wces,
                LooseEs = result.LooseEs,
                FMills = result.FMills,
                Hopper = result.Hopper,
                Spares = result.Spares,
                Others = result.Others,
                TotalQty = result.TotalQty,
                ReceivedOn = result.ReceivedOn,
                ReceivedBy = result.ReceivedBy,
                ReceivedNotes = result.ReceivedNotes,
                IsEmailSent = result.IsEmailSent,
                CreatedAt = result.CreatedAt
            }).ToList();

            _logger.LogInformation("Mapped {Count} LR entries", lrEntries.Count);

            return new LRResponse
            {
                Data = lrEntries,
                Total = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LR entries: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<LRSummaryDto> GetLRSummaryAsync(LRFilter filter)
    {
        try
        {
            _logger.LogInformation("Starting GetLRSummaryAsync with filter: {@Filter}", filter);

            // Build the same base query as GetLREntriesAsync but for summary calculations
            var query = from entry in _context.LrEntries
                        join party in _context.LrParties on entry.PartyId equals party.PartyId into partyJoin
                        from party in partyJoin.DefaultIfEmpty()
                        join transporter in _context.LrTransporters on entry.TransporterId equals transporter.TransporterId into transporterJoin
                        from transporter in transporterJoin.DefaultIfEmpty()
                        join unit in _context.DboUnits on entry.UnitId equals unit.UnitId into unitJoin
                        from unit in unitJoin.DefaultIfEmpty()
                        select new { entry, party, transporter, unit };

            // Apply the same filters as GetLREntriesAsync
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.entry.LrDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(x => x.entry.LrDate <= filter.EndDate.Value);
            }

            if (filter.UnitId.HasValue)
            {
                query = query.Where(x => x.entry.UnitId == filter.UnitId.Value);
            }

            if (filter.PartyId.HasValue)
            {
                query = query.Where(x => x.entry.PartyId == filter.PartyId.Value);
            }

            if (filter.TransporterId.HasValue)
            {
                query = query.Where(x => x.entry.TransporterId == filter.TransporterId.Value);
            }

            if (filter.CityId.HasValue)
            {
                query = query.Where(x => x.entry.CityId == filter.CityId.Value);
            }

            if (!string.IsNullOrEmpty(filter.BillNo))
            {
                query = query.Where(x => x.entry.BillNo != null && x.entry.BillNo.Contains(filter.BillNo));
            }

            if (!string.IsNullOrEmpty(filter.LrNo))
            {
                query = query.Where(x => x.entry.LrNo != null && x.entry.LrNo.Contains(filter.LrNo));
            }

            if (!string.IsNullOrEmpty(filter.TruckNo))
            {
                query = query.Where(x => x.entry.TruckNo != null && x.entry.TruckNo.Contains(filter.TruckNo));
            }

            // Calculate summary statistics
            var entries = await query.Select(x => x.entry).ToListAsync();

            var totalEntries = entries.Count;
            var totalParties = entries.Where(e => e.PartyId.HasValue).Select(e => e.PartyId).Distinct().Count();
            var totalTransporters = entries.Where(e => e.TransporterId.HasValue).Select(e => e.TransporterId).Distinct().Count();
            var totalCities = entries.Where(e => e.CityId.HasValue).Select(e => e.CityId).Distinct().Count();
            var totalLrAmount = entries.Sum(e => e.LrAmount ?? 0);
            var totalFreight = entries.Sum(e => e.Freight ?? 0);
            var totalOtherExpenses = entries.Sum(e => e.OtherExpenses ?? 0);
            var totalWeight = entries.Sum(e => e.LrWeight ?? 0);
            var totalQuantity = entries.Sum(e => e.TotalQty ?? 0);

            // Unit-wise summaries
            var unitSummaries = entries
                .Where(e => e.UnitId.HasValue)
                .GroupBy(e => new { e.UnitId })
                .Select(g => new UnitSummary
                {
                    UnitId = (byte)g.Key.UnitId!.Value,
                    UnitName = _context.DboUnits.FirstOrDefault(u => u.UnitId == g.Key.UnitId)?.UnitName ?? "Unknown",
                    EntryCount = g.Count(),
                    TotalLrAmount = g.Sum(e => e.LrAmount ?? 0),
                    TotalFreight = g.Sum(e => e.Freight ?? 0),
                    TotalWeight = g.Sum(e => e.LrWeight ?? 0),
                    TotalQuantity = g.Sum(e => e.TotalQty ?? 0)
                })
                .ToList();

            var summary = new LRSummaryDto
            {
                TotalEntries = totalEntries,
                TotalParties = totalParties,
                TotalTransporters = totalTransporters,
                TotalCities = totalCities,
                TotalLrAmount = totalLrAmount,
                TotalFreight = totalFreight,
                TotalOtherExpenses = totalOtherExpenses,
                TotalWeight = totalWeight,
                TotalQuantity = totalQuantity,
                TotalRecords = totalEntries,
                UnitSummaries = unitSummaries
            };

            _logger.LogInformation("LR Summary calculated: {TotalRecords} records, {TotalAmount} total amount", 
                summary.TotalRecords, summary.TotalLrAmount);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LR summary: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<byte[]> ExportToExcelAsync(LRFilter filter)
    {
        try
        {
            // Get all data without pagination for export
            var exportFilter = new LRFilter
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                UnitId = filter.UnitId,
                PartyId = filter.PartyId,
                TransporterId = filter.TransporterId,
                CityId = filter.CityId,
                BillNo = filter.BillNo,
                LrNo = filter.LrNo,
                TruckNo = filter.TruckNo,
                Page = null, // No pagination for export
                PageSize = null,
                SortBy = filter.SortBy,
                SortOrder = filter.SortOrder,
                Columns = filter.Columns
            };

            var lrResponse = await GetLREntriesAsync(exportFilter);
            var lrEntries = lrResponse.Data;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("LR Entries");

            // Determine which columns to include
            var selectedColumns = GetSelectedColumns(filter.Columns);

            // Set headers
            int col = 1;
            var columnMap = new Dictionary<string, int>();

            foreach (var column in selectedColumns)
            {
                worksheet.Cells[1, col].Value = GetColumnHeader(column);
                worksheet.Cells[1, col].Style.Font.Bold = true;
                worksheet.Cells[1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                columnMap[column] = col;
                col++;
            }

            // Fill data
            int row = 2;
            foreach (var entry in lrEntries)
            {
                foreach (var column in selectedColumns)
                {
                    var colIndex = columnMap[column];
                    worksheet.Cells[row, colIndex].Value = GetColumnValue(entry, column);
                }
                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting LR entries to Excel");
            throw;
        }
    }

    public async Task<byte[]> ExportToPdfAsync(LRFilter filter)
    {
        try
        {
            // Get all data without pagination for export
            var exportFilter = new LRFilter
            {
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                UnitId = filter.UnitId,
                PartyId = filter.PartyId,
                TransporterId = filter.TransporterId,
                CityId = filter.CityId,
                BillNo = filter.BillNo,
                LrNo = filter.LrNo,
                TruckNo = filter.TruckNo,
                Page = null,
                PageSize = null,
                SortBy = filter.SortBy,
                SortOrder = filter.SortOrder,
                Columns = filter.Columns
            };

            var lrResponse = await GetLREntriesAsync(exportFilter);
            var lrEntries = lrResponse.Data;

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, stream);
            
            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var title = new Paragraph("Lorry Receipt (LR) Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Add filter information
            var filterFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            if (filter.StartDate.HasValue || filter.EndDate.HasValue)
            {
                var dateRange = $"Date Range: {(filter.StartDate?.ToString("dd/MM/yyyy") ?? "Start")} to {(filter.EndDate?.ToString("dd/MM/yyyy") ?? "End")}";
                document.Add(new Paragraph(dateRange, filterFont) { SpacingAfter = 10 });
            }

            // Determine which columns to include
            var selectedColumns = GetSelectedColumns(filter.Columns);

            // Create table
            var table = new PdfPTable(selectedColumns.Count)
            {
                WidthPercentage = 100,
                SpacingBefore = 10
            };

            // Set column widths based on content
            float[] widths = selectedColumns.Select(GetColumnWidth).ToArray();
            table.SetWidths(widths);

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            foreach (var column in selectedColumns)
            {
                AddHeaderCell(table, GetColumnHeader(column), headerFont);
            }

            // Add data
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var entry in lrEntries)
            {
                foreach (var column in selectedColumns)
                {
                    AddCell(table, GetColumnValue(entry, column)?.ToString() ?? "", dataFont, Element.ALIGN_LEFT);
                }
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting LR entries to PDF");
            throw;
        }
    }

    public async Task<byte[]> ExportSummaryToPdfAsync(LRFilter filter)
    {
        try
        {
            var summary = await GetLRSummaryAsync(filter);

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, stream);
            
            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var title = new Paragraph("LR Summary Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);

            // Add summary statistics
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

            document.Add(new Paragraph($"Total Entries: {summary.TotalEntries:N0}", normalFont));
            document.Add(new Paragraph($"Total Parties: {summary.TotalParties:N0}", normalFont));
            document.Add(new Paragraph($"Total Transporters: {summary.TotalTransporters:N0}", normalFont));
            document.Add(new Paragraph($"Total Cities: {summary.TotalCities:N0}", normalFont));
            document.Add(new Paragraph($"Total LR Amount: ₹{summary.TotalLrAmount:N2}", normalFont));
            document.Add(new Paragraph($"Total Freight: ₹{summary.TotalFreight:N2}", normalFont));
            document.Add(new Paragraph($"Total Other Expenses: ₹{summary.TotalOtherExpenses:N2}", normalFont));
            document.Add(new Paragraph($"Total Weight: {summary.TotalWeight:N2} Qtl", normalFont));
            document.Add(new Paragraph($"Total Quantity: {summary.TotalQuantity:N2}", normalFont));

            // Add unit-wise summary if available
            if (summary.UnitSummaries.Any())
            {
                document.Add(new Paragraph("\nUnit-wise Summary:", boldFont) { SpacingBefore = 20 });

                var table = new PdfPTable(6) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 20, 15, 20, 20, 15, 15 });

                // Headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                AddHeaderCell(table, "Unit", headerFont);
                AddHeaderCell(table, "Entries", headerFont);
                AddHeaderCell(table, "LR Amount", headerFont);
                AddHeaderCell(table, "Freight", headerFont);
                AddHeaderCell(table, "Weight", headerFont);
                AddHeaderCell(table, "Quantity", headerFont);

                // Data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var unitSummary in summary.UnitSummaries)
                {
                    AddCell(table, unitSummary.UnitName, dataFont, Element.ALIGN_LEFT);
                    AddCell(table, unitSummary.EntryCount.ToString("N0"), dataFont, Element.ALIGN_RIGHT);
                    AddCell(table, "₹" + unitSummary.TotalLrAmount.ToString("N2"), dataFont, Element.ALIGN_RIGHT);
                    AddCell(table, "₹" + unitSummary.TotalFreight.ToString("N2"), dataFont, Element.ALIGN_RIGHT);
                    AddCell(table, unitSummary.TotalWeight.ToString("N2"), dataFont, Element.ALIGN_RIGHT);
                    AddCell(table, unitSummary.TotalQuantity.ToString("N2"), dataFont, Element.ALIGN_RIGHT);
                }

                document.Add(table);
            }

            document.Close();
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting LR summary to PDF");
            throw;
        }
    }

    public async Task<IEnumerable<LRPartyDto>> SearchPartiesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<LRPartyDto>();

            var parties = await _context.LrParties
                .Where(p => p.PartyName.Contains(searchTerm))
                .Take(50) // Limit results
                .Select(p => new LRPartyDto
                {
                    PartyId = p.PartyId,
                    PartyName = p.PartyName,
                    Address1 = p.Address1,
                    Address2 = p.Address2,
                    CityId = p.CityId,
                    CityName = p.CityName,
                    PinCode = p.PinCode,
                    MobilePhone = p.MobilePhone,
                    Telephone = p.Telephone,
                    Email = p.Email
                })
                .ToListAsync();

            return parties;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching parties");
            throw;
        }
    }

    // Helper methods
    private List<string> GetSelectedColumns(string? columnsJson)
    {
        var defaultColumns = new List<string>
        {
            "unitName", "billDate", "billNo", "partyName", "cityName", 
            "transporterName", "lrNo", "lrDate", "lrWeight", "ratePerQtl"
        };

        _logger.LogInformation("GetSelectedColumns called with columnsJson: {ColumnsJson}", columnsJson ?? "null");

        if (string.IsNullOrEmpty(columnsJson))
        {
            _logger.LogInformation("Using default columns: {DefaultColumns}", string.Join(", ", defaultColumns));
            return defaultColumns;
        }

        try
        {
            // Handle comma-separated string (like JobWorkService does)
            var columns = columnsJson.Split(',').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList();
            if (columns.Any())
            {
                _logger.LogInformation("Using selected columns: {SelectedColumns}", string.Join(", ", columns));
                return columns;
            }
            else
            {
                _logger.LogWarning("Parsed columns is empty, using default columns");
                return defaultColumns;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing columns string, using default columns");
            return defaultColumns;
        }
    }

    private string GetColumnHeader(string column)
    {
        return column switch
        {
            "unitName" => "Unit",
            "billDate" => "Bill Date",
            "billNo" => "Bill No",
            "partyName" => "Party",
            "cityName" => "City",
            "transporterName" => "Transporter",
            "lrNo" => "LR No",
            "lrDate" => "LR Date",
            "lrWeight" => "Weight",
            "ratePerQtl" => "Rate/Qtl",
            "lrAmount" => "LR Amount",
            "freight" => "Freight",
            "totalFreight" => "Total Freight",
            "truckNo" => "Truck No",
            "billAmount" => "Bill Amount",
            "otherExpenses" => "Other Expenses",
            "totalQty" => "Total Qty",
            _ => column
        };
    }

    private object? GetColumnValue(LREntryDto entry, string column)
    {
        return column switch
        {
            "unitName" => entry.UnitName,
            "billDate" => entry.BillDate?.ToString("dd/MM/yyyy"),
            "billNo" => entry.BillNo,
            "partyName" => entry.PartyName,
            "cityName" => entry.CityName,
            "transporterName" => entry.TransporterName,
            "lrNo" => entry.LrNo,
            "lrDate" => entry.LrDate?.ToString("dd/MM/yyyy"),
            "lrWeight" => entry.LrWeight?.ToString("N2"),
            "ratePerQtl" => entry.RatePerQtl?.ToString("N2"),
            "lrAmount" => entry.LrAmount?.ToString("N2"),
            "freight" => entry.Freight?.ToString("N2"),
            "totalFreight" => entry.TotalFreight?.ToString("N2"),
            "truckNo" => entry.TruckNo,
            "billAmount" => entry.BillAmount,
            "otherExpenses" => entry.OtherExpenses?.ToString("N2"),
            "totalQty" => entry.TotalQty?.ToString("N2"),
            _ => ""
        };
    }

    private float GetColumnWidth(string column)
    {
        return column switch
        {
            "unitName" => 10f,
            "billDate" => 12f,
            "billNo" => 12f,
            "partyName" => 20f,
            "cityName" => 15f,
            "transporterName" => 18f,
            "lrNo" => 12f,
            "lrDate" => 12f,
            "lrWeight" => 10f,
            "ratePerQtl" => 10f,
            "lrAmount" => 12f,
            "freight" => 10f,
            "totalFreight" => 12f,
            "truckNo" => 12f,
            "billAmount" => 12f,
            "otherExpenses" => 12f,
            "totalQty" => 10f,
            _ => 10f
        };
    }

    private void AddHeaderCell(PdfPTable table, string text, Font font)
    {
        var cell = new PdfPCell(new Phrase(text, font))
        {
            BackgroundColor = BaseColor.LIGHT_GRAY,
            HorizontalAlignment = Element.ALIGN_CENTER,
            Padding = 5
        };
        table.AddCell(cell);
    }

    private void AddCell(PdfPTable table, string text, Font font, int alignment, int colspan = 1)
    {
        var cell = new PdfPCell(new Phrase(text ?? "", font))
        {
            HorizontalAlignment = alignment,
            Padding = 3,
            Colspan = colspan
        };
        table.AddCell(cell);
    }
} 