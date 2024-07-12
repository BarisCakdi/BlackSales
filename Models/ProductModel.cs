using System.ComponentModel.DataAnnotations;

namespace BlackSales.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Ürün fiyatı zorunludur.")]
        public int Price { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CategoryId { get; set; }
        public bool IsApproved { get; set; }
        public string Mail {  get; set; }  


    }
    public class Category
    {
        public int id { get; set; }
        public string Name { get; set; }
        public IFormFile Img { get; set; }
        public string? ImagePath { get; set; }
    }
    
    public class ProductCategoryViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
    }
}
