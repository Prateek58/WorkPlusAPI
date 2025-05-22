using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.DTOs
{
    public class UserRoleDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<RoleDTO> AssignedRoles { get; set; } = new List<RoleDTO>();
        public List<RoleDTO> AvailableRoles { get; set; } = new List<RoleDTO>();
    }

    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UserRoleAssignmentDTO
    {
        public int UserId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }
} 