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
    public class ProductosController : Controller
    {
        private readonly FinalPasteleriaContext _context;

        public ProductosController(FinalPasteleriaContext context)
        {
            _context = context;
        }

        // =============================================
        // VISTA PÚBLICA - Catálogo de Productos
        // =============================================
        
        // GET: Productos (Vista del cliente)
        public async Task<IActionResult> Index(string? categoria, string? buscar, string? ordenar)
        {
            // Query base: solo productos activos
            var query = _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Estado == 1);

            // Filtrar por categoría
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.IdCategoriaNavigation.Nombre.ToLower() == categoria.ToLower());
            }

            // Búsqueda por nombre o código
            if (!string.IsNullOrEmpty(buscar))
            {
                buscar = buscar.ToLower();
                query = query.Where(p => 
                    p.Nombre.ToLower().Contains(buscar) || 
                    (p.Codigo != null && p.Codigo.ToLower().Contains(buscar)) ||
                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(buscar))
                );
            }

            // Ordenamiento
            query = ordenar switch
            {
                "name-asc" => query.OrderBy(p => p.Nombre),
                "name-desc" => query.OrderByDescending(p => p.Nombre),
                "price-asc" => query.OrderBy(p => p.Precio),
                "price-desc" => query.OrderByDescending(p => p.Precio),
                _ => query.OrderBy(p => p.Nombre)
            };

            var productos = await query.ToListAsync();

            // Pasar categorías disponibles a la vista
            ViewBag.Categorias = await _context.CategoriaProducto.ToListAsync();
            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.TerminoBusqueda = buscar;

            return View(productos);
        }

        // GET: Productos/Detalle/5 (Vista pública de detalle)
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id && m.Estado == 1);

            if (producto == null)
            {
                return NotFound();
            }

            // Productos relacionados de la misma categoría
            var productosRelacionados = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.IdCategoria == producto.IdCategoria && p.Id != producto.Id && p.Estado == 1)
                .Take(4)
                .ToListAsync();

            ViewBag.ProductosRelacionados = productosRelacionados;

            return View(producto);
        }

        // GET: Productos por Categoría
        public async Task<IActionResult> PorCategoria(string categoria)
        {
            if (string.IsNullOrEmpty(categoria))
            {
                return RedirectToAction(nameof(Index));
            }

            var productos = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .Where(p => p.Estado == 1 && p.IdCategoriaNavigation.Nombre.ToLower() == categoria.ToLower())
                .ToListAsync();

            ViewBag.Categorias = await _context.CategoriaProducto.ToListAsync();
            ViewBag.CategoriaSeleccionada = categoria;

            return View("Index", productos);
        }

        // =============================================
        // API ENDPOINTS (Para AJAX)
        // =============================================

        // API: Obtener producto por ID (para modal de vista rápida)
        [HttpGet]
        public async Task<IActionResult> ObtenerProducto(int id)
        {
            var producto = await _context.Producto
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id && m.Estado == 1);

            if (producto == null)
            {
                return NotFound(new { mensaje = "Producto no encontrado" });
            }

            return Json(new
            {
                id = producto.Id,
                nombre = producto.Nombre,
                codigo = producto.Codigo,
                precio = producto.Precio,
                descripcion = producto.Descripcion ?? "Sin descripción",
                stock = producto.Stock,
                imagenUrl = producto.ImagenUrl ?? "/images/no-image.jpg",
                categoria = producto.IdCategoriaNavigation?.Nombre ?? "Sin categoría",
                unidadMedida = producto.UnidadMedida,
                tipo = producto.Tipo
            });
        }

        // API: Buscar productos (para autocomplete)
        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string termino)
        {
            if (string.IsNullOrEmpty(termino))
            {
                return Json(new List<object>());
            }

            termino = termino.ToLower();
            
            var productos = await _context.Producto
                .Where(p => p.Estado == 1 && 
                       (p.Nombre.ToLower().Contains(termino) || 
                        (p.Codigo != null && p.Codigo.ToLower().Contains(termino))))
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    precio = p.Precio,
                    stock = p.Stock,
                    imagenUrl = p.ImagenUrl ?? "/images/no-image.jpg"
                })
                .Take(10)
                .ToListAsync();

            return Json(productos);
        }

        // API: Verificar stock
        [HttpGet]
        public async Task<IActionResult> VerificarStock(int id, int cantidad)
        {
            var producto = await _context.Producto
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado == 1);

            if (producto == null)
            {
                return Json(new { disponible = false, mensaje = "Producto no encontrado" });
            }

            bool disponible = producto.Stock >= cantidad;
            
            return Json(new
            {
                disponible = disponible,
                stockDisponible = producto.Stock,
                mensaje = disponible 
                    ? "Stock disponible" 
                    : $"Solo hay {producto.Stock} unidades disponibles"
            });
        }
    }
}