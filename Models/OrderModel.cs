namespace Shopping_Tutorial.Models
{
	public class OrderModel
	{
		public int Id { get; set; }
		public string OrderCode { get; set; }

		public decimal ShippingCost { get; set; }
		public string CouponCode { get; set; }
		public string UserName { get; set; }
		public DateTime CreatedDate { get; set; }

		public int Status { get; set; }
        public decimal Amount { get; set; }    // số tiền thanh toán

        public string Description { get; set; }  // mô tả đơn hàng

        public string TransactionId { get; set; }  // mã giao dịch thanh toán (vnp_TransactionNo)

        public string PaymentMethod { get; set; }  // ví dụ "VNPay", "Momo"
    }
}
