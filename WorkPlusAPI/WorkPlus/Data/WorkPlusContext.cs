using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using WorkPlusAPI.WorkPlus.Model;

namespace WorkPlusAPI.WorkPlus.Data;

public partial class WorkPlusContext : DbContext
{
    public WorkPlusContext()
    {
    }

    public WorkPlusContext(DbContextOptions<WorkPlusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Efmigrationshistory> Efmigrationshistories { get; set; }

    public virtual DbSet<EmployeeType> EmployeeTypes { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobEntry> JobEntries { get; set; }

    public virtual DbSet<JobEntryWorker> JobEntryWorkers { get; set; }

    public virtual DbSet<JobGroup> JobGroups { get; set; }

    public virtual DbSet<JobType> JobTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=work_plus;user=root;password=root123;port=3306", Microsoft.EntityFrameworkCore.ServerVersion.Parse("11.7.2-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Efmigrationshistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId).HasName("PRIMARY");

            entity.ToTable("__efmigrationshistory");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<EmployeeType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PRIMARY");

            entity.ToTable("employee_types");

            entity.HasIndex(e => e.TypeName, "type_name").IsUnique();

            entity.Property(e => e.TypeId)
                .HasColumnType("int(11)")
                .HasColumnName("type_id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("group_members");

            entity.HasIndex(e => e.GroupId, "group_id");

            entity.HasIndex(e => e.WorkerId, "worker_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.GroupId)
                .HasColumnType("int(11)")
                .HasColumnName("group_id");
            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("group_members_ibfk_1");

            entity.HasOne(d => d.Worker).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.WorkerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("group_members_ibfk_2");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PRIMARY");

            entity.ToTable("jobs");

            entity.HasIndex(e => e.CreatedBy, "created_by");

            entity.HasIndex(e => e.JobTypeId, "job_type_id");

            entity.Property(e => e.JobId)
                .HasColumnType("int(11)")
                .HasColumnName("job_id");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("int(11)")
                .HasColumnName("created_by");
            entity.Property(e => e.ExpectedHours)
                .HasPrecision(5, 2)
                .HasColumnName("expected_hours");
            entity.Property(e => e.ExpectedItemsPerHour)
                .HasColumnType("int(11)")
                .HasColumnName("expected_items_per_hour");
            entity.Property(e => e.IncentiveBonusRate)
                .HasPrecision(10, 2)
                .HasColumnName("incentive_bonus_rate");
            entity.Property(e => e.IncentiveType)
                .HasDefaultValueSql("'PerUnit'")
                .HasColumnType("enum('PerUnit','Percentage')")
                .HasColumnName("incentive_type");
            entity.Property(e => e.JobName)
                .HasMaxLength(100)
                .HasColumnName("job_name");
            entity.Property(e => e.JobTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("job_type_id");
            entity.Property(e => e.PenaltyRate)
                .HasPrecision(10, 2)
                .HasColumnName("penalty_rate");
            entity.Property(e => e.RatePerHour)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_hour");
            entity.Property(e => e.RatePerItem)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_item");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("jobs_ibfk_2");

            entity.HasOne(d => d.JobType).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.JobTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("jobs_ibfk_1");
        });

        modelBuilder.Entity<JobEntry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PRIMARY");

            entity.ToTable("job_entries");

            entity.HasIndex(e => e.CreatedBy, "created_by");

            entity.HasIndex(e => e.GroupId, "group_id");

            entity.HasIndex(e => e.JobId, "job_id");

            entity.HasIndex(e => e.WorkerId, "worker_id");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("int(11)")
                .HasColumnName("created_by");
            entity.Property(e => e.EntryType)
                .HasColumnType("enum('Individual','Group')")
                .HasColumnName("entry_type");
            entity.Property(e => e.ExpectedHours)
                .HasPrecision(5, 2)
                .HasColumnName("expected_hours");
            entity.Property(e => e.ExtraHours)
                .HasPrecision(5, 2)
                .HasColumnName("extra_hours");
            entity.Property(e => e.GroupId)
                .HasColumnType("int(11)")
                .HasColumnName("group_id");
            entity.Property(e => e.HoursTaken)
                .HasPrecision(5, 2)
                .HasColumnName("hours_taken");
            entity.Property(e => e.IncentiveAmount)
                .HasPrecision(10, 2)
                .HasColumnName("incentive_amount");
            entity.Property(e => e.IsFinalized)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_finalized");
            entity.Property(e => e.IsPostLunch).HasColumnName("is_post_lunch");
            entity.Property(e => e.ItemsCompleted)
                .HasColumnType("int(11)")
                .HasColumnName("items_completed");
            entity.Property(e => e.JobId)
                .HasColumnType("int(11)")
                .HasColumnName("job_id");
            entity.Property(e => e.ProductiveHours)
                .HasPrecision(5, 2)
                .HasColumnName("productive_hours");
            entity.Property(e => e.RatePerJob)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_job");
            entity.Property(e => e.Remarks)
                .HasColumnType("text")
                .HasColumnName("remarks");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasComputedColumnSql("case when `items_completed` is not null then `items_completed` * `rate_per_job` when `hours_taken` is not null then `hours_taken` * `rate_per_job` else 0 end", true)
                .HasColumnName("total_amount");
            entity.Property(e => e.UnderperformanceHours)
                .HasPrecision(5, 2)
                .HasColumnName("underperformance_hours");
            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.JobEntries)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_entries_ibfk_4");

            entity.HasOne(d => d.Group).WithMany(p => p.JobEntries)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("job_entries_ibfk_3");

            entity.HasOne(d => d.Job).WithMany(p => p.JobEntries)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_entries_ibfk_1");

            entity.HasOne(d => d.Worker).WithMany(p => p.JobEntries)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("job_entries_ibfk_2");
        });

        modelBuilder.Entity<JobEntryWorker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("job_entry_workers");

            entity.HasIndex(e => e.EntryId, "entry_id");

            entity.HasIndex(e => e.WorkerId, "worker_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");

            entity.HasOne(d => d.Entry).WithMany(p => p.JobEntryWorkers)
                .HasForeignKey(d => d.EntryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_entry_workers_ibfk_1");

            entity.HasOne(d => d.Worker).WithMany(p => p.JobEntryWorkers)
                .HasForeignKey(d => d.WorkerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("job_entry_workers_ibfk_2");
        });

        modelBuilder.Entity<JobGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PRIMARY");

            entity.ToTable("job_groups");

            entity.Property(e => e.GroupId)
                .HasColumnType("int(11)")
                .HasColumnName("group_id");
            entity.Property(e => e.GroupName)
                .HasMaxLength(100)
                .HasColumnName("group_name");
            entity.Property(e => e.MaxWorkers)
                .HasColumnType("int(11)")
                .HasColumnName("max_workers");
            entity.Property(e => e.MinWorkers)
                .HasColumnType("int(11)")
                .HasColumnName("min_workers");
        });

        modelBuilder.Entity<JobType>(entity =>
        {
            entity.HasKey(e => e.JobTypeId).HasName("PRIMARY");

            entity.ToTable("job_types");

            entity.Property(e => e.JobTypeId)
                .HasColumnType("int(11)")
                .HasColumnName("job_type_id");
            entity.Property(e => e.TypeName)
                .HasColumnType("enum('RatePerItem','Hourly')")
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "Name").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.Username, "Username").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Userrole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("userroles_ibfk_2"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("userroles_ibfk_1"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("userroles");
                        j.HasIndex(new[] { "RoleId" }, "RoleId");
                        j.IndexerProperty<int>("UserId").HasColumnType("int(11)");
                        j.IndexerProperty<int>("RoleId").HasColumnType("int(11)");
                    });
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.WorkerId).HasName("PRIMARY");

            entity.ToTable("workers");

            entity.HasIndex(e => e.TypeId, "fk_workers_employee_type");

            entity.HasIndex(e => e.UserId, "user_id").IsUnique();

            entity.Property(e => e.WorkerId)
                .HasColumnType("int(11)")
                .HasColumnName("worker_id");
            entity.Property(e => e.AgeAtJoining)
                .HasColumnType("int(11)")
                .HasColumnName("age_at_joining");
            entity.Property(e => e.BankAccountNo)
                .HasMaxLength(50)
                .HasColumnName("bank_account_no");
            entity.Property(e => e.BankLocation)
                .HasMaxLength(100)
                .HasColumnName("bank_location");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .HasColumnName("bank_name");
            entity.Property(e => e.BankRtgsCode)
                .HasMaxLength(20)
                .HasColumnName("bank_rtgs_code");
            entity.Property(e => e.BirthCity)
                .HasMaxLength(100)
                .HasColumnName("birth_city");
            entity.Property(e => e.BirthPlace)
                .HasMaxLength(100)
                .HasColumnName("birth_place");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(10)
                .HasColumnName("blood_group");
            entity.Property(e => e.DateOfJoining).HasColumnName("date_of_joining");
            entity.Property(e => e.DateOfLeaving).HasColumnName("date_of_leaving");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EsiApplicable)
                .HasDefaultValueSql("'0'")
                .HasColumnName("esi_applicable");
            entity.Property(e => e.EsiLocation)
                .HasMaxLength(100)
                .HasColumnName("esi_location");
            entity.Property(e => e.FatherName)
                .HasMaxLength(100)
                .HasColumnName("father_name");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasColumnType("enum('Male','Female','Other')")
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MotherName)
                .HasMaxLength(100)
                .HasColumnName("mother_name");
            entity.Property(e => e.NomineeAge)
                .HasColumnType("int(11)")
                .HasColumnName("nominee_age");
            entity.Property(e => e.NomineeName)
                .HasMaxLength(100)
                .HasColumnName("nominee_name");
            entity.Property(e => e.NomineeRelation)
                .HasMaxLength(50)
                .HasColumnName("nominee_relation");
            entity.Property(e => e.Pan)
                .HasMaxLength(20)
                .HasColumnName("pan");
            entity.Property(e => e.PermanentAddress1)
                .HasMaxLength(200)
                .HasColumnName("permanent_address1");
            entity.Property(e => e.PermanentAddress2)
                .HasMaxLength(200)
                .HasColumnName("permanent_address2");
            entity.Property(e => e.PermanentAddress3)
                .HasMaxLength(200)
                .HasColumnName("permanent_address3");
            entity.Property(e => e.PermanentCity)
                .HasMaxLength(100)
                .HasColumnName("permanent_city");
            entity.Property(e => e.PermanentState)
                .HasMaxLength(100)
                .HasColumnName("permanent_state");
            entity.Property(e => e.PfNo)
                .HasMaxLength(50)
                .HasColumnName("pf_no");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            entity.Property(e => e.PresentAddress1)
                .HasMaxLength(200)
                .HasColumnName("present_address1");
            entity.Property(e => e.PresentAddress2)
                .HasMaxLength(200)
                .HasColumnName("present_address2");
            entity.Property(e => e.PresentAddress3)
                .HasMaxLength(200)
                .HasColumnName("present_address3");
            entity.Property(e => e.PresentCity)
                .HasMaxLength(100)
                .HasColumnName("present_city");
            entity.Property(e => e.PresentState)
                .HasMaxLength(100)
                .HasColumnName("present_state");
            entity.Property(e => e.ReferenceName)
                .HasMaxLength(100)
                .HasColumnName("reference_name");
            entity.Property(e => e.Remarks)
                .HasColumnType("text")
                .HasColumnName("remarks");
            entity.Property(e => e.TypeId)
                .HasColumnType("int(11)")
                .HasColumnName("type_id");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Type).WithMany(p => p.Workers)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workers_employee_type");

            entity.HasOne(d => d.User).WithOne(p => p.Worker)
                .HasForeignKey<Worker>(d => d.UserId)
                .HasConstraintName("workers_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
