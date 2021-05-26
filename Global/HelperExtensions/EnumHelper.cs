namespace ODataWebserver.Global
{
    public static class EnumHelper
    {
        public static bool IsOneFlagSet<T>(T currentValue, T flagsOneHasToBeSet) where T : struct => ((int)(object)currentValue & (int)(object)flagsOneHasToBeSet) > 0;

        public static bool IsAllFlagsSet<T>(T currValue, T flagsAllHaveToBeSet) where T : struct => ((int)(object)currValue & (int)(object)flagsAllHaveToBeSet) == (int)(object)flagsAllHaveToBeSet;
    }
}
