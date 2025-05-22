using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class JobTypeDTO
    {
        public int JobTypeId { get; set; }
        
        [Required]
        public string TypeName { get; set; } = string.Empty;
    }
} 