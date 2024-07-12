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
            ViewData["Title"] = "Admin";
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
            var sql = "INSERT INTO posts (Name, Price, CreatedDate, UpdatedDate, CategoryId, ImagePath, Mail) VALUES (@Name, @Price, @CreatedDate, @UpdatedDate, @CategoryId, @ImagePath, @Mail)";
            var data = new
            {
                model.Name,
                model.Price,
                model.CreatedDate,
                model.UpdatedDate,
                model.CategoryId,
                model.ImagePath,
                model.Mail,
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
        public IActionResult Edit(Product model, string ExistingImagePath)
        {
            ViewData["Title"] = "Kayıt Düzenle";

            // Model geçerli değilse
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            // Eğer resim güncellemek istersek bu kısım kullanılıyor.
            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }
            else
            {
                // Mevcut resim ile devam ediyorum
                model.ImagePath = ExistingImagePath;
            }
            

            

            using var conn = new SqlConnection(connectionString);

            // Mevcut e-posta adresini veritabanından alıyorum
            var sqlMail = "SELECT Mail FROM posts WHERE Id = @Id";
            var existingMail = conn.QuerySingleOrDefault<string>(sqlMail, new { model.Id });

            if (existingMail == null)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Kayıt bulunamadı.";
                return View("Message");
            }

            var sqlUpdate = "UPDATE posts SET CategoryId=@CategoryId, Name=@Name, ImagePath=@ImagePath, Price=@Price, UpdatedDate=@UpdatedDate, Mail=@Mail WHERE Id = @Id";
            var param = new
            {
                model.CategoryId,
                model.Name,
                model.ImagePath,
                model.Price,
                UpdatedDate = DateTime.Now,
                Mail = existingMail, // Veritabanından alınan mevcut e-posta adresi
                model.Id
            };

            var affectedRows = conn.Execute(sqlUpdate, param);

            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Güncellendi.";
            return View("Message");
        }


        public IActionResult Delete(int id)
        {

            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE from posts WHERE Id = " + id;
            var rowsAffected = connection.Execute(sql, new { id });
            return RedirectToAction("index");

        }
        public IActionResult Pending()
        {
            ViewData["Title"] = "Admin - Onay";
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Product>("SELECT * FROM posts").ToList();

            return View(posts);
        }
        public IActionResult PostApproval(int? id)
        {
            var PostModel = new ProductCategoryViewModel();
            string mail = string.Empty;
            // Connect to the database
            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "UPDATE posts SET IsApproved = 1 WHERE Id = @Id";
                var rowsAffected = connection.Execute(sql, new { Id = id });
                
            }
            using (var connection = new SqlConnection(connectionString))
            {
                var sql = $"SELECT Mail FROM posts WHERE Id =" + id;
                 mail = connection.Query<string>(sql).First();
                
            }

            

            var mailMassage = new MailMessage
            {
                From = new MailAddress("postmaster@bildirim.bariscakdi.com.tr", "Barış"),
                Subject = "bariscakdi.com.tr'den mesaj var!!",
                Body = "İlanınız onaylanmıştır.",
                IsBodyHtml = true,
            };
            //bu kısımda gönderilen maili yönlendirmek istediğimiz mail adresi
            mailMassage.ReplyToList.Add("bariscakdi@gmail.com");

            mailMassage.To.Add(new MailAddress(mail, "BlackSales"));


            client.Send(mailMassage);




            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Onaylandı";
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

    }
}
