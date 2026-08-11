using Framework.BuildingBlock.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Framework.BuildingBlock.Extensions;

public static class FrameworkEntityTypeBuilderExtensions
{
    public static void ConfigureByConvention(
        this EntityTypeBuilder builder)
    {
        builder.TryConfigureConcurrencyStamp();
        builder.TryConfigureSoftDelete();
        builder.TryConfigureAudit();
    }


    private static void TryConfigureConcurrencyStamp(
        this EntityTypeBuilder builder)
    {
        if (!typeof(IHasConcurrencyStamp)
            .IsAssignableFrom(builder.Metadata.ClrType))
        {
            return;
        }

        builder.Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
            .IsConcurrencyToken()
            .HasMaxLength(40)
            .HasColumnName(nameof(IHasConcurrencyStamp.ConcurrencyStamp));
    }


    private static void TryConfigureSoftDelete(
        this EntityTypeBuilder builder)
    {
        if (!typeof(IHasSoftDelete)
            .IsAssignableFrom(builder.Metadata.ClrType))
        {
            return;
        }

        builder.Property(nameof(IHasSoftDelete.IsDeleted))
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName(nameof(IHasSoftDelete.IsDeleted));
    }


    private static void TryConfigureAudit(
        this EntityTypeBuilder builder)
    {
        var clrType = builder.Metadata.ClrType;


        if (!typeof(IHasAuditProperties)
            .IsAssignableFrom(clrType))
        {
            return;
        }


        builder.TryConfigureCreationAudit();

        builder.TryConfigureModificationAudit();

        if (typeof(IHasSoftDelete)
            .IsAssignableFrom(clrType))
        {

            builder.TryConfigureDeletionAudit();
        }
    }


    private static void TryConfigureCreationAudit(
        this EntityTypeBuilder builder)
    {
        if (!typeof(IHasAuditProperties)
            .IsAssignableFrom(builder.Metadata.ClrType))
        {
            return;
        }


        builder.Property(nameof(IHasAuditProperties.CreationTime))
            .IsRequired()
            .HasColumnName(nameof(IHasAuditProperties.CreationTime));


        builder.Property(nameof(IHasAuditProperties.CreatorId))
            .IsRequired(false)
            .HasColumnName(nameof(IHasAuditProperties.CreatorId));
    }


    private static void TryConfigureModificationAudit(
        this EntityTypeBuilder builder)
    {
        if (!typeof(IHasAuditProperties)
            .IsAssignableFrom(builder.Metadata.ClrType))
        {
            return;
        }


        builder.Property(nameof(IHasAuditProperties.ModificationTime))
            .IsRequired(false)
            .HasColumnName(nameof(IHasAuditProperties.ModificationTime));


        builder.Property(nameof(IHasAuditProperties.ModifierId))
            .IsRequired(false)
            .HasColumnName(nameof(IHasAuditProperties.ModifierId));
    }


    private static void TryConfigureDeletionAudit(
        this EntityTypeBuilder builder)
    {
        if (!typeof(IHasAuditProperties)
            .IsAssignableFrom(builder.Metadata.ClrType))
        {
            return;
        }


        builder.Property(nameof(IHasSoftDelete.DeletionTime))
            .IsRequired(false)
            .HasColumnName(nameof(IHasSoftDelete.DeletionTime));


        builder.Property(nameof(IHasSoftDelete.DeleterId))
            .IsRequired(false)
            .HasColumnName(nameof(IHasSoftDelete.DeleterId));
    }
}
