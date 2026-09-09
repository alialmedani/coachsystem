using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// "Log From Plan": the trainee starts a nutrition log from one of their own nutrition plans.
/// The server snapshots that plan's meals (food + quantity) into the log. The trainee is the
/// current user (no TraineeId here); the plan must belong to them.
/// </summary>
public class CreateNutritionLogFromPlanDto
{
    [Required]
    public Guid NutritionPlanId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(NutritionLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
