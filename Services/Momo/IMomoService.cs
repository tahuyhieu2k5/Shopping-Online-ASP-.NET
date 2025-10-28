using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Momo;

namespace Shopping_Tutorial.Services.Momo
{
	public interface IMomoService
	{
		Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfo model);
		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
	}
}
