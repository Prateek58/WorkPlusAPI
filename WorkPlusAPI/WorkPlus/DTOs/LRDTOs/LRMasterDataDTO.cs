using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.WorkPlus.DTOs.LRDTOs
{
    public class UnitDTO
    {
        public int UnitId { get; set; }
        
        [Required(ErrorMessage = "Unit name is required")]
        [StringLength(100, ErrorMessage = "Unit name cannot exceed 100 characters")]
        public string UnitName { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Unit code cannot exceed 20 characters")]
        public string? UnitCode { get; set; }
        
        public string? Address { get; set; }
        public int? CityId { get; set; }
        
        [StringLength(10, ErrorMessage = "Pincode cannot exceed 10 characters")]
        public string? Pincode { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }
        
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        
        [StringLength(15, ErrorMessage = "GSTIN cannot exceed 15 characters")]
        public string? Gstin { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public string? CityName { get; set; }
    }
    
    public class PartyDTO
    {
        public int PartyId { get; set; }
        
        [Required(ErrorMessage = "Party name is required")]
        [StringLength(200, ErrorMessage = "Party name cannot exceed 200 characters")]
        public string PartyName { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Party code cannot exceed 20 characters")]
        public string? PartyCode { get; set; }
        
        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }
        
        [StringLength(200, ErrorMessage = "Address1 cannot exceed 200 characters")]
        public string? Address1 { get; set; }
        
        [StringLength(200, ErrorMessage = "Address2 cannot exceed 200 characters")]
        public string? Address2 { get; set; }
        
        public int? CityId { get; set; }
        
        [StringLength(10, ErrorMessage = "Pincode cannot exceed 10 characters")]
        public string? Pincode { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }
        
        [StringLength(15, ErrorMessage = "Mobile cannot exceed 15 characters")]
        public string? Mobile { get; set; }
        
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        
        [StringLength(15, ErrorMessage = "GSTIN cannot exceed 15 characters")]
        public string? Gstin { get; set; }
        
        [StringLength(10, ErrorMessage = "PAN cannot exceed 10 characters")]
        public string? Pan { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public string? CityName { get; set; }
    }
    
    public class TransporterDTO
    {
        public int TransporterId { get; set; }
        
        [Required(ErrorMessage = "Transporter name is required")]
        [StringLength(200, ErrorMessage = "Transporter name cannot exceed 200 characters")]
        public string TransporterName { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Transporter code cannot exceed 20 characters")]
        public string? TransporterCode { get; set; }
        
        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }
        
        public string? Address { get; set; }
        public int? CityId { get; set; }
        
        [StringLength(10, ErrorMessage = "Pincode cannot exceed 10 characters")]
        public string? Pincode { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }
        
        [StringLength(15, ErrorMessage = "Mobile cannot exceed 15 characters")]
        public string? Mobile { get; set; }
        
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        
        [StringLength(15, ErrorMessage = "GSTIN cannot exceed 15 characters")]
        public string? Gstin { get; set; }
        
        [StringLength(10, ErrorMessage = "PAN cannot exceed 10 characters")]
        public string? Pan { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public string? CityName { get; set; }
    }
    
    public class CityDTO
    {
        public int CityId { get; set; }
        
        [Required(ErrorMessage = "City name is required")]
        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string CityName { get; set; } = string.Empty;
        
        [StringLength(10, ErrorMessage = "Latitude cannot exceed 10 characters")]
        public string? Latitude { get; set; }
        
        [StringLength(10, ErrorMessage = "Longitude cannot exceed 10 characters")]
        public string? Longitude { get; set; }
        
        [Required(ErrorMessage = "State is required")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        public string State { get; set; } = string.Empty;
    }
    
    public class DocumentTypeDTO
    {
        public int TypeId { get; set; }
        
        [Required(ErrorMessage = "Type name is required")]
        [StringLength(50, ErrorMessage = "Type name cannot exceed 50 characters")]
        public string TypeName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Allowed extensions are required")]
        [StringLength(100, ErrorMessage = "Allowed extensions cannot exceed 100 characters")]
        public string AllowedExtensions { get; set; } = string.Empty;
    }
    
    public class DocumentDTO
    {
        public int DocumentId { get; set; }
        
        [Required(ErrorMessage = "LR Entry ID is required")]
        public int LrEntryId { get; set; }
        
        [Required(ErrorMessage = "Document type is required")]
        public int TypeId { get; set; }
        
        [Required(ErrorMessage = "Document name is required")]
        [StringLength(255, ErrorMessage = "Document name cannot exceed 255 characters")]
        public string DocumentName { get; set; } = string.Empty;
        
        public DateTime UploadedAt { get; set; }
        
        // Navigation Properties
        public string? TypeName { get; set; }
        public string? AllowedExtensions { get; set; }
    }
    
    public class CreateCityDTO
    {
        [Required(ErrorMessage = "City name is required")]
        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string CityName { get; set; } = string.Empty;
        
        [StringLength(10, ErrorMessage = "Latitude cannot exceed 10 characters")]
        public string? Latitude { get; set; }
        
        [StringLength(10, ErrorMessage = "Longitude cannot exceed 10 characters")]
        public string? Longitude { get; set; }
        
        [Required(ErrorMessage = "State is required")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
        public string State { get; set; } = string.Empty;
    }
} 