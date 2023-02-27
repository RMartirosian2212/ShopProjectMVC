using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Models;
using ShopProject_Utility;

namespace ShopProject_DataAccess.Controllers
{
    [Authorize(Roles = WebConstant.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        private readonly IApplicationTypeRepository _db;
        public ApplicationTypeController(IApplicationTypeRepository db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<ApplicationType> objList = _db.GetAll(); 
            return View(objList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType applicationType)
        {
            if (ModelState.IsValid)
            {
                _db.Add(applicationType);
                _db.Save();
                TempData[WebConstant.Success] = "Application type created successfully";
                return RedirectToAction("Index");
            }
            TempData[WebConstant.Error] = "Error while creating application type";
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ApplicationType? applicationType = _db.Find(id.GetValueOrDefault());
            if (applicationType == null)
            {
                return NotFound();
            }
            return View(applicationType);
        }
        [HttpPost]
        public IActionResult Edit(ApplicationType applicationType)
        {
            if (ModelState.IsValid)
            {
                _db.Update(applicationType);
                _db.Save();
                TempData[WebConstant.Success] = "Application type edited successfully";
                return RedirectToAction("Index");
            }
            TempData[WebConstant.Error] = "Error while editing application type";
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ApplicationType? applicationType = _db.Find(id.GetValueOrDefault());
            if (applicationType == null)
            {
                return NotFound();
            }
            return View(applicationType);
        }
        [HttpPost]
        public IActionResult Delete(ApplicationType applicationType)
        {
            if (applicationType == null)
            {
                TempData[WebConstant.Error] = "Error while deleting application type";
                return NotFound();
            }
            _db.Remove(applicationType);
            _db.Save();
            TempData[WebConstant.Success] = "Application type deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
