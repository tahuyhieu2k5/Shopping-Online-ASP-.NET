using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Vnpay;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
	public class CheckoutController : Controller
	{

		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		private readonly IVnPayService _vnPayService;
		private static readonly HttpClient client = new HttpClient();
		public CheckoutController(IEmailSender emailSender, DataContext context, IVnPayService vnPayService)
		{
			_dataContext = context;
			_emailSender = emailSender;
			_vnPayService = vnPayService;

		}
		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{

				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = ordercode;

				orderItem.UserName = userEmail;
				orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;
				// Retrieve shipping price from cookie
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;

				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}
				orderItem.ShippingCost = shippingPrice;
				//Nhận coupon code
				var CouponCode = Request.Cookies["CouponTitle"];
				orderItem.CouponCode = CouponCode;
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
				//tạo order detail
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach (var cart in cartItems)
				{
					var orderdetail = new OrderDetail();
					orderdetail.UserName = userEmail;
					orderdetail.OrderCode = ordercode;
					orderdetail.ProductId = cart.ProductId;
					orderdetail.Price = cart.Price;
					orderdetail.Quantity = cart.Quantity;
					//update product quantity
					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.SoldOut += cart.Quantity;
					_dataContext.Update(product);
					_dataContext.Add(orderdetail);
					_dataContext.SaveChanges();

				}
				HttpContext.Session.Remove("Cart");
				//Send mail order when success
				//var receiver = userEmail;
				//var subject = "Đặt hàng thành công";
				//var message = "Đặt hàng thành công, trải nghiệm dịch vụ nhé.";

				//await _emailSender.SendEmailAsync(receiver, subject, message);

				TempData["success"] = "Đơn hàng đã được tạo,vui lòng chờ duyệt đơn hàng nhé.";
				return RedirectToAction("History", "Account");
			}
			return View();
		}
		[HttpGet]
		public IActionResult PaymentCallbackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);

			return Json(response);
		}




	}
}
