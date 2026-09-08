using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Dashboards;

/// <summary>Trainee input: my nutrition adherence on a specific day (trainee = current user).</summary>
public class GetMyNutritionAdherenceInput
{
    [Required]
    public DateTime Date { get; set; }
}
