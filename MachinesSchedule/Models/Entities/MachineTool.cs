using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MachinesSchedule.Models.Entities
{
    public class MachineTool
    {
        public int Id { get; set; }
        public int MachineToolsId { get; set; }
        public string MachineName { get; set; }
        [NotMapped]
        public bool IsWork { get; set; } = false;
        [NotMapped]
        public bool FullStop { get; set; } = false;

        [NotMapped]
        public Dictionary<string, int> Metals = new Dictionary<string, int>();
        public string FastestJob() //возвращает название металла, который машина перерабатывает быстрее всего
        {
            return (from m in Metals
                    where m.Value == (Metals.Min(met => met.Value))
                    select m.Key).FirstOrDefault();
        }
    }
}
