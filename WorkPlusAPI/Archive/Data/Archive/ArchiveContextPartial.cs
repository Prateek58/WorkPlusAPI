using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.Models.Archive;

namespace WorkPlusAPI.Data;

public partial class ArchiveContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Add any additional manual configurations here
        // This method will be called after the scaffolded OnModelCreating
    }
} 