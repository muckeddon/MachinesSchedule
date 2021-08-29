namespace MachinesSchedule.Models.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public string NameOfMachine { get; set; }
        public string StatusOfMachine { get; set; }
        public string NameOfTime { get; set; }
        public string Time { get; set; }
    }
}
