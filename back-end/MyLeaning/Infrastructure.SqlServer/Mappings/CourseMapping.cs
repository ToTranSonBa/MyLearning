using Domain.Courses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.SqlServer.Mappings;

/// <summary>
/// Entity Framework configuration for Course entity.
/// Defines table structure, constraints, and indexes.
/// </summary>
public class CourseMapping : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        // Primary Key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.Level)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(c => c.TotalLessons)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(c => c.EstimatedDurationHours)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(c => c.InstructorName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        // Indexes for better query performance
        builder.HasIndex(c => c.Title)
            .IsUnique()
            .HasDatabaseName("IDX_Courses_Title_Unique");

        builder.HasIndex(c => c.Level)
            .HasDatabaseName("IDX_Courses_Level");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IDX_Courses_IsActive");

        builder.HasIndex(c => new { c.IsActive, c.Level })
            .HasDatabaseName("IDX_Courses_Active_Level");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IDX_Courses_CreatedAt");
    }
}
