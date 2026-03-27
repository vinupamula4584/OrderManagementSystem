using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data;
using OrderManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagementSystem.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Show all orders for Admin, only own orders for normal users
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Orders.ToListAsync());
            }

            var userId = _userManager.GetUserId(User);

            var userOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return View(userOrders);
        }

        // Show order details only if user owns it or is Admin
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // Show create form
        public IActionResult Create()
        {
            return View();
        }

        // Save new order and link it to logged-in user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ProductName,Quantity,Price,OrderDate,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.UserId = _userManager.GetUserId(User);
                order.CreatedByEmail = User.Identity.Name;
                order.OrderDate = DateTime.Now;

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // Show edit form only if user owns order or is Admin
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // Save edited order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,ProductName,Quantity,Price,OrderDate,Status,UserId,CreatedByEmail")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            var existingOrder = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && existingOrder.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    order.UserId = existingOrder.UserId;
                    order.CreatedByEmail = existingOrder.CreatedByEmail;

                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            return View(order);
        }

        // Show delete page only if user owns order or is Admin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }

        // Confirm delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (!User.IsInRole("Admin") && order.UserId != userId)
            {
                return Forbid();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}