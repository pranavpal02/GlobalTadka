using GlobalTadka.Data;
using GlobalTadka.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe; // For Stripe Checkout

namespace GlobalTadka.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private Repository<GlobalTadka.Models.Product> _products; // Fully qualify Product here
        private Repository<Order> _orders;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _products = new Repository<GlobalTadka.Models.Product>(context); // Fully qualify Product here
            _orders = new Repository<Order>(context);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //Retrieve or create an OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _products.GetAllAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddItem(int prodId, int prodQty)
        {
            var product = await _context.Products.FindAsync(prodId);
            if (product == null)
            {
                return NotFound();
            }

            // Retrieve or create an OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _products.GetAllAsync()
            };

            // Check if the product is already in the order
            var existingItem = model.OrderItems.FirstOrDefault(oi => oi.ProductId == prodId);

            // If the product is already in the order, update the quantity
            if (existingItem != null)
            {
                existingItem.Quantity += prodQty;
            }
            else
            {
                model.OrderItems.Add(new OrderItemViewModel
                {
                    ProductId = product.ProductId,
                    Price = product.Price,
                    Quantity = prodQty,
                    ProductName = product.Name
                });
            }

            // Update the total amount
            model.TotalAmount = model.OrderItems.Sum(oi => oi.Price * oi.Quantity);

            // Save updated OrderViewModel to session
            HttpContext.Session.Set("OrderViewModel", model);

            // Redirect back to Create to show updated order items
            return RedirectToAction("Create", model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            // Retrieve the OrderViewModel from session or other state management
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");

            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create");
            }

            // Create a new Order entity
            Order order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount,
                UserId = _userManager.GetUserId(User)
            };

            // Add OrderItems to the Order entity
            foreach (var item in model.OrderItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            // Save the Order entity to the database
            await _orders.AddAsync(order);

            // Clear the OrderViewModel from session or other state management
            HttpContext.Session.Remove("OrderViewModel");

            // Redirect to the Order Confirmation page
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User);

            var userOrders = await _orders.GetAllByIdAsync(userId, "UserId", new QueryOptions<Order>
            {
                Includes = "OrderItems.Product"
            });

            return View(userOrders);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
                return RedirectToAction("Create");

            // Ensure the total amount is at least $0.50
            
            if (model.TotalAmount < (decimal)0.50)
            {
                
                TempData["ErrorMessage"] = "The total order amount must be at least $0.50.";
                return RedirectToAction("Cart");
            }


            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = model.OrderItems.Select(item => new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // price in cents
                        Currency = "usd", // Set currency to USD
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName
                        }
                    },
                    Quantity = item.Quantity
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + Url.Action("Success"),
                CancelUrl = domain + Url.Action("Cart")
            };

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Success()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create");
            }

            // Create and save order after payment is successful
            Order order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount,
                UserId = _userManager.GetUserId(User)
            };

            foreach (var item in model.OrderItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            await _orders.AddAsync(order);

            HttpContext.Session.Remove("OrderViewModel");

            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        public IActionResult Cancel()
        {
            return RedirectToAction("Cart");
        }
    }
}
