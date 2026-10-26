using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebPasteleria.Domain;

namespace WebPasteleria.ViewModels;

public class ClienteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La cédula de identidad es obligatoria.")]
    [StringLength(12)]
    [Display(Name = "Cédula de identidad")]
    public string CedulaIdentidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(100)]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [StringLength(50)]
    [Display(Name = "Tipo de cliente")]
    public string? TipoCliente { get; set; }

    [StringLength(500)]
    [Display(Name = "Preferencias")]
    public string? Preferencias { get; set; }

    [Range(0, 1, ErrorMessage = "Estado inválido.")]
    [Display(Name = "Estado")]
    public byte Estado { get; set; } = EstadoRegistro.Activo;

    [BindNever]
    public string? UsuarioRegistro { get; set; }

    [BindNever]
    public DateTime FechaRegistro { get; set; }
}
