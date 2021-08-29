using ExcelDataReader;
using MachinesSchedule.Models;
using MachinesSchedule.Models.DataAccessLayer;
using MachinesSchedule.Models.Entities;
using MachinesSchedule.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
                                Shipment shipment = new Shipment();
                                shipment.ShipmentId = Convert.ToInt32(reader.GetValue(0));
                                shipment.NomenclatureId = Convert.ToInt32(reader.GetValue(1));
                                _context.Shipment.Add(shipment);
                            }
                            if (model.Name == "Номенклатуры")
                            {
                                Nomenclature nomenclature = new Nomenclature();
                                nomenclature.NomenclatureId = Convert.ToInt32(reader.GetValue(0));
                                nomenclature.NomenclatureName = reader.GetValue(1).ToString();
                                _context.Nomenclature.Add(nomenclature);
                            }
                            if (model.Name == "Оборудование")
                            {
                                MachineTool machineTools = new MachineTool();
                                machineTools.MachineToolsId = Convert.ToInt32(reader.GetValue(0));
                                machineTools.MachineName = reader.GetValue(1).ToString();
                                _context.MachineTools.Add(machineTools);
                            }
                            if (model.Name == "Время")
                            {
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

            List<(string, int)> listlist = new List<(string, int)>();
            List<(string, int)> listlist2 = new List<(string, int)>();
            List<(string, int)> listlist3 = new List<(string, int)>();
            Parallel.Invoke(() => listlist = machineTools[0].MachineToolStart(fastestWay, availableMetals),
                () => listlist2 = machineTools[1].MachineToolStart(fastestWay, availableMetals),
                () => listlist3 = machineTools[2].MachineToolStart(fastestWay, availableMetals));

            return View();
        }

    }
}
