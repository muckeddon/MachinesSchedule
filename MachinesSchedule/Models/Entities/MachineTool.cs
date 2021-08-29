using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;

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
        public List<(string, int)> MachineToolStart(List<(string, string)> fastestPairs, Dictionary<string, int> availableMetals) //возвращает список обработанных машиной металлов по порядку
                                                                                                                                  //кортежем (Название металла/Время обработки)
        {
            IsWork = true;
            List<(string, int)> mnT = new List<(string, int)>();
            string FirstMetalForWork = fastestPairs.FirstOrDefault(r => r.Item1 == MachineName).Item2; //получаем металл с которым работает машина в приоритете
            int time = Metals.FirstOrDefault(m => m.Key == FirstMetalForWork).Value; //получаем время за которое машина перерабатывает металл
            string MetalForWork = FirstMetalForWork;

            while (FullStop != true)
            {
                if (availableMetals[MetalForWork] <= 0) //проверяем наличие металла с которым работает машина
                {
                    IsWork = false;
                }

                if (IsWork == false) //проверяем работу машины, если она остановилась, то ищем следующий металл для работы
                {
                    MetalForWork = Checker(FirstMetalForWork, fastestPairs, availableMetals);
                    if (MetalForWork != "None")
                    {
                        time = Metals.FirstOrDefault(m => m.Key == MetalForWork).Value;
                        IsWork = true;
                    }
                    else
                        FullStop = true;
                }
                if (MetalForWork != "None")
                {
                    availableMetals[MetalForWork] = availableMetals[MetalForWork] - 1;
                    mnT.Add((MetalForWork, time));
                }
                Thread.Sleep(time * 10);
            }
            return mnT;
        }
    }
}
