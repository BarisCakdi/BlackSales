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
    public class AdminController : Controller
    {
        

        public IActionResult Index()
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
        [HttpPost]
        public IActionResult Add(Product model)
        {
            ViewData["Title"] = "Kayıt Ekle";

            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }

            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO posts (Name, Price, CreatedDate, UpdatedDate, CategoryId, ImagePath) VALUES (@Name, @Price, @CreatedDate, @UpdatedDate, @CategoryId, @ImagePath)";
            var data = new
            {
                model.Name,
                model.Price,
                model.CreatedDate,
                model.UpdatedDate,
                model.CategoryId,
                model.ImagePath,
            };

            var rowsAffected = connection.Execute(sql, data);

            
           
            var mailMassage = new MailMessage
            {
                From = new MailAddress("postmaster@bildirim.bariscakdi.com.tr", "Barış"),
                Subject = "bariscakdi.com.tr'den mesaj var!!",
                Body = "İlanınız onaylandıktan sonra yayına alınacaktır.",
                IsBodyHtml = true,
            };
            //bu kısımda gönderilen maili yönlendirmek istediğimiz mail adresi
            mailMassage.ReplyToList.Add("bariscakdi@gmail.com");

            mailMassage.To.Add(new MailAddress(model.Mail, "BlackSales"));


            client.Send(mailMassage);


            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "İlanınız onaylandıktan sonra yayınlanıcaktır.";
            return View("Message");
        }
        [HttpPost]
        public IActionResult Edit(Product model)
        {
            ViewData["Title"] = "Kayıt Düzenle";
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "UPDATE posts SET CategoryId=@CategoryID, Name=@Name, ImagePath=@ImagePath, Price=@Price, UpdatedDate=@UpdatedDate WHERE Id = @Id";
            var param = new
            {
                model.Price,
                model.Name,
                model.ImagePath,
                UpdatedDate = DateTime.Now,
                model.CategoryId,
                model.Id
            };

            var affectedRows = connection.Execute(sql, param);
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Güncellendi.";
            return View("Message");
        }
        public IActionResult Delete(int id)
        {

            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE from posts WHERE Id = @Id";
            var rowsAffected = connection.Execute(sql, new { id });
            return RedirectToAction("index");

        }
        public IActionResult Pending()
        {

            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Product>("SELECT * FROM posts").ToList();

            return View(posts);
        }
        public IActionResult PostApproval(int? id)
        {
            var PostModel = new ProductCategoryViewModel();
            // Connect to the database
            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "UPDATE posts SET IsApproved = 1 WHERE Id = @Id";
                var rowsAffected = connection.Execute(sql, new { Id = id });
                
            }
            using (var connection = new SqlConnection(connectionString))
            {
                var sql = $"SELECT Mail FROM posts WHERE Id = @Id";
                string Mail = connection.Query<string>(sql).First();
                //var posts = connection.Query<Product>(sql).ToList();
                //PostModel.Products = posts;
            }

            

            var mailMassage = new MailMessage
            {
                From = new MailAddress("postmaster@bildirim.bariscakdi.com.tr", "Barış"),
                Subject = "bariscakdi.com.tr'den mesaj var!!",
                Body = "İlanınız onaylandıktan sonra yayına alınacaktır.",
                IsBodyHtml = true,
            };
            //bu kısımda gönderilen maili yönlendirmek istediğimiz mail adresi
            mailMassage.ReplyToList.Add("bariscakdi@gmail.com");

            mailMassage.To.Add(new MailAddress("Mail", "BlackSales"));


            client.Send(mailMassage);




            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Güncellendi.";
            return View("Message");

        }
        
        [HttpPost]
        public IActionResult AddCategry(Category model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }
            using var connection = new SqlConnection(connectionString);

            var cate = "INSERT INTO categry (Name, ImagePath) VALUES (@Name, @ImagePath)";
            var data = new
            {
                model.Name,
                model.ImagePath,
            };

            var rowsAffected = connection.Execute(cate, data);



            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Eklendi";
            return View("Message");
        }
        [HttpPost]
        public IActionResult EditCategry(Category model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "UPDATE categry SET Name=@Name, ImagePath=@ImagePath WHERE id = @id";
            var param = new
            {
                model.Name,
                model.ImagePath,
                model.id
            };

            var affectedRows = connection.Execute(sql, param);
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Güncellendi.";
            return View("Message");
        }
        public IActionResult DelCategry(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE from categry WHERE id = @id";
            var rowsAffected = connection.Execute(sql, new { id });
            return RedirectToAction("index");

        }

        public IActionResult Contact()
        {
            
            var mailMassage = new MailMessage
            {
                From = new MailAddress("postmaster@bildirim.bariscakdi.com.tr", "Barış"),
                Subject = "bariscakdi.com.tr'den mesaj var!!",
                Body = $@"Kimden: <br>
                        E-Posta Adresi: <br>
                        Konu: <br>
                        Mesaj: ",
                IsBodyHtml = true,
            };
            //bu kısımda gönderilen maili yönlendirmek istediğimiz mail adresi
            mailMassage.ReplyToList.Add("bariscakdi@gmail.com");

            //mailMassage.To.Add("bariscakdi@gmail.com"); Bu direk olarak gönderme alt taraftaki özel olarak isim belirtme.
            mailMassage.To.Add(new MailAddress("bariscakdi@gmail.com", "baris"));

            client.Send(mailMassage);

            return View();
        }
    }
}
