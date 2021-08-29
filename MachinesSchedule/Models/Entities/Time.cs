
namespace MachinesSchedule.Models.Entities
{
    public class Time
    {
        public int Id { get; set; }
        public int MachineToolId { get; set; }
        public int NomenclatureId { get; set; }
        public int OperationTime { get; set; }
    }
}
