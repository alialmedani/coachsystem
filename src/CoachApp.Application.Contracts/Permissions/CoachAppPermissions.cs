namespace CoachApp.Permissions;

public static class CoachAppPermissions
{
    public const string GroupName = "CoachApp";

    public static class Trainees
    {
        public const string Default = GroupName + ".Trainees";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class TrainingPlans
    {
        public const string Default = GroupName + ".TrainingPlans";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
}
