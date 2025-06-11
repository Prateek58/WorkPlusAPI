using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using WorkPlusAPI.WorkPlus.Model.LR;

namespace WorkPlusAPI.WorkPlus.Data;

public partial class LRDbContext : DbContext
{
    public LRDbContext()
    {
    }

    public LRDbContext(DbContextOptions<LRDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LrDocument> LrDocuments { get; set; }

    public virtual DbSet<LrDocumentType> LrDocumentTypes { get; set; }

    public virtual DbSet<LrEntry> LrEntries { get; set; }

    public virtual DbSet<LrParty> LrParties { get; set; }

    public virtual DbSet<LrTransporter> LrTransporters { get; set; }

    public virtual DbSet<Statecity> Statecities { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

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

        modelBuilder.Entity<LrDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PRIMARY");

            entity.ToTable("lr_documents");

            entity.HasIndex(e => e.LrEntryId, "idx_lr_documents_entry");

            entity.HasIndex(e => e.TypeId, "type_id");

            entity.Property(e => e.DocumentId)
                .HasColumnType("int(11)")
                .HasColumnName("document_id");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(255)
                .HasColumnName("document_name");
            entity.Property(e => e.LrEntryId)
                .HasColumnType("int(11)")
                .HasColumnName("lr_entry_id");
            entity.Property(e => e.TypeId)
                .HasColumnType("int(11)")
                .HasColumnName("type_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.LrEntry).WithMany(p => p.LrDocuments)
                .HasForeignKey(d => d.LrEntryId)
                .HasConstraintName("lr_documents_ibfk_1");

            entity.HasOne(d => d.Type).WithMany(p => p.LrDocuments)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lr_documents_ibfk_2");
        });

        modelBuilder.Entity<LrDocumentType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PRIMARY");

            entity.ToTable("lr_document_types");

            entity.Property(e => e.TypeId)
                .HasColumnType("int(11)")
                .HasColumnName("type_id");
            entity.Property(e => e.AllowedExtensions)
                .HasMaxLength(100)
                .HasColumnName("allowed_extensions");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<LrEntry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PRIMARY");

            entity.ToTable("lr_entries");

            entity.HasIndex(e => e.BillNo, "idx_bill_no");

            entity.HasIndex(e => e.DestinationCityId, "idx_destination_city");

            entity.HasIndex(e => e.LrDate, "idx_lr_date");

            entity.HasIndex(e => e.LrNo, "idx_lr_no");

            entity.HasIndex(e => e.OriginCityId, "idx_origin_city");

            entity.HasIndex(e => e.Status, "idx_status");

            entity.HasIndex(e => e.TruckNo, "idx_truck_no");

            entity.HasIndex(e => e.PartyId, "party_id");

            entity.HasIndex(e => e.TransporterId, "transporter_id");

            entity.HasIndex(e => e.UnitId, "unit_id");

            entity.Property(e => e.EntryId)
                .HasColumnType("int(11)")
                .HasColumnName("entry_id");
            entity.Property(e => e.BillAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("bill_amount");
            entity.Property(e => e.BillDate).HasColumnName("bill_date");
            entity.Property(e => e.BillNo)
                .HasMaxLength(50)
                .HasColumnName("bill_no");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("int(11)")
                .HasColumnName("created_by");
            entity.Property(e => e.DestinationCityId)
                .HasColumnType("int(11)")
                .HasColumnName("destination_city_id");
            entity.Property(e => e.DriverMobile)
                .HasMaxLength(15)
                .HasColumnName("driver_mobile");
            entity.Property(e => e.DriverName)
                .HasMaxLength(100)
                .HasColumnName("driver_name");
            entity.Property(e => e.Freight)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("freight");
            entity.Property(e => e.LrAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("lr_amount");
            entity.Property(e => e.LrDate).HasColumnName("lr_date");
            entity.Property(e => e.LrNo)
                .HasMaxLength(50)
                .HasColumnName("lr_no");
            entity.Property(e => e.LrQty)
                .HasPrecision(10, 3)
                .HasDefaultValueSql("'0.000'")
                .HasColumnName("lr_qty");
            entity.Property(e => e.LrWeight)
                .HasPrecision(10, 3)
                .HasDefaultValueSql("'0.000'")
                .HasColumnName("lr_weight");
            entity.Property(e => e.OriginCityId)
                .HasColumnType("int(11)")
                .HasColumnName("origin_city_id");
            entity.Property(e => e.OtherExpenses)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("other_expenses");
            entity.Property(e => e.PartyId)
                .HasColumnType("int(11)")
                .HasColumnName("party_id");
            entity.Property(e => e.RatePerQtl)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("rate_per_qtl");
            entity.Property(e => e.Remarks)
                .HasColumnType("text")
                .HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'DRAFT'")
                .HasColumnType("enum('DRAFT','CONFIRMED','IN_TRANSIT','DELIVERED','CANCELLED')")
                .HasColumnName("status");
            entity.Property(e => e.TotalFreight)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("total_freight");
            entity.Property(e => e.TotalQty)
                .HasPrecision(10, 3)
                .HasDefaultValueSql("'0.000'")
                .HasColumnName("total_qty");
            entity.Property(e => e.TransporterId)
                .HasColumnType("int(11)")
                .HasColumnName("transporter_id");
            entity.Property(e => e.TruckNo)
                .HasMaxLength(20)
                .HasColumnName("truck_no");
            entity.Property(e => e.UnitId)
                .HasColumnType("int(11)")
                .HasColumnName("unit_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy)
                .HasColumnType("int(11)")
                .HasColumnName("updated_by");

            entity.HasOne(d => d.DestinationCity).WithMany(p => p.LrEntryDestinationCities)
                .HasForeignKey(d => d.DestinationCityId)
                .HasConstraintName("lr_entries_ibfk_5");

            entity.HasOne(d => d.OriginCity).WithMany(p => p.LrEntryOriginCities)
                .HasForeignKey(d => d.OriginCityId)
                .HasConstraintName("lr_entries_ibfk_4");

            entity.HasOne(d => d.Party).WithMany(p => p.LrEntries)
                .HasForeignKey(d => d.PartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lr_entries_ibfk_2");

            entity.HasOne(d => d.Transporter).WithMany(p => p.LrEntries)
                .HasForeignKey(d => d.TransporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lr_entries_ibfk_3");

            entity.HasOne(d => d.Unit).WithMany(p => p.LrEntries)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lr_entries_ibfk_1");
        });

        modelBuilder.Entity<LrParty>(entity =>
        {
            entity.HasKey(e => e.PartyId).HasName("PRIMARY");

            entity.ToTable("lr_parties");

            entity.HasIndex(e => e.CityId, "idx_city_id");

            entity.HasIndex(e => e.PartyCode, "idx_party_code").IsUnique();

            entity.HasIndex(e => e.PartyName, "idx_party_name");

            entity.Property(e => e.PartyId)
                .HasColumnType("int(11)")
                .HasColumnName("party_id");
            entity.Property(e => e.Address1)
                .HasMaxLength(200)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(200)
                .HasColumnName("address2");
            entity.Property(e => e.CityId)
                .HasColumnType("int(11)")
                .HasColumnName("city_id");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gstin)
                .HasMaxLength(15)
                .HasColumnName("gstin");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Mobile)
                .HasMaxLength(15)
                .HasColumnName("mobile");
            entity.Property(e => e.Pan)
                .HasMaxLength(10)
                .HasColumnName("pan");
            entity.Property(e => e.PartyCode)
                .HasMaxLength(20)
                .HasColumnName("party_code");
            entity.Property(e => e.PartyName)
                .HasMaxLength(200)
                .HasColumnName("party_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .HasColumnName("pincode");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.City).WithMany(p => p.LrParties)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("lr_parties_ibfk_1");
        });

        modelBuilder.Entity<LrTransporter>(entity =>
        {
            entity.HasKey(e => e.TransporterId).HasName("PRIMARY");

            entity.ToTable("lr_transporters");

            entity.HasIndex(e => e.CityId, "idx_city_id");

            entity.HasIndex(e => e.TransporterCode, "idx_transporter_code").IsUnique();

            entity.HasIndex(e => e.TransporterName, "idx_transporter_name");

            entity.Property(e => e.TransporterId)
                .HasColumnType("int(11)")
                .HasColumnName("transporter_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.CityId)
                .HasColumnType("int(11)")
                .HasColumnName("city_id");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gstin)
                .HasMaxLength(15)
                .HasColumnName("gstin");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Mobile)
                .HasMaxLength(15)
                .HasColumnName("mobile");
            entity.Property(e => e.Pan)
                .HasMaxLength(10)
                .HasColumnName("pan");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .HasColumnName("pincode");
            entity.Property(e => e.TransporterCode)
                .HasMaxLength(20)
                .HasColumnName("transporter_code");
            entity.Property(e => e.TransporterName)
                .HasMaxLength(200)
                .HasColumnName("transporter_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.City).WithMany(p => p.LrTransporters)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("lr_transporters_ibfk_1");
        });

        modelBuilder.Entity<Statecity>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PRIMARY");

            entity
                .ToTable("statecity")
                .HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            entity.HasIndex(e => e.CityName, "idx_city_name");

            entity.HasIndex(e => e.State, "idx_state");

            entity.Property(e => e.CityId)
                .ValueGeneratedNever()
                .HasColumnType("int(5)")
                .HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(50)
                .HasColumnName("city_name");
            entity.Property(e => e.Latitude)
                .HasMaxLength(10)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasMaxLength(10)
                .HasColumnName("longitude");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .HasColumnName("state");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PRIMARY");

            entity.ToTable("units");

            entity.HasIndex(e => e.CityId, "idx_city_id");

            entity.HasIndex(e => e.UnitCode, "idx_unit_code").IsUnique();

            entity.HasIndex(e => e.UnitName, "idx_unit_name");

            entity.Property(e => e.UnitId)
                .HasColumnType("int(11)")
                .HasColumnName("unit_id");
            entity.Property(e => e.Address)
                .HasColumnType("text")
                .HasColumnName("address");
            entity.Property(e => e.CityId)
                .HasColumnType("int(11)")
                .HasColumnName("city_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gstin)
                .HasMaxLength(15)
                .HasColumnName("gstin");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .HasColumnName("pincode");
            entity.Property(e => e.UnitCode)
                .HasMaxLength(20)
                .HasColumnName("unit_code");
            entity.Property(e => e.UnitName)
                .HasMaxLength(100)
                .HasColumnName("unit_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.City).WithMany(p => p.Units)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("units_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
