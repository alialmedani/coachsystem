using System.Linq;
using CoachApp.Training.Trainees;

namespace CoachApp.Training.EntityFrameworkCore;

/// <summary>
/// Detail-include helper kept for pattern consistency. The <see cref="Trainee"/>
/// aggregate currently has no child collections, so this is a no-op; add
/// <c>.Include(...)</c> calls here when relations are introduced.
/// </summary>
public static class TrainingEfCoreQueryableExtensions
{
    public static IQueryable<Trainee> IncludeDetails(this IQueryable<Trainee> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable;
    }
}
