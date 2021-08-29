﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}