using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using furniture_store.Models;
using Microsoft.AspNetCore.Authorization;

namespace furniture_store.Controllers;

public class CartController : Controller
{
    private readonly FurnitureContext _context;
    private const string CartCookieName = "luxe_cart";

    public CartController(FurnitureContext context)
    {
        _context = context;
    }

    private List<CartItem> GetCartItems()
    {
        var cookieValue = Request.Cookies[CartCookieName];
        if (string.IsNullOrEmpty(cookieValue))
        {
            return new List<CartItem>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<CartItem>>(cookieValue) ?? new List<CartItem>();
        }
        catch
        {
            return new List<CartItem>();
        }
    }

    private void SaveCartItems(List<CartItem> items)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(7),
            Path = "/",
            HttpOnly = true,
            Secure = false // set true in production
        };

        var json = JsonSerializer.Serialize(items);
        Response.Cookies.Append(CartCookieName, json, cookieOptions);
    }

    // GET: /Cart
    [HttpGet]
    public IActionResult Index()
    {
        var items = GetCartItems();
        return View(items);
    }

    // POST: /Cart/Add
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1, bool buyNow = false)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var items = GetCartItems();
        var existingItem = items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                ImageUrl = product.ImageUrl ?? "",
                Price = product.Price,
                Quantity = quantity
            });
        }

        SaveCartItems(items);
        
        if (buyNow)
        {
            return RedirectToAction("Index");
        }

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Update
    [HttpPost]
    public IActionResult Update(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            return Remove(productId);
        }

        var items = GetCartItems();
        var existingItem = items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity = quantity;
        }

        SaveCartItems(items);
        return RedirectToAction("Index");
    }

    // POST: /Cart/Remove
    [HttpPost]
    public IActionResult Remove(int productId)
    {
        var items = GetCartItems();
        var itemToRemove = items.FirstOrDefault(i => i.ProductId == productId);

        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);
        }

        SaveCartItems(items);
        return RedirectToAction("Index");
    }

    // POST: /Cart/Clear
    [HttpPost]
    public IActionResult Clear()
    {
        Response.Cookies.Delete(CartCookieName);
        return RedirectToAction("Index");
    }

    // GET: /Cart/Checkout
    [HttpGet]
    [Authorize]
    public IActionResult Checkout()
    {
        var items = GetCartItems();
        if (!items.Any())
        {
            return RedirectToAction("Index");
        }

        // Prefill user details if logged in
        ViewBag.CustomerName = "";
        ViewBag.CustomerEmail = "";
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            ViewBag.CustomerName = User.Identity.Name ?? "";
            var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email);
            if (emailClaim != null)
            {
                ViewBag.CustomerEmail = emailClaim.Value;
            }
        }

        return View(items);
    }

    // POST: /Cart/Checkout
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Checkout(string customerName, string customerPhone, string? customerEmail, string address, string? note, string paymentMethod = "cod")
    {
        var items = GetCartItems();
        if (!items.Any())
        {
            return RedirectToAction("Index");
        }

        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(address))
        {
            ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc (*).");
            return View(items);
        }

        // Create new Order
        var order = new Order
        {
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail,
            Address = address,
            Note = note,
            PaymentMethod = paymentMethod,
            Status = (paymentMethod == "bank" || paymentMethod == "momo") ? "paid" : "pending",
            CreatedAt = DateTime.Now,
            Total = items.Sum(i => i.Total)
        };

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                order.UserId = userId;
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Saves order and populates order.Id

        // Add order items
        foreach (var item in items)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.Name,
                Quantity = item.Quantity,
                Price = item.Price
            };
            _context.OrderItems.Add(orderItem);
        }
        await _context.SaveChangesAsync();

        // Clear cart
        Response.Cookies.Delete(CartCookieName);

        return RedirectToAction("OrderSuccess", new { id = order.Id });
    }

    // GET: /Cart/OrderSuccess/{id}
    [HttpGet]
    public async Task<IActionResult> OrderSuccess(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}
