using MachinesSchedule.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MachinesSchedule.Models.DataAccessLayer
{
    public class ApplicationContext : DbContext
    {
        #region Properties
        public DbSet<MachineTool> MachineTools { get; set; }
        public DbSet<Nomenclature> Nomenclature { get; set; }
        public DbSet<Shipment> Shipment { get; set; }
        public DbSet<Time> Time { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        #endregion
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
