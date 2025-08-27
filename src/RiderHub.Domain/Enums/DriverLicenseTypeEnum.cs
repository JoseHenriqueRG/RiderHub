using System.ComponentModel.DataAnnotations;

namespace RiderHub.Domain.Enums
{
    public enum DriverLicenseTypeEnum
    {
        [Display(Name = "Categoria A")]
        A,

        [Display(Name = "Categoria B")]
        B,

        [Display(Name = "Categoria A+B")]
        AB
    }
}
