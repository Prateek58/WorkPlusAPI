using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using WorkPlusAPI.Models.Archive;

namespace WorkPlusAPI.Data;

public partial class ArchiveContext : DbContext
{
    public ArchiveContext()
    {
    }

    public ArchiveContext(DbContextOptions<ArchiveContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DboCity> DboCities { get; set; }

    public virtual DbSet<DboLoginType> DboLoginTypes { get; set; }

    public virtual DbSet<DboState> DboStates { get; set; }

    public virtual DbSet<DboUnit> DboUnits { get; set; }

    public virtual DbSet<DboUser> DboUsers { get; set; }

    public virtual DbSet<JwEmployee> JwEmployees { get; set; }

    public virtual DbSet<JwEmployeeType> JwEmployeeTypes { get; set; }

    public virtual DbSet<JwEntry> JwEntries { get; set; }

    public virtual DbSet<JwWork> JwWorks { get; set; }

    public virtual DbSet<JwWorkGroup> JwWorkGroups { get; set; }

    public virtual DbSet<JwWorkType> JwWorkTypes { get; set; }

    public virtual DbSet<LrDocument> LrDocuments { get; set; }

    public virtual DbSet<LrEntry> LrEntries { get; set; }

    public virtual DbSet<LrParty> LrParties { get; set; }

    public virtual DbSet<LrTransporter> LrTransporters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=work_plus_archive;user=root;password=root123;port=3306", Microsoft.EntityFrameworkCore.ServerVersion.Parse("11.7.2-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<DboCity>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PRIMARY");

            entity.ToTable("dbo_cities");

            entity.HasIndex(e => e.StateId, "state_id");

            entity.Property(e => e.CityId)
                .HasColumnType("int(11)")
                .HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(100)
                .HasColumnName("city_name");
            entity.Property(e => e.CountryName)
                .HasMaxLength(100)
                .HasColumnName("country_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.PinCode)
                .HasMaxLength(10)
                .HasColumnName("pin_code");
            entity.Property(e => e.StateId)
                .HasColumnType("int(11)")
                .HasColumnName("state_id");
            entity.Property(e => e.StateName)
                .HasMaxLength(100)
                .HasColumnName("state_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.State).WithMany(p => p.DboCities)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("dbo_cities_ibfk_1");
        });

        modelBuilder.Entity<DboLoginType>(entity =>
        {
            entity.HasKey(e => e.LoginTypeId).HasName("PRIMARY");

            entity.ToTable("dbo_login_types");

            entity.Property(e => e.LoginTypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("login_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.LoginTypeName)
                .HasMaxLength(50)
                .HasColumnName("login_type_name");
        });

        modelBuilder.Entity<DboState>(entity =>
        {
            entity.HasKey(e => e.StateId).HasName("PRIMARY");

            entity.ToTable("dbo_states");

            entity.Property(e => e.StateId)
                .HasColumnType("int(11)")
                .HasColumnName("state_id");
            entity.Property(e => e.CountryName)
                .HasMaxLength(50)
                .HasDefaultValueSql("'India'")
                .HasColumnName("country_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.StateName)
                .HasMaxLength(50)
                .HasColumnName("state_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<DboUnit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PRIMARY");

            entity.ToTable("dbo_units");

            entity.Property(e => e.UnitId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("unit_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UnitName)
                .HasMaxLength(50)
                .HasColumnName("unit_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<DboUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("dbo_users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.LoginTypeId, "login_type_id");

            entity.Property(e => e.UserId)
                .HasColumnType("smallint(6)")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(50)
                .HasColumnName("display_name");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_admin");
            entity.Property(e => e.IsEnabled)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_enabled");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.LoginTypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("login_type_id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LoginType).WithMany(p => p.DboUsers)
                .HasForeignKey(d => d.LoginTypeId)
                .HasConstraintName("dbo_users_ibfk_1");
        });

        modelBuilder.Entity<JwEmployee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PRIMARY");

            entity.ToTable("jw_employees");

            entity.HasIndex(e => e.EmployeeTypeId, "employee_type_id");

            entity.Property(e => e.EmployeeId)
                .HasColumnType("int(11)")
                .HasColumnName("employee_id");
            entity.Property(e => e.AgeAtJoining)
                .HasColumnType("tinyint(4)")
                .HasColumnName("age_at_joining");
            entity.Property(e => e.BankAccountNo)
                .HasMaxLength(50)
                .HasColumnName("bank_account_no");
            entity.Property(e => e.BankBranch)
                .HasMaxLength(50)
                .HasColumnName("bank_branch");
            entity.Property(e => e.BankLocation)
                .HasMaxLength(50)
                .HasColumnName("bank_location");
            entity.Property(e => e.BankName)
                .HasMaxLength(50)
                .HasColumnName("bank_name");
            entity.Property(e => e.BankRtgsCode)
                .HasMaxLength(50)
                .HasColumnName("bank_rtgs_code");
            entity.Property(e => e.BirthDistrict)
                .HasMaxLength(100)
                .HasColumnName("birth_district");
            entity.Property(e => e.BirthPlace)
                .HasMaxLength(100)
                .HasColumnName("birth_place");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(3)
                .HasColumnName("blood_group");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DateOfJoining).HasColumnName("date_of_joining");
            entity.Property(e => e.DateOfLeaving).HasColumnName("date_of_leaving");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmployeeTypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("employee_type_id");
            entity.Property(e => e.EsiApplicable).HasColumnName("esi_applicable");
            entity.Property(e => e.EsiDispensoryNo)
                .HasMaxLength(100)
                .HasColumnName("esi_dispensory_no");
            entity.Property(e => e.EsiLocation)
                .HasMaxLength(100)
                .HasColumnName("esi_location");
            entity.Property(e => e.EsiNo)
                .HasMaxLength(55)
                .HasColumnName("esi_no");
            entity.Property(e => e.FatherName)
                .HasMaxLength(100)
                .HasColumnName("father_name");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.MotherName)
                .HasMaxLength(100)
                .HasColumnName("mother_name");
            entity.Property(e => e.NameDisplay)
                .HasMaxLength(100)
                .HasColumnName("name_display");
            entity.Property(e => e.NomineeAge)
                .HasColumnType("tinyint(4)")
                .HasColumnName("nominee_age");
            entity.Property(e => e.NomineeName)
                .HasMaxLength(100)
                .HasColumnName("nominee_name");
            entity.Property(e => e.NomineeRelation)
                .HasMaxLength(100)
                .HasColumnName("nominee_relation");
            entity.Property(e => e.Pan)
                .HasMaxLength(20)
                .HasColumnName("pan");
            entity.Property(e => e.PfApplicable).HasColumnName("pf_applicable");
            entity.Property(e => e.PfNo)
                .HasMaxLength(50)
                .HasColumnName("pf_no");
            entity.Property(e => e.PhoneNo)
                .HasMaxLength(20)
                .HasColumnName("phone_no");
            entity.Property(e => e.PresentAddress1)
                .HasMaxLength(50)
                .HasColumnName("present_address1");
            entity.Property(e => e.PresentAddress2)
                .HasMaxLength(50)
                .HasColumnName("present_address2");
            entity.Property(e => e.PresentAddress3)
                .HasMaxLength(50)
                .HasColumnName("present_address3");
            entity.Property(e => e.PresentCity)
                .HasMaxLength(100)
                .HasColumnName("present_city");
            entity.Property(e => e.PresentState)
                .HasMaxLength(100)
                .HasColumnName("present_state");
            entity.Property(e => e.RatePerDay)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_day");
            entity.Property(e => e.RatePerHour)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_hour");
            entity.Property(e => e.ReferenceName)
                .HasMaxLength(100)
                .HasColumnName("reference_name");
            entity.Property(e => e.Remarks)
                .HasMaxLength(155)
                .HasColumnName("remarks");
            entity.Property(e => e.SalaryIncrementDueDate).HasColumnName("salary_increment_due_date");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.EmployeeType).WithMany(p => p.JwEmployees)
                .HasForeignKey(d => d.EmployeeTypeId)
                .HasConstraintName("jw_employees_ibfk_1");
        });

        modelBuilder.Entity<JwEmployeeType>(entity =>
        {
            entity.HasKey(e => e.EmployeeTypeId).HasName("PRIMARY");

            entity.ToTable("jw_employee_types");

            entity.Property(e => e.EmployeeTypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("employee_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<JwEntry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PRIMARY");

            entity.ToTable("jw_entries");

            entity.Property(e => e.EntryId)
                .HasColumnType("bigint(20)")
                .HasColumnName("entry_id");
            entity.Property(e => e.ApprovedBy)
                .HasColumnType("int(11)")
                .HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("datetime")
                .HasColumnName("approved_on");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployeeId)
                .HasColumnType("int(11)")
                .HasColumnName("employee_id");
            entity.Property(e => e.EntryByUserId)
                .HasColumnType("smallint(6)")
                .HasColumnName("entry_by_user_id");
            entity.Property(e => e.EntryDate)
                .HasColumnType("datetime")
                .HasColumnName("entry_date");
            entity.Property(e => e.IsApproved)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_approved");
            entity.Property(e => e.JwNo)
                .HasColumnType("int(11)")
                .HasColumnName("jw_no");
            entity.Property(e => e.QtyHours)
                .HasPrecision(10, 2)
                .HasColumnName("qty_hours");
            entity.Property(e => e.QtyItems)
                .HasPrecision(10, 2)
                .HasColumnName("qty_items");
            entity.Property(e => e.RateForJob)
                .HasPrecision(10, 2)
                .HasColumnName("rate_for_job");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UnitId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("unit_id");
            entity.Property(e => e.WorkId)
                .HasColumnType("int(11)")
                .HasColumnName("work_id");
        });

        modelBuilder.Entity<JwWork>(entity =>
        {
            entity.HasKey(e => e.WorkId).HasName("PRIMARY");

            entity.ToTable("jw_works");

            entity.Property(e => e.WorkId)
                .HasColumnType("int(11)")
                .HasColumnName("work_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupId)
                .HasColumnType("int(11)")
                .HasColumnName("group_id");
            entity.Property(e => e.RatePerPiece)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_piece");
            entity.Property(e => e.TimePerPiece)
                .HasColumnType("tinyint(4)")
                .HasColumnName("time_per_piece");
            entity.Property(e => e.WorkName)
                .HasMaxLength(50)
                .HasColumnName("work_name");
            entity.Property(e => e.WorkTypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("work_type_id");
        });

        modelBuilder.Entity<JwWorkGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PRIMARY");

            entity.ToTable("jw_work_groups");

            entity.Property(e => e.GroupId)
                .HasColumnType("int(11)")
                .HasColumnName("group_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.GroupName)
                .HasMaxLength(50)
                .HasColumnName("group_name");
        });

        modelBuilder.Entity<JwWorkType>(entity =>
        {
            entity.HasKey(e => e.WorkTypeId).HasName("PRIMARY");

            entity.ToTable("jw_work_types");

            entity.Property(e => e.WorkTypeId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("work_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<LrDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PRIMARY");

            entity.ToTable("lr_documents");

            entity.HasIndex(e => e.LrEntryId, "lr_entry_id");

            entity.Property(e => e.DocumentId)
                .HasColumnType("bigint(20)")
                .HasColumnName("document_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FileName)
                .HasMaxLength(100)
                .HasColumnName("file_name");
            entity.Property(e => e.LrEntryId)
                .HasColumnType("bigint(20)")
                .HasColumnName("lr_entry_id");
        });

        modelBuilder.Entity<LrEntry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PRIMARY");

            entity.ToTable("lr_entries");

            entity.HasIndex(e => e.CityId, "city_id");

            entity.HasIndex(e => e.PartyId, "party_id");

            entity.HasIndex(e => e.TransporterId, "transporter_id");

            entity.HasIndex(e => e.UnitId, "unit_id");

            entity.Property(e => e.EntryId)
                .HasColumnType("bigint(20)")
                .HasColumnName("entry_id");
            entity.Property(e => e.BillAmount)
                .HasMaxLength(50)
                .HasColumnName("bill_amount");
            entity.Property(e => e.BillDate)
                .HasColumnType("datetime")
                .HasColumnName("bill_date");
            entity.Property(e => e.BillNo)
                .HasMaxLength(50)
                .HasColumnName("bill_no");
            entity.Property(e => e.CityId)
                .HasColumnType("int(11)")
                .HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(250)
                .HasColumnName("city_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FMills)
                .HasPrecision(10, 2)
                .HasColumnName("f_mills");
            entity.Property(e => e.Freight)
                .HasPrecision(10, 2)
                .HasColumnName("freight");
            entity.Property(e => e.Hopper)
                .HasPrecision(10, 2)
                .HasColumnName("hopper");
            entity.Property(e => e.IsEmailSent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_email_sent");
            entity.Property(e => e.LooseEs)
                .HasPrecision(10, 2)
                .HasColumnName("loose_es");
            entity.Property(e => e.LrAmount)
                .HasPrecision(10, 2)
                .HasColumnName("lr_amount");
            entity.Property(e => e.LrDate)
                .HasColumnType("datetime")
                .HasColumnName("lr_date");
            entity.Property(e => e.LrNo)
                .HasMaxLength(50)
                .HasColumnName("lr_no");
            entity.Property(e => e.LrQty)
                .HasPrecision(10, 2)
                .HasColumnName("lr_qty");
            entity.Property(e => e.LrWeight)
                .HasPrecision(10, 2)
                .HasColumnName("lr_weight");
            entity.Property(e => e.OtherExpenses)
                .HasPrecision(10, 2)
                .HasColumnName("other_expenses");
            entity.Property(e => e.Others)
                .HasPrecision(10, 2)
                .HasColumnName("others");
            entity.Property(e => e.PartyId)
                .HasColumnType("bigint(20)")
                .HasColumnName("party_id");
            entity.Property(e => e.RatePerQtl)
                .HasPrecision(10, 2)
                .HasColumnName("rate_per_qtl");
            entity.Property(e => e.ReceivedBy)
                .HasMaxLength(150)
                .HasColumnName("received_by");
            entity.Property(e => e.ReceivedNotes)
                .HasMaxLength(500)
                .HasColumnName("received_notes");
            entity.Property(e => e.ReceivedOn)
                .HasColumnType("datetime")
                .HasColumnName("received_on");
            entity.Property(e => e.Spares)
                .HasPrecision(10, 2)
                .HasColumnName("spares");
            entity.Property(e => e.TotalFreight)
                .HasPrecision(10, 2)
                .HasColumnName("total_freight");
            entity.Property(e => e.TotalQty)
                .HasPrecision(10, 2)
                .HasColumnName("total_qty");
            entity.Property(e => e.TransporterId)
                .HasColumnType("int(11)")
                .HasColumnName("transporter_id");
            entity.Property(e => e.TruckNo)
                .HasMaxLength(50)
                .HasColumnName("truck_no");
            entity.Property(e => e.UnitId)
                .HasColumnType("tinyint(4)")
                .HasColumnName("unit_id");
            entity.Property(e => e.Wces)
                .HasPrecision(10, 2)
                .HasColumnName("wces");
        });

        modelBuilder.Entity<LrParty>(entity =>
        {
            entity.HasKey(e => e.PartyId).HasName("PRIMARY");

            entity.ToTable("lr_parties");

            entity.Property(e => e.PartyId)
                .HasColumnType("bigint(20)")
                .HasColumnName("party_id");
            entity.Property(e => e.Address1)
                .HasMaxLength(350)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(350)
                .HasColumnName("address2");
            entity.Property(e => e.CityId)
                .HasColumnType("int(11)")
                .HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(50)
                .HasColumnName("city_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.LoginName)
                .HasMaxLength(50)
                .HasColumnName("login_name");
            entity.Property(e => e.LoginPassword)
                .HasMaxLength(50)
                .HasColumnName("login_password");
            entity.Property(e => e.MobilePhone)
                .HasMaxLength(50)
                .HasColumnName("mobile_phone");
            entity.Property(e => e.PartyName)
                .HasMaxLength(150)
                .HasColumnName("party_name");
            entity.Property(e => e.PinCode)
                .HasMaxLength(50)
                .HasColumnName("pin_code");
            entity.Property(e => e.Telephone)
                .HasMaxLength(50)
                .HasColumnName("telephone");
        });

        modelBuilder.Entity<LrTransporter>(entity =>
        {
            entity.HasKey(e => e.TransporterId).HasName("PRIMARY");

            entity.ToTable("lr_transporters");

            entity.Property(e => e.TransporterId)
                .HasColumnType("int(11)")
                .HasColumnName("transporter_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.TransporterName)
                .HasMaxLength(150)
                .HasColumnName("transporter_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
