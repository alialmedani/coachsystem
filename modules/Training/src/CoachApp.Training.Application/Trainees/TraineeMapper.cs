using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace CoachApp.Training.Trainees;

/// <summary>
/// Mapperly mapper (the solution standard — see .abpstudio/ai-rules; AutoMapper
/// is not used). Only entity -> DTO is generated: creation and update flow
/// through the aggregate's behavior methods, never through a reverse map.
/// Resolved by <c>ObjectMapper.Map&lt;Trainee, TraineeDto&gt;()</c>.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TraineeToTraineeDtoMapper : MapperBase<Trainee, TraineeDto>
{
    public override partial TraineeDto Map(Trainee source);

    public override partial void Map(Trainee source, TraineeDto destination);
}
