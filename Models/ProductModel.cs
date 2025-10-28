using Shopping_Tutorial.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Shopping_Tutorial.Models
{
    public class ProductModel
    {
        [Key]
        public long Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
        public string Name { get; set; }


        public int Quantity { get; set; }

        public int SoldOut { get; set; }

        public string Slug { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        public decimal Price { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public string Image { get; set; }
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }

        public RatingModel Ratings { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }


    }
}
