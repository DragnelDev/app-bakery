using System.ComponentModel.DataAnnotations;

namespace WebPasteleria.ViewModels;

public class ContactoViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [StringLength(2000)]
    public string Mensaje { get; set; } = string.Empty;
}
