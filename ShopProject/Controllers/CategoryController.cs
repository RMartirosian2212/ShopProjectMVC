using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShopProject_Models;
using ShopProject_Utility;
using System.Data;
using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;

namespace ShopProject_DataAccess.Controllers
{
    [Authorize(Roles = WebConstant.AdminRole)]
    public class CategoryController : Controller
	{
		public readonly ICategoryRepository _db;
		public CategoryController(ICategoryRepository db)
		{
			_db = db;
		}
		public IActionResult Index()
		{
			IEnumerable<Category> categories = _db.GetAll();
			return View(categories);
		}
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Category category)
		{
			if (ModelState.IsValid)
			{
                _db.Add(category);
                _db.Save();
                TempData[WebConstant.Success] = "Category created successfully";
                return RedirectToAction("Index");
            }
			TempData[WebConstant.Error] = "Error while creating category";
			return View();
		}
		[HttpGet]
		public IActionResult Edit(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Category? category = _db.Find(id.GetValueOrDefault());
			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}
		[HttpPost]
		public IActionResult Edit(Category category)
		{
			if (ModelState.IsValid)
			{
                _db.Update(category);
                _db.Save();
                TempData[WebConstant.Success] = "Category edited successfully";
                return RedirectToAction("Index");
            }
			TempData[WebConstant.Error] = "Error while editing category";
			return View();
		}
		public IActionResult Delete(int? id)
		{
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? category = _db.Find(id.GetValueOrDefault());
			if (category == null)
			{
				return NotFound();
			}
			return View(category);
		}
		[HttpPost]
		public IActionResult Delete(Category category)
		{
			if (category == null)
			{
				TempData[WebConstant.Error] = "Error while deleting category";
				return NotFound();
			}
			_db.Remove(category);
			_db.Save();
			TempData[WebConstant.Success] = "Category deleted successfully";
			return RedirectToAction("Index");
		}
	}
}
