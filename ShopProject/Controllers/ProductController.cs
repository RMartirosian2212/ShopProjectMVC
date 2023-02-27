using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Models;
using ShopProject_Models.ViewModels;
using ShopProject_Utility;

namespace ShopProject_DataAccess.Controllers
{
    [Authorize(Roles = WebConstant.AdminRole)]
    public class ProductController : Controller
    {
        public readonly IProductRepository _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductRepository db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Product>? products = _db.GetAll(includeProperties:"Category,ApplicationType");
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.GetAllDropdownList(WebConstant.CategoryName),
                ApplicationTypeSelectList = _db.GetAllDropdownList(WebConstant.ApplicationTypeName)
            };
            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;  // получим фотку и сохраним в этой переменной
                var webPath = _webHostEnvironment.WebRootPath;  //создадим путь для корневого файла wwwroot

                if (productVM.Product.Id == 0)    // узнаем,если изображение имеется(редактирование) или не имеется(создание)
                {
                    var upload = webPath + WebConstant.ImagePath;
                    var fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension; // обновим ссылку на image
                    TempData[WebConstant.Success] = "Product created successfully";
                    _db.Add(productVM.Product);
                }
                else //редактирование
                {
                    var objOld = _db.FirstOrDefault(o => o.Id == productVM.Product.Id, isTracking: false); // находим старый объект продукт; также добавляем AsNoTracking() , чтобы решить проблему 

                    if (files.Count > 0) // если фотка загружен,то проверяем ее конкретно так
                    {
                        var upload = webPath + WebConstant.ImagePath; // получаем новую фотку
                        var fileName = Guid.NewGuid().ToString();
                        var extension = Path.GetExtension(files[0].FileName);


                        var oldFile = Path.Combine(upload, objOld.Image);    //получаем старую фотку,проверяем на пусосту и удаляем 

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream); //вставляем новую фотку
                        }
                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objOld.Image;
                    }
                    TempData[WebConstant.Success] = "Product updated successfully";
                    _db.Update(productVM.Product);
                }
                _db.Save();
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _db.GetAllDropdownList(WebConstant.CategoryName);
            productVM.ApplicationTypeSelectList = _db.GetAllDropdownList(WebConstant.ApplicationTypeName);
            return View(productVM);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _db.FirstOrDefault(u => u.Id == id,includeProperties: "Category,ApplicationType");
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Find(id.GetValueOrDefault());
            var upload = _webHostEnvironment.WebRootPath + WebConstant.ImagePath;
            var oldFile = Path.Combine(upload, obj.Image);
            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _db.Remove(obj);
            _db.Save();
            TempData[WebConstant.Success] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
