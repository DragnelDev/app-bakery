using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPasteleria.Models;

namespace WebPasteleria.Controllers
{
    [Area("Admin")]
    public class EmpleadosController : Controller
    {
        private readonly FinalPasteleriaContext _context;

        public EmpleadosController(FinalPasteleriaContext context)
        {
            _context = context;
        }

        // GET: Empleados
        public async Task<IActionResult> Index()
        {
            var empleados = await _context.Empleado
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();
            return View(empleados);
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleado
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empleados/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CedulaIdentidad,Nombre,ApellidoPaterno,ApellidoMaterno,FechaNacimiento,Direccion,Celular,Email,Cargo")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                // Validaciones de negocio
                var cedulaExistente = await _context.Empleado
                    .AnyAsync(e => e.CedulaIdentidad == empleado.CedulaIdentidad);

                if (cedulaExistente)
                {
                    ModelState.AddModelError("CedulaIdentidad", "Esta cédula de identidad ya está registrada.");
                    return View(empleado);
                }

                var emailExistente = await _context.Empleado
                    .AnyAsync(e => e.Email == empleado.Email);

                if (emailExistente)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está registrado.");
                    return View(empleado);
                }

                // Asignar valores por defecto
                empleado.UsuarioRegistro = User?.Identity?.Name ?? "Sistema";
                empleado.FechaRegistro = DateTime.Now;
                empleado.Estado = 1; // Activo por defecto

                _context.Add(empleado);
                await _context.SaveChangesAsync();
                
                TempData["Mensaje"] = "Empleado registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleado.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CedulaIdentidad,Nombre,ApellidoPaterno,ApellidoMaterno,FechaNacimiento,Direccion,Celular,Email,Cargo,Estado")] Empleado empleado)
        {
            if (id != empleado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el empleado original
                    var empleadoOriginal = await _context.Empleado.FindAsync(id);
                    if (empleadoOriginal == null)
                    {
                        return NotFound();
                    }

                    // Validar que la cédula no exista en otro empleado
                    if (empleadoOriginal.CedulaIdentidad != empleado.CedulaIdentidad)
                    {
                        var cedulaExistente = await _context.Empleado
                            .AnyAsync(e => e.CedulaIdentidad == empleado.CedulaIdentidad && e.Id != id);

                        if (cedulaExistente)
                        {
                            ModelState.AddModelError("CedulaIdentidad", "Esta cédula de identidad ya está registrada.");
                            return View(empleado);
                        }
                    }

                    // Validar que el email no exista en otro empleado
                    if (empleadoOriginal.Email != empleado.Email)
                    {
                        var emailExistente = await _context.Empleado
                            .AnyAsync(e => e.Email == empleado.Email && e.Id != id);

                        if (emailExistente)
                        {
                            ModelState.AddModelError("Email", "Este correo electrónico ya está registrado.");
                            return View(empleado);
                        }
                    }

                    // Mantener los datos de registro original
                    empleado.UsuarioRegistro = empleadoOriginal.UsuarioRegistro;
                    empleado.FechaRegistro = empleadoOriginal.FechaRegistro;

                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    
                    TempData["Mensaje"] = "Empleado actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleado
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleado.FindAsync(id);
            if (empleado != null)
            {
                // Verificar si el empleado tiene usuarios asociados
                var usuariosAsociados = await _context.Usuario
                    .AnyAsync(u => u.IdEmpleado == empleado.Id);

                if (usuariosAsociados)
                {
                    TempData["Error"] = "No se puede eliminar este empleado porque tiene usuarios asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Empleado.Remove(empleado);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Empleado eliminado exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Empleados/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var empleados = await _context.Empleado
                .Where(e => e.Nombre.Contains(searchTerm) ||
                           e.ApellidoPaterno.Contains(searchTerm) ||
                           e.ApellidoMaterno.Contains(searchTerm) ||
                           e.CedulaIdentidad.Contains(searchTerm) ||
                           e.Email.Contains(searchTerm))
                .OrderByDescending(e => e.FechaRegistro)
                .ToListAsync();

            return View("Index", empleados);
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleado.Any(e => e.Id == id);
        }
    }
}
