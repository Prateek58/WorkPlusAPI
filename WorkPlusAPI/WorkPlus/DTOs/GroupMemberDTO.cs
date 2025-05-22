using System.ComponentModel.DataAnnotations;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class GroupMemberDTO
    {
        public int Id { get; set; }
        
        [Required]
        public int GroupId { get; set; }
        
        [Required]
        public int WorkerId { get; set; }
        
        // Navigation properties for display purposes
        public string? GroupName { get; set; }
        public string? WorkerName { get; set; }
    }
    
    public class GroupMemberCreateDTO
    {
        [Required]
        public int GroupId { get; set; }
        
        [Required]
        public int WorkerId { get; set; }
    }
} 