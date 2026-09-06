using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.Training.EntityFrameworkCore;

public class TrainingModelBuilderConfigurationOptions : AbpModelBuilderConfigurationOptions
{
    public TrainingModelBuilderConfigurationOptions(
        string tablePrefix = "",
        string? schema = null)
        : base(tablePrefix, schema)
    {
    }
}
