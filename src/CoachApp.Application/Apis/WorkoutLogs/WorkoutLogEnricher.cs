using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.WorkoutLogs;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.WorkoutLogs;

/// <summary>Fills <see cref="WorkoutLogEntryDto.ExerciseName"/> from the exercise library.</summary>
internal static class WorkoutLogEnricher
{
    public static async Task EnrichAsync(WorkoutLogDto dto, IRepository<Exercise, Guid> exerciseRepository)
    {
        var ids = dto.Entries.Select(e => e.ExerciseId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var names = (await exerciseRepository.GetListAsync(x => ids.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => x.Name);

        foreach (var entry in dto.Entries)
        {
            if (names.TryGetValue(entry.ExerciseId, out var name))
            {
                entry.ExerciseName = name;
            }
        }
    }
}
