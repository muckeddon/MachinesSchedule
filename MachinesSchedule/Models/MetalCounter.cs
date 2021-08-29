using MachinesSchedule.Models.DataAccessLayer;
using MachinesSchedule.Models.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MachinesSchedule.Models
{
    public class MetalCounter
    {
        private ApplicationContext _context;
        public MetalCounter(ApplicationContext context)
        {
            _context = context;
        }
        public string Fastest(List<MachineTool> machines, string metal) //Возвращает название оборудования, которая перерабатыват metal быстрее всех
        {

            var minInAllMachines = (from m in machines
                                    from met in m.Metals
                                    where met.Key == metal
                                    select met.Value).Min();

            var nameOfMachine = (from m in machines
                                 from met in m.Metals
                                 where met.Key == metal && met.Value == minInAllMachines
                                 select m.MachineName).FirstOrDefault();

            return nameOfMachine;
        }
        public List<(string, string)> FastestPairs(List<MachineTool> machines) //Возвращает пары пересечений вида (НазваниеОборудования/Металл) оптимальные для начала работы всего оборудования
        {
            List<(string, string)> FastestJobPerMachine = new List<(string, string)>();
            foreach (var m in machines)
            {
                FastestJobPerMachine.Add((m.MachineName, m.FastestJob()));
            }

            List<(string, string)> FastestJobPerAllMachines = new List<(string, string)>();
            foreach (var m in _context.Nomenclature.ToList())
            {
                FastestJobPerAllMachines.Add((Fastest(machines, m.NomenclatureName.ToString()), m.NomenclatureName.ToString()));
            }

            List<(string, string)> result = FastestJobPerMachine.Intersect(FastestJobPerAllMachines).ToList();

            if (result.Count() == 2)
            {
                string metal = "";
                foreach (var m in _context.Nomenclature.ToList())   //получаем оставшийся металл
                {
                    foreach (var r in result)
                    {
                        if (m.NomenclatureName.ToString() != r.Item2)
                            metal = m.NomenclatureName.ToString();
                    }
                }

                string name = "";
                foreach (var m in machines)     //получаем оставшуюся машину
                {
                    foreach (var r in result)
                    {
                        if (m.MachineName != r.Item1)
                            name = m.MachineName;
                    }
                }
                result.Add((name, metal));

                return result;
            }

            if (result.Count() == 1)
            {
                List<MachineTool> machineTools = new List<MachineTool>();
                List<string> Metals = new List<string>();

                foreach (var r in result) // получаем оставшиеся машины
                {
                    machineTools.Add(machines.FirstOrDefault(m => m.MachineName != r.Item1));
                }

                foreach (var r in result)
                {
                    foreach (var m in _context.Nomenclature.ToList())   //получаем оставшиеся металлы
                    {
                        if (m.NomenclatureName.ToString() != r.Item2)
                            Metals.Add(m.NomenclatureName.ToString());
                    }
                }
            }

            return result; //if(result.Count() == 3)
        }
        public Dictionary<string, int> GetAllMetals(List<Shipment> shipments, List<Nomenclature> nomenclatures) //возвращает словарь с количеством каждого металла
        {
            Dictionary<string, int> metals = new Dictionary<string, int>();

            foreach (var m in _context.Nomenclature.ToList())
            {
                int metalCount = (from s in shipments
                                  where s.NomenclatureId == (nomenclatures.FirstOrDefault(n => n.NomenclatureName == m.NomenclatureName.ToString()).NomenclatureId)
                                  select s).Count();
                metals.Add(m.NomenclatureName.ToString(), metalCount);
            }

            return metals;
        }
    }
}
