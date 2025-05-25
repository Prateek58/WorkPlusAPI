using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.WorkPlus.DTOs.LRDTOs
{
    public class LREntryDTO
    {
        public int EntryId { get; set; }
        
        // LEGACY REQUIRED FIELDS
        [Required(ErrorMessage = "Unit is required")]
        public int UnitId { get; set; }
        
        [Required(ErrorMessage = "Party is required")]
        public int PartyId { get; set; }
        
        [Required(ErrorMessage = "Transporter is required")]
        public int TransporterId { get; set; }
        
        [Required(ErrorMessage = "LR Number is required")]
        [StringLength(50, ErrorMessage = "LR Number cannot exceed 50 characters")]
        public string LrNo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "LR Date is required")]
        public DateTime LrDate { get; set; }
        
        // LEGACY OPTIONAL FIELDS
        public DateTime? BillDate { get; set; }
        
        [StringLength(50, ErrorMessage = "Bill Number cannot exceed 50 characters")]
        public string? BillNo { get; set; }
        
        [StringLength(20, ErrorMessage = "Truck Number cannot exceed 20 characters")]
        public string? TruckNo { get; set; }
        
        public decimal LrWeight { get; set; } = 0;
        public decimal RatePerQtl { get; set; } = 0;
        public decimal LrQty { get; set; } = 0;
        public decimal LrAmount { get; set; } = 0;
        public decimal Freight { get; set; } = 0;
        public decimal OtherExpenses { get; set; } = 0;
        public decimal TotalFreight { get; set; } = 0;
        public decimal TotalQty { get; set; } = 0;
        public decimal BillAmount { get; set; } = 0;
        
        // CITY REFERENCES
        public int? OriginCityId { get; set; }
        public int? DestinationCityId { get; set; }
        
        // NEW OPTIONAL FIELDS
        [StringLength(100, ErrorMessage = "Driver Name cannot exceed 100 characters")]
        public string? DriverName { get; set; }
        
        [StringLength(15, ErrorMessage = "Driver Mobile cannot exceed 15 characters")]
        public string? DriverMobile { get; set; }
        
        public string? Remarks { get; set; }
        
        public string Status { get; set; } = "DRAFT";
        
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation Properties for Display
        public string? UnitName { get; set; }
        public string? PartyName { get; set; }
        public string? TransporterName { get; set; }
        public string? OriginCityName { get; set; }
        public string? DestinationCityName { get; set; }
    }
    
    public class CreateLREntryDTO
    {
        [Required(ErrorMessage = "Unit is required")]
        public int UnitId { get; set; }
        
        [Required(ErrorMessage = "Party is required")]
        public int PartyId { get; set; }
        
        [Required(ErrorMessage = "Transporter is required")]
        public int TransporterId { get; set; }
        
        [Required(ErrorMessage = "LR Number is required")]
        [StringLength(50, ErrorMessage = "LR Number cannot exceed 50 characters")]
        public string LrNo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "LR Date is required")]
        public DateTime LrDate { get; set; }
        
        public DateTime? BillDate { get; set; }
        public string? BillNo { get; set; }
        public string? TruckNo { get; set; }
        public decimal LrWeight { get; set; } = 0;
        public decimal RatePerQtl { get; set; } = 0;
        public decimal LrQty { get; set; } = 0;
        public decimal LrAmount { get; set; } = 0;
        public decimal Freight { get; set; } = 0;
        public decimal OtherExpenses { get; set; } = 0;
        public decimal TotalFreight { get; set; } = 0;
        public decimal TotalQty { get; set; } = 0;
        public decimal BillAmount { get; set; } = 0;
        public int? OriginCityId { get; set; }
        public int? DestinationCityId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverMobile { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } = "DRAFT";
    }
    
    public class UpdateLREntryDTO : CreateLREntryDTO
    {
        [Required]
        public int EntryId { get; set; }
    }
} 