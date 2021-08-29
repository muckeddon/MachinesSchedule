using ExcelDataReader;
using MachinesSchedule.Models;
using MachinesSchedule.Models.DataAccessLayer;
using MachinesSchedule.Models.Entities;
using MachinesSchedule.ViewModels;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MachinesSchedule.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext _context;

        public HomeController(ApplicationContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ImportFile()
        {
            return RedirectToAction("ImportExcel", "Home");
        }
        [HttpGet]
        public IActionResult ImportExcel()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ImportExcel(ImportExcelViewModel model)
        {
            using (var stream = new MemoryStream())
            {
                model.Document.CopyTo(stream);
                stream.Position = 0;
                int lineCounter = 0;
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read()) //каждая строка файла
                    {
                        if (lineCounter >= 1)
                        {
                            if (model.Name == "Партии")
                            {
                                if (_context.Shipment.ToList().Count != 0)//проверка на наличие Расписания в бд
                                {
                                    var itemsForDelete = _context.Set<Shipment>();
                                    _context.Shipment.RemoveRange(itemsForDelete);
                                    _context.SaveChanges();
                                }

                                Shipment shipment = new Shipment();
                                shipment.ShipmentId = Convert.ToInt32(reader.GetValue(0));
                                shipment.NomenclatureId = Convert.ToInt32(reader.GetValue(1));
                                _context.Shipment.Add(shipment);
                            }
                            if (model.Name == "Номенклатуры")
                            {
                                if (_context.Nomenclature.ToList().Count != 0)//проверка на наличие Расписания в бд
                                {
                                    var itemsForDelete = _context.Set<Nomenclature>();
                                    _context.Nomenclature.RemoveRange(itemsForDelete);
                                    _context.SaveChanges();
                                }
                                Nomenclature nomenclature = new Nomenclature();
                                nomenclature.NomenclatureId = Convert.ToInt32(reader.GetValue(0));
                                nomenclature.NomenclatureName = reader.GetValue(1).ToString();
                                _context.Nomenclature.Add(nomenclature);
                            }
                            if (model.Name == "Оборудование")
                            {
                                if (_context.MachineTools.ToList().Count != 0)//проверка на наличие Расписания в бд
                                {
                                    var itemsForDelete = _context.Set<MachineTool>();
                                    _context.MachineTools.RemoveRange(itemsForDelete);
                                    _context.SaveChanges();
                                }
                                MachineTool machineTools = new MachineTool();
                                machineTools.MachineToolsId = Convert.ToInt32(reader.GetValue(0));
                                machineTools.MachineName = reader.GetValue(1).ToString();
                                _context.MachineTools.Add(machineTools);
                            }
                            if (model.Name == "Время")
                            {
                                if (_context.Time.ToList().Count != 0)//проверка на наличие Расписания в бд
                                {
                                    var itemsForDelete = _context.Set<Time>();
                                    _context.Time.RemoveRange(itemsForDelete);
                                    _context.SaveChanges();
                                }
                                Time time = new Time();
                                time.MachineToolId = Convert.ToInt32(reader.GetValue(0));
                                time.NomenclatureId = Convert.ToInt32(reader.GetValue(1));
                                time.OperationTime = Convert.ToInt32(reader.GetValue(2));
                                _context.Time.Add(time);
                            }
                            _context.SaveChanges();
                        }
                        lineCounter++;
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult CreateSchedule()
        {
            return RedirectToAction("ScheduleCreator", "Home");
        }
        public IActionResult ScheduleCreator()
        {
            List<MachineTool> machineTools = _context.MachineTools.ToList();
            List<Time> times = _context.Time.ToList();
            List<Nomenclature> nomenclatures = _context.Nomenclature.ToList();
            List<Shipment> shipments = _context.Shipment.ToList();

            foreach (var mt in machineTools)
            {
                Dictionary<string, int> Metals = new Dictionary<string, int>();
                foreach (var t in times)
                {
                    foreach (var n in nomenclatures)
                    {
                        if (n.NomenclatureId == t.NomenclatureId && t.MachineToolId == mt.MachineToolsId)
                            Metals.Add(n.NomenclatureName, t.OperationTime);
                    }
                }
                mt.Metals = Metals;
            }

            MetalCounter metalCounter = new MetalCounter(_context);

            List<(string, string)> fastestWay = metalCounter.FastestPairs(machineTools);
            Dictionary<string, int> availableMetals = metalCounter.GetAllMetals(shipments, nomenclatures);

            List<(string, int)> firstMachineSchedule = new List<(string, int)>();
            List<(string, int)> secondMachineSchedule = new List<(string, int)>();
            List<(string, int)> thirdMachineSchedule = new List<(string, int)>();

            Parallel.Invoke(() => firstMachineSchedule = machineTools[0].MachineToolStart(fastestWay, availableMetals), //"включение" машин в параллельную обработку для расчета оптимального расписания
                () => secondMachineSchedule = machineTools[1].MachineToolStart(fastestWay, availableMetals),
                () => thirdMachineSchedule = machineTools[2].MachineToolStart(fastestWay, availableMetals));

            List<Dictionary<string, object>> readySchedule = ReadySchedule(firstMachineSchedule, secondMachineSchedule, thirdMachineSchedule);
            
            return View(readySchedule);
        }
        private List<Dictionary<string, object>> ReadySchedule(List<(string, int)> firstMachineSchedule, List<(string, int)> secondMachineSchedule, List<(string, int)> thirdMachineSchedule)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            if (_context.Schedule.ToList().Count != 0)//проверка на наличие Расписания в бд
            {
                var itemsForDelete = _context.Set<Schedule>();
                _context.Schedule.RemoveRange(itemsForDelete);
                _context.SaveChanges();
            }    

            Schedule schedule = new Schedule();
            int maxNumb = firstMachineSchedule.Count;
            if (secondMachineSchedule.Count > maxNumb)
                maxNumb = secondMachineSchedule.Count;
            if (thirdMachineSchedule.Count > maxNumb)
                maxNumb = thirdMachineSchedule.Count;

            for (int i = 0; i < maxNumb; i++)
            {
                Dictionary<string, object> forCycle = new Dictionary<string, object>();
                if (firstMachineSchedule.Count <= i)
                {
                    forCycle.Add("Печь 1", "Остановлена");
                    forCycle.Add("Время для Печи 1", "Печь остановлена");
                }
                else
                {
                    forCycle.Add("Печь 1", firstMachineSchedule[i].Item1);
                    forCycle.Add("Время для Печи 1", firstMachineSchedule[i].Item2);
                }

                if (secondMachineSchedule.Count <= i)
                {
                    forCycle.Add("Печь 2", "Остановлена");
                    forCycle.Add("Время для Печи 2", "Печь остановлена");
                }
                else
                {
                    forCycle.Add("Печь 2", secondMachineSchedule[i].Item1);
                    forCycle.Add("Время для Печи 2", secondMachineSchedule[i].Item2);
                }

                if (thirdMachineSchedule.Count <= i)
                {
                    forCycle.Add("Печь 3", "Остановлена");
                    forCycle.Add("Время для Печи 3", "Печь остановлена");
                }
                else
                {
                    forCycle.Add("Печь 3", thirdMachineSchedule[i].Item1);
                    forCycle.Add("Время для Печи 3", thirdMachineSchedule[i].Item2);
                }

                int counter = 0;
                foreach(var f in forCycle)//добавление расписания в БД
                {
                    if (counter == 1)
                    {
                        schedule.NameOfTime = f.Key;
                        schedule.Time = f.Value.ToString();
                        counter++;
                    }
                    if (counter == 0)
                    {
                        schedule.NameOfMachine = f.Key;
                        schedule.StatusOfMachine = f.Value.ToString();
                        counter++;
                    }
                    if (counter == 2)
                    {
                        _context.Schedule.Add(schedule);
                        _context.SaveChanges();
                        schedule = new Schedule();
                        counter = 0;
                    }
                }
                list.Add(forCycle);
            }
            return list;
        }
        public IActionResult ExportToExcel()
        {
            List<Schedule> schedule = _context.Schedule.ToList();
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
            List<Dictionary<string, object>> readySchedule = new List<Dictionary<string, object>>();
            int counter = 0;
            foreach(var s in schedule) //получаем все записи из бд о расписании и приводим их в вид необходимый ExcelPackage
            {
                if(counter <= 2)
                {
                    keyValuePairs.Add(s.NameOfMachine, s.StatusOfMachine);
                    keyValuePairs.Add(s.NameOfTime, s.Time);
                }
                else
                {
                    counter = 0;
                    readySchedule.Add(keyValuePairs);
                    keyValuePairs = new Dictionary<string, object>();
                    keyValuePairs.Add(s.NameOfMachine, s.StatusOfMachine);
                    keyValuePairs.Add(s.NameOfTime, s.Time);
                }
                counter++;
            }
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Schedule");
                worksheet.Cells.LoadFromDictionaries(readySchedule, true);
                package.Save();
            }
            stream.Position = 0;
            string excelname = "Schedule.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelname);
        }
    }
}
