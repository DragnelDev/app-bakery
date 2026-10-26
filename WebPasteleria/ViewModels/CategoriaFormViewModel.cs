using System.ComponentModel.DataAnnotations;
using WebPasteleria.Domain;

namespace WebPasteleria.ViewModels;

public class CategoriaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 1, ErrorMessage = "Estado inválido.")]
    [Display(Name = "Estado")]
    public byte Estado { get; set; } = EstadoRegistro.Activo;
}
