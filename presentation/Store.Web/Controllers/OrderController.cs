using Microsoft.AspNetCore.Mvc;
using Store.Web.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace Store.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBookRepository  bookRepository;
        private readonly IOrderRepository orderRepository;

        public OrderController(IBookRepository bookRepository,
                                              IOrderRepository orderRepository)
        {
            this.bookRepository = bookRepository;
            this.orderRepository = orderRepository;
        }

        [HttpGet]
        public IActionResult Index() 
        {
            if (HttpContext.Session.TryGetCart(out Cart cart))
            {
                var order = orderRepository.GetById(cart.OrderId);
                OrderModel model = Map(order);

                return View(model);
            }

            return View("Empty");  
        }

        public OrderModel Map(Order order) 
        {
            var bookId = order.Items.Select(item => item.BookId);
            var books = bookRepository.GetAllByIds(bookId);
            var itemModels = from item in order.Items
                             join book in books on item.BookId equals book.Id
                             select new OrderItemModel
                             {
                                 BookId = book.Id,
                                 Title = book.Title,
                                 Author = book.Author,
                                 Price = item.Price,
                                 Count = item.Count,
                             };
            return new OrderModel
            {
                Id = order.Id,
                Items = itemModels.ToArray(),
                TotalCount = order.TotalCount,
                TotalPrice = order.TotalPrice,
            };
        }
        // когда нажимаем кнопку добавить в корзину 
        [HttpPost]
        public IActionResult AddItem(int bookId, int count = 1)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();

            var book = bookRepository.GetById(bookId);

            order.AddOrUpdateItem(book, count);

            SaveOrderAndCard(order, cart);

            return RedirectToAction("Index", "Book", new { id = bookId });
        }


        [HttpPost]
        public IActionResult UpdateItem(int bookId, int count)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();

            order.GetItem(bookId).Count = count;
            SaveOrderAndCard(order, cart);

            return RedirectToAction("Index", "Order");
        }

        private void SaveOrderAndCard(Order order, Cart cart)
        {
            orderRepository.Update(order);

            cart.TotalCount = order.TotalCount;
            cart.TotalPrice = order.TotalPrice;

            HttpContext.Session.Set(cart);
        }

        private (Order order, Cart cart) GetOrCreateOrderAndCart()
        {
            Order order;
            if (HttpContext.Session.TryGetCart(out Cart cart))
            {
                order = orderRepository.GetById(cart.OrderId);
            }
            else
            {
                order = orderRepository.Create();
                cart = new Cart(order.Id);
            }

            return (order, cart);
        }

        [HttpPost]
        public IActionResult RemoveItem(int bookId)
        {
            (Order order, Cart cart) = GetOrCreateOrderAndCart();

            order.RemoveItem(bookId);

            SaveOrderAndCard(order, cart);

            return RedirectToAction("Index", "Order");
        }

        [HttpPost]
        public IActionResult SendConfirmationCode(int id, string cellPhone)
        {
            var order = orderRepository.GetById(id);
            var model = Map(order);

            if (!IsValidCellPhone(cellPhone))
            {
                model.Error["cellPone"] = "Номер телефона не соответствует";
                return View("Index", model);
            }

            int code = 1111; // random.Next(1000, 10000)
            HttpContext.Session.SetInt32(cellPhone, code);
            //notificationServis.SendConfirmationCode(cellPhone, code);

            return View("Confirmation",
                        new ConfirmationModel
                        {
                            OrderId = id,
                            CellPhone = cellPhone
                        });
        }

        private bool IsValidCellPhone(string cellPhone)
        {
            if (cellPhone == null)
                return false;

            cellPhone = cellPhone.Replace(" ", "")
                                 .Replace("-", "");

            return Regex.IsMatch(cellPhone, @"^\+?\d{11}$");
        }

        [HttpPost]
        public IActionResult StartDelivery(int id, string cellPhone, int code)
        {
            int? storedCode = HttpContext.Session.GetInt32(cellPhone);
            if (storedCode == null)
            {
                return View("Confirmation",
                        new ConfirmationModel
                        {
                            OrderId = id,
                            CellPhone = cellPhone,
                            Error = new Dictionary<string, string>
                            {
                                { "code", "Код не может быть пустым." }
                            }
                        });
            }

            if (storedCode != code)
            {
                return View("Confirmation",
                        new ConfirmationModel
                        {
                            OrderId = id,
                            CellPhone = cellPhone,
                            Error = new Dictionary<string, string>
                            {
                                { "code", "Неверный код." }
                            }
                        });
            }

            return View();
        }


    }
}
    