using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class EmployeeTypeDTO
    {
        public int TypeId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TypeName { get; set; } = string.Empty;
    }
} 