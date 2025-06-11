using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using WorkPlusAPI.WorkPlus.Model.HR;

namespace WorkPlusAPI.WorkPlus.Data;

public partial class HRDbContext : DbContext
{
    public HRDbContext()
    {
    }

    public HRDbContext(DbContextOptions<HRDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HrAttendance> HrAttendances { get; set; }

    public virtual DbSet<HrLeaveBalance> HrLeaveBalances { get; set; }

    public virtual DbSet<HrLeaveRequest> HrLeaveRequests { get; set; }

    public virtual DbSet<HrMasterCalendarConfig> HrMasterCalendarConfigs { get; set; }

    public virtual DbSet<HrMasterConfig> HrMasterConfigs { get; set; }

    public virtual DbSet<HrMasterHoliday> HrMasterHolidays { get; set; }

    public virtual DbSet<HrMasterLeaveType> HrMasterLeaveTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured via dependency injection in Program.cs
        // This method intentionally left minimal to ensure DI configuration is used
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<HrAttendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hr_attendance");

            entity.HasIndex(e => e.LeaveTypeId, "fk_attendance_leave_type");

            entity.HasIndex(e => e.AttendanceDate, "hr_idx_attendance_date_parts");

            entity.HasIndex(e => e.MarkedBy, "idx_marked_by");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => new { e.WorkerId, e.AttendanceDate }, "idx_worker_date").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CheckInTime)
                .HasColumnType("time")
                .HasColumnName("check_in_time");
            entity.Property(e => e.CheckOutTime)
                .HasColumnType("time")
                .HasColumnName("check_out_time");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HalfDayType)
                .HasColumnType("enum('First Half','Second Half')")
                .HasColumnName("half_day_type");
            entity.Property(e => e.LeaveTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("leave_type_id");
            entity.Property(e => e.MarkedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("marked_at");
            entity.Property(e => e.MarkedBy)
                .HasColumnType("int(11)")
                .HasColumnName("marked_by");
            entity.Property(e => e.Remarks)
                .HasColumnType("text")
                .HasColumnName("remarks");
            entity.Property(e => e.Shift)
                .HasDefaultValueSql("'Full Day'")
                .HasColumnType("enum('Morning','Evening','Full Day')")
                .HasColumnName("shift");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Present'")
                .HasColumnType("enum('Present','Absent','Half Day','Leave','CompOff','Holiday')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.HrAttendances)
                .HasForeignKey(d => d.LeaveTypeId)
                .HasConstraintName("fk_attendance_leave_type");
        });

        modelBuilder.Entity<HrLeaveBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hr_leave_balance");

            entity.HasIndex(e => e.LeaveTypeId, "idx_leave_type");

            entity.HasIndex(e => new { e.WorkerId, e.Year }, "idx_worker_year");

            entity.HasIndex(e => new { e.WorkerId, e.LeaveTypeId, e.Year }, "uk_worker_leave_year").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Allocated)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("allocated");
            entity.Property(e => e.Balance)
                .HasPrecision(5, 2)
                .HasComputedColumnSql("`allocated` - `used`", true)
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LeaveTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("leave_type_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Used)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("used");
            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");
            entity.Property(e => e.Year)
                .HasColumnType("year(4)")
                .HasColumnName("year");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.HrLeaveBalances)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_balance_leave_type");
        });

        modelBuilder.Entity<HrLeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hr_leave_requests");

            entity.HasIndex(e => e.LeaveTypeId, "fk_request_leave_type");

            entity.HasIndex(e => new { e.FromDate, e.ToDate, e.Status }, "hr_idx_leave_requests_dates");

            entity.HasIndex(e => e.ApprovedBy, "idx_approved_by");

            entity.HasIndex(e => new { e.FromDate, e.ToDate }, "idx_dates");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.WorkerId, "idx_worker");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.ApprovedAt)
                .HasColumnType("datetime")
                .HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy)
                .HasColumnType("int(11)")
                .HasColumnName("approved_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FromDate).HasColumnName("from_date");
            entity.Property(e => e.LeaveTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("leave_type_id");
            entity.Property(e => e.Reason)
                .HasColumnType("text")
                .HasColumnName("reason");
            entity.Property(e => e.RejectionReason)
                .HasColumnType("text")
                .HasColumnName("rejection_reason");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("requested_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Pending'")
                .HasColumnType("enum('Pending','Approved','Rejected','Cancelled')")
                .HasColumnName("status");
            entity.Property(e => e.ToDate).HasColumnName("to_date");
            entity.Property(e => e.TotalDays)
                .HasPrecision(5, 2)
                .HasColumnName("total_days");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.HrLeaveRequests)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_request_leave_type");
        });

        modelBuilder.Entity<HrMasterCalendarConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hr_master_calendar_config");

            entity.HasIndex(e => e.DayOfWeek, "uk_day_of_week").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DayOfWeek)
                .HasColumnType("enum('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday')")
                .HasColumnName("day_of_week");
            entity.Property(e => e.IsWorkingDay)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_working_day");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<HrMasterConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigKey).HasName("PRIMARY");

            entity.ToTable("hr_master_config");

            entity.Property(e => e.ConfigKey)
                .HasMaxLength(50)
                .HasColumnName("config_key");
            entity.Property(e => e.ConfigValue)
                .HasMaxLength(100)
                .HasColumnName("config_value");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<HrMasterHoliday>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hr_master_holidays");

            entity.HasIndex(e => new { e.HolidayDate, e.IsActive }, "idx_date_active");

            entity.HasIndex(e => e.HolidayDate, "uk_holiday_date").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.IsOptional)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_optional");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<HrMasterLeaveType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hr_master_leave_types");

            entity.HasIndex(e => e.Code, "code").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_active");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.AppliesTo)
                .HasDefaultValueSql("'All'")
                .HasColumnType("enum('FullTime','PartTime','Contract','All')")
                .HasColumnName("applies_to");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.IsPaid)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_paid");
            entity.Property(e => e.MaxDaysPerYear)
                .HasColumnType("int(11)")
                .HasColumnName("max_days_per_year");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
