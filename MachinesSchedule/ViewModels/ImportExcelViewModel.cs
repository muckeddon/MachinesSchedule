using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MachinesSchedule.ViewModels
{
    public class ImportExcelViewModel
    {
        #region Properties
        [Required]
        [Display(Name = "Выбор документа")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Загрузить файл")]
        public IFormFile Document { get; set; }
        #endregion
    }
}
