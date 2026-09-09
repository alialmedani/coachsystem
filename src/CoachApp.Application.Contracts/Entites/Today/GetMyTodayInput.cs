using System;

namespace CoachApp.Entites.Today;

/// <summary>
/// Input for the trainee "Today" view. <see cref="Date"/> is the trainee's local date (the client
/// supplies it so "today" respects their timezone); when null the server uses today's UTC date.
/// </summary>
public class GetMyTodayInput
{
    public DateTime? Date { get; set; }
}
