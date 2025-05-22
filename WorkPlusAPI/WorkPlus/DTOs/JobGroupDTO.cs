using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class JobGroupDTO
    {
        public int GroupId { get; set; }
        
        [Required]
        public string GroupName { get; set; } = string.Empty;
        
        [Required]
        public int MinWorkers { get; set; }
        
        [Required]
        public int MaxWorkers { get; set; }
    }
} 