using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs.LRDTOs;
using WorkPlusAPI.WorkPlus.Model.LR;
using System.Data;
// using MySql.Data.MySqlClient;
// using Dapper;

namespace WorkPlusAPI.WorkPlus.Service.LR
{
    public interface ILRService
    {
        // LR Entries
        Task<IEnumerable<LREntryDTO>> GetLREntriesAsync();
        Task<LREntryDTO?> GetLREntryByIdAsync(int id);
        Task<LREntryDTO> CreateLREntryAsync(CreateLREntryDTO createDto);
        Task<bool> UpdateLREntryAsync(UpdateLREntryDTO updateDto);
        Task<bool> DeleteLREntryAsync(int id);
        
        // Master Data
        Task<IEnumerable<UnitDTO>> GetUnitsAsync();
        Task<IEnumerable<PartyDTO>> GetPartiesAsync();
        Task<IEnumerable<TransporterDTO>> GetTransportersAsync();
        Task<IEnumerable<CityDTO>> GetCitiesAsync();
        Task<IEnumerable<DocumentTypeDTO>> GetDocumentTypesAsync();
        
        // City Management
        Task<CityDTO> CreateCityAsync(CreateCityDTO createDto);
        Task<IEnumerable<CityDTO>> SearchCitiesAsync(string searchTerm);
        
        // Document Management
        Task<IEnumerable<DocumentDTO>> GetDocumentsByLREntryIdAsync(int lrEntryId);
        Task<DocumentDTO> UploadDocumentAsync(int lrEntryId, int typeId, string fileName);
        Task<bool> DeleteDocumentAsync(int documentId);
    }
    
    public class LRService : ILRService
    {
        private readonly LRDbContext _context;
        private readonly IConfiguration _configuration;
        
        public LRService(LRDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        // LR Entries
        public async Task<IEnumerable<LREntryDTO>> GetLREntriesAsync()
        {
            var entries = await _context.LrEntries
                .Include(e => e.Unit)
                .Include(e => e.Party)
                .Include(e => e.Transporter)
                .Include(e => e.OriginCity)
                .Include(e => e.DestinationCity)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return entries.Select(e => new LREntryDTO
            {
                EntryId = e.EntryId,
                UnitId = e.UnitId,
                PartyId = e.PartyId,
                TransporterId = e.TransporterId,
                LrNo = e.LrNo,
                LrDate = e.LrDate.ToDateTime(TimeOnly.MinValue),
                BillDate = e.BillDate?.ToDateTime(TimeOnly.MinValue),
                BillNo = e.BillNo,
                TruckNo = e.TruckNo,
                LrWeight = e.LrWeight ?? 0,
                RatePerQtl = e.RatePerQtl ?? 0,
                LrQty = e.LrQty ?? 0,
                LrAmount = e.LrAmount ?? 0,
                Freight = e.Freight ?? 0,
                OtherExpenses = e.OtherExpenses ?? 0,
                TotalFreight = e.TotalFreight ?? 0,
                TotalQty = e.TotalQty ?? 0,
                BillAmount = e.BillAmount ?? 0,
                OriginCityId = e.OriginCityId,
                DestinationCityId = e.DestinationCityId,
                DriverName = e.DriverName,
                DriverMobile = e.DriverMobile,
                Remarks = e.Remarks,
                Status = e.Status ?? "DRAFT",
                CreatedBy = e.CreatedBy,
                CreatedAt = e.CreatedAt ?? DateTime.Now,
                UpdatedBy = e.UpdatedBy,
                UpdatedAt = e.UpdatedAt ?? DateTime.Now,
                UnitName = e.Unit?.UnitName,
                PartyName = e.Party?.PartyName,
                TransporterName = e.Transporter?.TransporterName,
                OriginCityName = e.OriginCity?.CityName,
                DestinationCityName = e.DestinationCity?.CityName
            });
        }
        
        public async Task<LREntryDTO?> GetLREntryByIdAsync(int id)
        {
            var entry = await _context.LrEntries
                .Include(e => e.Unit)
                .Include(e => e.Party)
                .Include(e => e.Transporter)
                .Include(e => e.OriginCity)
                .Include(e => e.DestinationCity)
                .FirstOrDefaultAsync(e => e.EntryId == id);

            if (entry == null) return null;

            return new LREntryDTO
            {
                EntryId = entry.EntryId,
                UnitId = entry.UnitId,
                PartyId = entry.PartyId,
                TransporterId = entry.TransporterId,
                LrNo = entry.LrNo,
                LrDate = entry.LrDate.ToDateTime(TimeOnly.MinValue),
                BillDate = entry.BillDate?.ToDateTime(TimeOnly.MinValue),
                BillNo = entry.BillNo,
                TruckNo = entry.TruckNo,
                LrWeight = entry.LrWeight ?? 0,
                RatePerQtl = entry.RatePerQtl ?? 0,
                LrQty = entry.LrQty ?? 0,
                LrAmount = entry.LrAmount ?? 0,
                Freight = entry.Freight ?? 0,
                OtherExpenses = entry.OtherExpenses ?? 0,
                TotalFreight = entry.TotalFreight ?? 0,
                TotalQty = entry.TotalQty ?? 0,
                BillAmount = entry.BillAmount ?? 0,
                OriginCityId = entry.OriginCityId,
                DestinationCityId = entry.DestinationCityId,
                DriverName = entry.DriverName,
                DriverMobile = entry.DriverMobile,
                Remarks = entry.Remarks,
                Status = entry.Status ?? "DRAFT",
                CreatedBy = entry.CreatedBy,
                CreatedAt = entry.CreatedAt ?? DateTime.Now,
                UpdatedBy = entry.UpdatedBy,
                UpdatedAt = entry.UpdatedAt ?? DateTime.Now,
                UnitName = entry.Unit?.UnitName,
                PartyName = entry.Party?.PartyName,
                TransporterName = entry.Transporter?.TransporterName,
                OriginCityName = entry.OriginCity?.CityName,
                DestinationCityName = entry.DestinationCity?.CityName
            };
        }
        
        public async Task<LREntryDTO> CreateLREntryAsync(CreateLREntryDTO createDto)
        {
            var entry = new LrEntry
            {
                UnitId = createDto.UnitId,
                PartyId = createDto.PartyId,
                TransporterId = createDto.TransporterId,
                LrNo = createDto.LrNo,
                LrDate = DateOnly.FromDateTime(createDto.LrDate),
                BillDate = createDto.BillDate.HasValue ? DateOnly.FromDateTime(createDto.BillDate.Value) : null,
                BillNo = createDto.BillNo,
                TruckNo = createDto.TruckNo,
                LrWeight = createDto.LrWeight,
                RatePerQtl = createDto.RatePerQtl,
                LrQty = createDto.LrQty,
                LrAmount = createDto.LrAmount,
                Freight = createDto.Freight,
                OtherExpenses = createDto.OtherExpenses,
                TotalFreight = createDto.TotalFreight,
                TotalQty = createDto.TotalQty,
                BillAmount = createDto.BillAmount,
                OriginCityId = createDto.OriginCityId,
                DestinationCityId = createDto.DestinationCityId,
                DriverName = createDto.DriverName,
                DriverMobile = createDto.DriverMobile,
                Remarks = createDto.Remarks,
                Status = createDto.Status ?? "DRAFT",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.LrEntries.Add(entry);
            await _context.SaveChangesAsync();

            return await GetLREntryByIdAsync(entry.EntryId) ?? throw new Exception("Failed to create LR entry");
        }
        
        public async Task<bool> UpdateLREntryAsync(UpdateLREntryDTO updateDto)
        {
            var entry = await _context.LrEntries.FindAsync(updateDto.EntryId);
            if (entry == null) return false;

            entry.UnitId = updateDto.UnitId;
            entry.PartyId = updateDto.PartyId;
            entry.TransporterId = updateDto.TransporterId;
            entry.LrNo = updateDto.LrNo;
            entry.LrDate = DateOnly.FromDateTime(updateDto.LrDate);
            entry.BillDate = updateDto.BillDate.HasValue ? DateOnly.FromDateTime(updateDto.BillDate.Value) : null;
            entry.BillNo = updateDto.BillNo;
            entry.TruckNo = updateDto.TruckNo;
            entry.LrWeight = updateDto.LrWeight;
            entry.RatePerQtl = updateDto.RatePerQtl;
            entry.LrQty = updateDto.LrQty;
            entry.LrAmount = updateDto.LrAmount;
            entry.Freight = updateDto.Freight;
            entry.OtherExpenses = updateDto.OtherExpenses;
            entry.TotalFreight = updateDto.TotalFreight;
            entry.TotalQty = updateDto.TotalQty;
            entry.BillAmount = updateDto.BillAmount;
            entry.OriginCityId = updateDto.OriginCityId;
            entry.DestinationCityId = updateDto.DestinationCityId;
            entry.DriverName = updateDto.DriverName;
            entry.DriverMobile = updateDto.DriverMobile;
            entry.Remarks = updateDto.Remarks;
            entry.Status = updateDto.Status;
            entry.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteLREntryAsync(int id)
        {
            var entry = await _context.LrEntries.FindAsync(id);
            if (entry == null) return false;

            _context.LrEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }
        
        // Master Data
        public async Task<IEnumerable<UnitDTO>> GetUnitsAsync()
        {
            var units = await _context.Units
                .Include(u => u.City)
                .Where(u => u.IsActive == true)
                .OrderBy(u => u.UnitName)
                .ToListAsync();

            return units.Select(u => new UnitDTO
            {
                UnitId = u.UnitId,
                UnitName = u.UnitName,
                UnitCode = u.UnitCode,
                Address = u.Address,
                CityId = u.CityId,
                Pincode = u.Pincode,
                Phone = u.Phone,
                Email = u.Email,
                Gstin = u.Gstin,
                IsActive = u.IsActive ?? true,
                CityName = u.City?.CityName
            });
        }
        
        public async Task<IEnumerable<PartyDTO>> GetPartiesAsync()
        {
            var parties = await _context.LrParties
                .Include(p => p.City)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.PartyName)
                .ToListAsync();

            return parties.Select(p => new PartyDTO
            {
                PartyId = p.PartyId,
                PartyName = p.PartyName,
                PartyCode = p.PartyCode,
                ContactPerson = p.ContactPerson,
                Address1 = p.Address1,
                Address2 = p.Address2,
                CityId = p.CityId,
                Pincode = p.Pincode,
                Phone = p.Phone,
                Mobile = p.Mobile,
                Email = p.Email,
                Gstin = p.Gstin,
                Pan = p.Pan,
                IsActive = p.IsActive ?? true,
                CityName = p.City?.CityName
            });
        }
        
        public async Task<IEnumerable<TransporterDTO>> GetTransportersAsync()
        {
            var transporters = await _context.LrTransporters
                .Include(t => t.City)
                .Where(t => t.IsActive == true)
                .OrderBy(t => t.TransporterName)
                .ToListAsync();

            return transporters.Select(t => new TransporterDTO
            {
                TransporterId = t.TransporterId,
                TransporterName = t.TransporterName,
                TransporterCode = t.TransporterCode,
                ContactPerson = t.ContactPerson,
                Address = t.Address,
                CityId = t.CityId,
                Pincode = t.Pincode,
                Phone = t.Phone,
                Mobile = t.Mobile,
                Email = t.Email,
                Gstin = t.Gstin,
                Pan = t.Pan,
                IsActive = t.IsActive ?? true,
                CityName = t.City?.CityName
            });
        }
        
        public async Task<IEnumerable<CityDTO>> GetCitiesAsync()
        {
            var cities = await _context.Statecities
                .OrderBy(c => c.CityName)
                .ToListAsync();

            return cities.Select(c => new CityDTO
            {
                CityId = c.CityId,
                CityName = c.CityName,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                State = c.State
            });
        }
        
        public async Task<IEnumerable<CityDTO>> SearchCitiesAsync(string searchTerm)
        {
            var cities = await _context.Statecities
                .Where(c => c.CityName.Contains(searchTerm) || c.State.Contains(searchTerm))
                .OrderBy(c => c.CityName)
                .Take(50)
                .ToListAsync();

            return cities.Select(c => new CityDTO
            {
                CityId = c.CityId,
                CityName = c.CityName,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                State = c.State
            });
        }
        
        public async Task<CityDTO> CreateCityAsync(CreateCityDTO createDto)
        {
            // Get the next available city ID
            var maxCityId = await _context.Statecities
                .MaxAsync(c => (int?)c.CityId) ?? 0;
            
            var city = new Statecity
            {
                CityId = maxCityId + 1,
                CityName = createDto.CityName,
                Latitude = createDto.Latitude ?? "",
                Longitude = createDto.Longitude ?? "",
                State = createDto.State
            };

            _context.Statecities.Add(city);
            await _context.SaveChangesAsync();

            return new CityDTO
            {
                CityId = city.CityId,
                CityName = city.CityName,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                State = city.State
            };
        }
        
        public async Task<IEnumerable<DocumentTypeDTO>> GetDocumentTypesAsync()
        {
            var documentTypes = await _context.LrDocumentTypes
                .OrderBy(dt => dt.TypeName)
                .ToListAsync();

            return documentTypes.Select(dt => new DocumentTypeDTO
            {
                TypeId = dt.TypeId,
                TypeName = dt.TypeName,
                AllowedExtensions = dt.AllowedExtensions
            });
        }
        
        public async Task<IEnumerable<DocumentDTO>> GetDocumentsByLREntryIdAsync(int lrEntryId)
        {
            var documents = await _context.LrDocuments
                .Include(d => d.Type)
                .Where(d => d.LrEntryId == lrEntryId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return documents.Select(d => new DocumentDTO
            {
                DocumentId = d.DocumentId,
                LrEntryId = d.LrEntryId,
                TypeId = d.TypeId,
                DocumentName = d.DocumentName ?? string.Empty,
                UploadedAt = d.UploadedAt ?? DateTime.Now,
                TypeName = d.Type?.TypeName,
                AllowedExtensions = d.Type?.AllowedExtensions
            });
        }
        
        public async Task<DocumentDTO> UploadDocumentAsync(int lrEntryId, int typeId, string fileName)
        {
            var document = new LrDocument
            {
                LrEntryId = lrEntryId,
                TypeId = typeId,
                DocumentName = fileName,
                UploadedAt = DateTime.Now
            };

            _context.LrDocuments.Add(document);
            await _context.SaveChangesAsync();

            return new DocumentDTO
            {
                DocumentId = document.DocumentId,
                LrEntryId = document.LrEntryId,
                TypeId = document.TypeId,
                DocumentName = document.DocumentName ?? string.Empty,
                UploadedAt = document.UploadedAt ?? DateTime.Now
            };
        }
        
        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await _context.LrDocuments.FindAsync(documentId);
            if (document == null) return false;

            _context.LrDocuments.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 