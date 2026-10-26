using System.ComponentModel.DataAnnotations;

namespace WebPasteleria.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu usuario.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Clave { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }
}
