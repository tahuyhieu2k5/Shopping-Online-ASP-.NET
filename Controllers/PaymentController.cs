using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly DataContext _dataContext;

        public PaymentController(IMomoService momoService, IVnPayService vnPayService, DataContext dataContext)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfo model)
        {
           
                var response = await _momoService.CreatePaymentAsync(model);

                if (string.IsNullOrEmpty(response?.PayUrl))
                {
                    ViewBag.Message = "Không tạo được URL thanh toán MoMo.";
                return Content("Kết quả thanh toán: " + ViewBag.Message);

            }

            return Redirect(response.PayUrl);
           
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Tìm đơn hàng theo OrderCode (txnRef)
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == response.OrderId);

            if (order == null)
            {
                ViewBag.Message = "Không tìm thấy đơn hàng.";
                return View("PaymentResult", response);
            }

            if (response.Success && response.VnPayResponseCode == "00")
            {
                order.Status = 1; // Thành công
                order.PaymentMethod = "VNPay";
                order.TransactionId = response.TransactionId;
                await _dataContext.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                ViewBag.Message = "Thanh toán VNPay thành công!";
            }
            else
            {
                order.Status = 2; // Thất bại
                await _dataContext.SaveChangesAsync();

                ViewBag.Message = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}";
            }

            return View("PaymentResult", response);
        }

        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            model.TxnRef = Guid.NewGuid().ToString("N");

            var userEmail = User.FindFirstValue(ClaimTypes.Email); // ✅ Lấy email user

            var order = new OrderModel
            {
                OrderCode = model.TxnRef,
                Amount = (decimal)model.Amount,
                Description = model.OrderDescription,
                Status = 0,
                CreatedDate = DateTime.UtcNow,
                UserName = userEmail, // ✅ Gán email làm UserName để truy vấn được
                ShippingCost = 0,
                CouponCode = null
            };

            _dataContext.Orders.Add(order);
            await _dataContext.SaveChangesAsync();

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }


    }
}
