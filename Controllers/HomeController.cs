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
        

        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT * FROM posts WHERE IsApproved = 1 ").ToList();
            var categories = connection.Query<Category>("SELECT * FROM categry").ToList();

            var viewModel = new ProductCategoryViewModel
            {
                Products = products,
                Categories = categories
            };

            return View(viewModel);
        }
        public IActionResult PostAdd()
        {
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

       
    }
}
