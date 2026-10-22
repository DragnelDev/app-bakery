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
    public class CarritoDetallesController : Controller
    {
        private readonly FinalPasteleriaContext _context;

        public CarritoDetallesController(FinalPasteleriaContext context)
        {
            _context = context;
        }

        // GET: CarritoDetalles
        public async Task<IActionResult> Index()
        {
            var finalPasteleriaContext = _context.CarritoDetalle.Include(c => c.IdCarritoNavigation).Include(c => c.IdProductoNavigation);
            return View(await finalPasteleriaContext.ToListAsync());
        }

        // GET: CarritoDetalles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carritoDetalle = await _context.CarritoDetalle
                .Include(c => c.IdCarritoNavigation)
                .Include(c => c.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carritoDetalle == null)
            {
                return NotFound();
            }

            return View(carritoDetalle);
        }

        // GET: CarritoDetalles/Create
        public IActionResult Create()
        {
            ViewData["IdCarrito"] = new SelectList(_context.Carrito, "Id", "Id");
            ViewData["IdProducto"] = new SelectList(_context.Producto, "Id", "Id");
            return View();
        }

        // POST: CarritoDetalles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCarrito,IdProducto,Cantidad,PrecioUnitario,SubTotal")] CarritoDetalle carritoDetalle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(carritoDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCarrito"] = new SelectList(_context.Carrito, "Id", "Id", carritoDetalle.IdCarrito);
            ViewData["IdProducto"] = new SelectList(_context.Producto, "Id", "Id", carritoDetalle.IdProducto);
            return View(carritoDetalle);
        }

        // GET: CarritoDetalles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carritoDetalle = await _context.CarritoDetalle.FindAsync(id);
            if (carritoDetalle == null)
            {
                return NotFound();
            }
            ViewData["IdCarrito"] = new SelectList(_context.Carrito, "Id", "Id", carritoDetalle.IdCarrito);
            ViewData["IdProducto"] = new SelectList(_context.Producto, "Id", "Id", carritoDetalle.IdProducto);
            return View(carritoDetalle);
        }

        // POST: CarritoDetalles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCarrito,IdProducto,Cantidad,PrecioUnitario,SubTotal")] CarritoDetalle carritoDetalle)
        {
            if (id != carritoDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carritoDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarritoDetalleExists(carritoDetalle.Id))
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
            ViewData["IdCarrito"] = new SelectList(_context.Carrito, "Id", "Id", carritoDetalle.IdCarrito);
            ViewData["IdProducto"] = new SelectList(_context.Producto, "Id", "Id", carritoDetalle.IdProducto);
            return View(carritoDetalle);
        }

        // GET: CarritoDetalles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carritoDetalle = await _context.CarritoDetalle
                .Include(c => c.IdCarritoNavigation)
                .Include(c => c.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carritoDetalle == null)
            {
                return NotFound();
            }

            return View(carritoDetalle);
        }

        // POST: CarritoDetalles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carritoDetalle = await _context.CarritoDetalle.FindAsync(id);
            if (carritoDetalle != null)
            {
                _context.CarritoDetalle.Remove(carritoDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarritoDetalleExists(int id)
        {
            return _context.CarritoDetalle.Any(e => e.Id == id);
        }
    }
}
