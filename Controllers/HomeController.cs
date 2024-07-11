using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BlackSales.Models;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Net.Mail;
using System.Net;
using System.Reflection;

namespace BlackSales.Controllers
{
    public class HomeController : Controller
    {
        

        public IActionResult Index(int? categoryId)
        {
            ViewData["Title"] = "Ana Sayfa";

            using var connection = new SqlConnection(connectionString);

            List<Product> products;

            if (categoryId.HasValue)
            {
                // Kategoriye göre ürünleri getirirken parametreli sorgu
                products = connection.Query<Product>("SELECT * FROM posts WHERE IsApproved = 1 AND CategoryId = @CategoryId", new { CategoryId = categoryId.Value }).ToList();
            }
            else
            {
                // Tüm ürünleri getir
                products = connection.Query<Product>("SELECT * FROM posts WHERE IsApproved = 1").ToList();
            }

            var categories = connection.Query<Category>("SELECT * FROM categry").ToList();

            var viewModel = new ProductCategoryViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId 
            };

            return View(viewModel);
        }

        public IActionResult PostAdd()
        {
            ViewData["Title"] = "İlan Ekle";
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT * FROM posts").ToList();
            var categories = connection.Query<Category>("SELECT * FROM categry").ToList();

            var viewModel = new ProductCategoryViewModel
            {
                Products = products,
                Categories = categories
            };

            return View(viewModel);
        }
        public IActionResult Categories(int id)
        {
            ViewData["Title"] = "Kategoriler";
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM posts WHERE id =@id";
            var post = connection.QuerySingleOrDefault<Product>(sql, new {id});

            return View(post);
        }

       
    }
}
