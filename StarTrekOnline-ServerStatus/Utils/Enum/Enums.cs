namespace StarTrekOnline_ServerStatus.Utils.API
{
    public class Enums
    {
        public enum ShardStatus
        {
            Maintenance,
            Up,
            None,
        }

        public enum MaintenanceTimeType
        {
            WaitingForMaintenance,
            Maintenance,
            MaintenanceEnded,
            SpecialMaintenance,
            None,
        }
    }
}