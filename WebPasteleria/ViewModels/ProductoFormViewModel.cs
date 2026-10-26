using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebPasteleria.Domain;

namespace WebPasteleria.ViewModels;

public class ProductoFormViewModel
{
    public int Id { get; set; }

    [StringLength(50)]
    [Display(Name = "Código")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una categoría.")]
    [Display(Name = "Categoría")]
    public int IdCategoria { get; set; }

    [StringLength(20)]
    [Display(Name = "Unidad de medida")]
    public string? UnidadMedida { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "El precio debe ser mayor a cero.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    [Display(Name = "Stock")]
    public int? Stock { get; set; }

    [StringLength(50)]
    [Display(Name = "Tipo")]
    public string? Tipo { get; set; }

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [StringLength(500)]
    [Display(Name = "URL de la imagen")]
    public string? ImagenUrl { get; set; }

    [Range(0, 1, ErrorMessage = "Estado inválido.")]
    [Display(Name = "Estado")]
    public byte Estado { get; set; } = EstadoRegistro.Activo;

    // Solo lectura: se muestran en el formulario pero nunca se aceptan del cliente.
    [BindNever]
    public string? UsuarioRegistro { get; set; }

    [BindNever]
    public DateTime FechaRegistro { get; set; }
}
