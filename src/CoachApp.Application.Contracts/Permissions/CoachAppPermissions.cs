namespace CoachApp.Permissions;

/// <summary>
/// Permission tree for CoachApp. Two personas:
/// <list type="bullet">
/// <item><c>Coach.*</c> — management side (granted to the <c>Coach</c> role / tenant admin).</item>
/// <item><c>Trainee.*</c> — self-service side (granted to the <c>Trainee</c> role).</item>
/// </list>
/// </summary>
public static class CoachAppPermissions
{
    public const string GroupName = "CoachApp";

    public static class Coach
    {
        public static class Trainees
        {
            public const string Default = GroupName + ".Coach.Trainees";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        public static class Exercises
        {
            public const string Default = GroupName + ".Coach.Exercises";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        public static class WorkoutPlans
        {
            public const string Default = GroupName + ".Coach.WorkoutPlans";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        public static class Foods
        {
            public const string Default = GroupName + ".Coach.Foods";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        public static class NutritionPlans
        {
            public const string Default = GroupName + ".Coach.NutritionPlans";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        /// <summary>View a trainee's workout/nutrition logs and progress.</summary>
        public static class Tracking
        {
            public const string Default = GroupName + ".Coach.Tracking";
        }
    }

    public static class Trainee
    {
        public static class MyProfile
        {
            public const string Default = GroupName + ".Trainee.MyProfile";
        }

        public static class MyWorkoutPlans
        {
            public const string Default = GroupName + ".Trainee.MyWorkoutPlans";
        }

        public static class WorkoutLogs
        {
            public const string Default = GroupName + ".Trainee.WorkoutLogs";
            public const string Create = Default + ".Create";
        }

        public static class MyNutritionPlans
        {
            public const string Default = GroupName + ".Trainee.MyNutritionPlans";
        }

        public static class NutritionLogs
        {
            public const string Default = GroupName + ".Trainee.NutritionLogs";
            public const string Create = Default + ".Create";
        }
    }
}
