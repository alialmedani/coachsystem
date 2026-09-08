using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.TrainingPlans;

public class CreateUpdateTrainingPlanDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    [StringLength(TrainingPlanConsts.MaxTitleLength)]
    public string Title { get; set; } = string.Empty;

    [StringLength(TrainingPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
