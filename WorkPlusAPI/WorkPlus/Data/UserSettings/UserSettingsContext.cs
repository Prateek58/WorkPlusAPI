using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WorkPlusAPI.WorkPlus.Model.UserSettings;

namespace WorkPlusAPI.WorkPlus.Data.UserSettings;

public partial class UserSettingsContext : DbContext
{
    public UserSettingsContext(DbContextOptions<UserSettingsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_settings");

            entity.HasIndex(e => e.SettingKey, "idx_user_settings_key");

            entity.HasIndex(e => e.UserId, "idx_user_settings_user_id");

            entity.HasIndex(e => new { e.UserId, e.SettingKey }, "unique_user_setting").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingType)
                .HasDefaultValueSql("'string'")
                .HasColumnType("enum('string','boolean','json','color')")
                .HasColumnName("setting_type");
            entity.Property(e => e.SettingValue)
                .HasColumnType("text")
                .HasColumnName("setting_value");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
