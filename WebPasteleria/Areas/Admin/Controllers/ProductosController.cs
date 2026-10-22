using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPasteleria.Models;

namespace WebPasteleria.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductosController : Controller
    {
        private readonly FinalPasteleriaContext _context;

        public ProductosController(FinalPasteleriaContext context)
        {
            _context = context;
        }

        // GET: Admin/Productos (Listado administrativo)
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();

            return View(productos);
        }

        // GET: Admin/Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // GET: Admin/Productos/Create
        public IActionResult Create()
        {
            ViewData["IdCategoria"] = new SelectList(_context.CategoriaProducto, "Id", "Nombre");
            return View();
        }

        private bool ValidarProducto(Producto producto, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                mensajeError = "El nombre del producto es requerido";
                return false;
            }

            if (producto.IdCategoria <= 0)
            {
                mensajeError = "Debe seleccionar una categoría válida";
                return false;
            }

            if (producto.Precio <= 0)
            {
                mensajeError = "El precio debe ser mayor a cero";
                return false;
            }

            if (producto.Stock < 0)
            {
                mensajeError = "El stock no puede ser negativo";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(producto.Codigo))
            {
                bool codigoExiste = _context.Producto
                    .Any(p => p.Codigo == producto.Codigo && p.Id != producto.Id);

                if (codigoExiste)
                {
                    mensajeError = "Ya existe un producto con ese código";
                    return false;
                }
            }

            return true;
        }

        // POST: Admin/Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ValidarProducto(producto, out string mensajeError))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(producto.Codigo))
                    {
                        var categoria = await _context.CategoriaProducto.FindAsync(producto.IdCategoria);
                        var prefijo = categoria?.Nombre.Substring(0, 3).ToUpper() ?? "PRO";
                        var numero = (_context.Producto.Count() + 1).ToString("D3");
                        producto.Codigo = $"{prefijo}{numero}";
                    }

                    producto.UsuarioRegistro = User.Identity?.Name ?? "System";
                    producto.FechaRegistro = DateTime.Now;
                    producto.Estado = 1;

                    _context.Add(producto);
                    await _context.SaveChangesAsync();

                    TempData["AlertType"] = "success";
                    TempData["AlertMessage"] = "Producto creado exitosamente";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = $"Error al crear el producto: {ex.Message}";
                }
            }
            else
            {
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = mensajeError;
            }

            ViewData["IdCategoria"] = new SelectList(_context.CategoriaProducto, "Id", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        // GET: Admin/Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Producto.FindAsync(id);
            if (producto == null) return NotFound();

            ViewData["IdCategoria"] = new SelectList(_context.CategoriaProducto, "Id", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        // POST: Admin/Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ValidarProducto(producto, out string mensajeError))
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    TempData["AlertType"] = "success";
                    TempData["AlertMessage"] = "Producto actualizado exitosamente";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = $"Error al actualizar el producto: {ex.Message}";
                }
            }
            else
            {
                TempData["AlertType"] = "warning";
                TempData["AlertMessage"] = mensajeError;
            }

            ViewData["IdCategoria"] = new SelectList(_context.CategoriaProducto, "Id", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        // GET: Admin/Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Admin/Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var producto = await _context.Producto.FindAsync(id);
                if (producto != null)
                {
                    producto.Estado = 0;
                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    TempData["AlertType"] = "success";
                    TempData["AlertMessage"] = "Producto eliminado exitosamente";
                }
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = $"Error al eliminar el producto: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Productos/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var producto = await _context.Producto.FindAsync(id);
                if (producto != null)
                {
                    producto.Estado = (byte)(producto.Estado == 1 ? 0 : 1);
                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        mensaje = producto.Estado == 1 ? "Producto activado" : "Producto desactivado",
                        nuevoEstado = producto.Estado
                    });
                }

                return Json(new { success = false, mensaje = "Producto no encontrado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = $"Error: {ex.Message}" });
            }
        }

        private bool ProductoExists(int id)
        {
            return _context.Producto.Any(e => e.Id == id);
        }
    }
}
