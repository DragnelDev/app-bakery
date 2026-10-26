using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebPasteleria.Domain;

namespace WebPasteleria.ViewModels;

public class UsuarioFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un empleado.")]
    [Display(Name = "Empleado")]
    public int IdEmpleado { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Obligatoria al crear. Al editar, si se deja vacía se conserva la actual.</summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Clave { get; set; }

    [Compare(nameof(Clave), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarClave { get; set; }

    [Required(ErrorMessage = "Selecciona un rol.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = Roles.Empleado;

    [Range(0, 1, ErrorMessage = "Estado inválido.")]
    [Display(Name = "Estado")]
    public byte Estado { get; set; } = EstadoRegistro.Activo;

    [Display(Name = "Desbloquear cuenta (reiniciar intentos fallidos)")]
    public bool ReiniciarIntentos { get; set; }

    [BindNever]
    public int IntentosFallidos { get; set; }

    [BindNever]
    public DateTime? UltimoLogin { get; set; }
}
