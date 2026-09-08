using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Dashboards;

/// <summary>Coach input: nutrition adherence for a specific trainee on a specific day.</summary>
public class GetNutritionAdherenceInput
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    public DateTime Date { get; set; }
}
