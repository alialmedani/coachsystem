using System;
using CoachApp.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.Exercises;

/// <summary>
/// An exercise in the coach's library (tenant-scoped, so each coach owns their own
/// set). Referenced by workout plans. Pragmatic domain: public setters + a
/// validating <see cref="Create"/>.
/// </summary>
public class Exercise : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    /// <summary>How to perform the exercise.</summary>
    public virtual string? Instructions { get; set; }

    public virtual MuscleGroup TargetMuscle { get; set; }

    public virtual Equipment Equipment { get; set; }

    public virtual string? VideoUrl { get; set; }

    public virtual string? ImageUrl { get; set; }

    public virtual bool IsActive { get; set; }

    /// <summary>For the ORM only.</summary>
    protected Exercise()
    {
    }

    public static Exercise Create(
        Guid id,
        string name,
        MuscleGroup targetMuscle = MuscleGroup.Other,
        Equipment equipment = Equipment.None,
        string? description = null,
        string? instructions = null,
        string? videoUrl = null,
        string? imageUrl = null,
        Guid? tenantId = null)
    {
        return new Exercise
        {
            Id = id,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), ExerciseConsts.MaxNameLength),
            TargetMuscle = targetMuscle,
            Equipment = equipment,
            Description = Check.Length(description, nameof(description), ExerciseConsts.MaxDescriptionLength),
            Instructions = Check.Length(instructions, nameof(instructions), ExerciseConsts.MaxInstructionsLength),
            VideoUrl = Check.Length(videoUrl, nameof(videoUrl), ExerciseConsts.MaxMediaUrlLength),
            ImageUrl = Check.Length(imageUrl, nameof(imageUrl), ExerciseConsts.MaxMediaUrlLength),
            TenantId = tenantId,
            IsActive = true
        };
    }

    public virtual void Activate() => IsActive = true;

    public virtual void Deactivate() => IsActive = false;
}
