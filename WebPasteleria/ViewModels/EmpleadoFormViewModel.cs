using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebPasteleria.Domain;

namespace WebPasteleria.ViewModels;

public class EmpleadoFormViewModel
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

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Apellido paterno")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Apellido materno")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El celular es obligatorio.")]
    [StringLength(20)]
    [Display(Name = "Celular")]
    public string Celular { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(100)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Cargo")]
    public string Cargo { get; set; } = string.Empty;

    [Range(0, 1, ErrorMessage = "Estado inválido.")]
    [Display(Name = "Estado")]
    public byte Estado { get; set; } = EstadoRegistro.Activo;

    [BindNever]
    public string? UsuarioRegistro { get; set; }

    [BindNever]
    public DateTime FechaRegistro { get; set; }
}
