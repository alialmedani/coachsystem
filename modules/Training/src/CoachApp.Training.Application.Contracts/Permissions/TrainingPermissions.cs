namespace CoachApp.Training.Permissions;

public static class TrainingPermissions
{
    public const string GroupName = "Training";

    public static class Trainees
    {
        public const string Default = GroupName + ".Trainees";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
