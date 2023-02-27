using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopProject_Models;
using ShopProject_Models.ViewModels;
using System.Diagnostics;
using ShopProject_DataAccess.Data;
using ShopProject_Utility;
using ShopProject_DataAccess.Repository;
using ShopProject_DataAccess.Repository.IRepository;

namespace ShopProject_DataAccess.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IProductRepository _productRepo;
		private readonly ICategoryRepository _categoryRepo;

		public HomeController(ILogger<HomeController> logger, IProductRepository productRepository, ICategoryRepository categoryRepository)
		{
			_logger = logger;
			_productRepo = productRepository;
			_categoryRepo = categoryRepository;
		}

		public IActionResult Index()
		{
			HomeVM homeVm = new HomeVM()
			{
				Products = _productRepo.GetAll(includeProperties: "Category,ApplicationType"),
				Categories = _categoryRepo.GetAll()
			};
			return View(homeVm);
		}

		[HttpGet]
		public IActionResult Details(int id)
		{
			List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
			if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart) != null
			    && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).Count() > 0)
			{
				shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstant.SessionCart);
			}
			
			
			DetailsVM detailsVm = new DetailsVM()
			{
                Product = _productRepo.FirstOrDefault(u => u.Id == id,includeProperties: "Category,ApplicationType"),
				ExistInCart = false
			};

			foreach (var item in shoppingCartList)
			{
				if (item.ProductId == id)
				{
					detailsVm.ExistInCart = true;
				}
			}
			
			return View(detailsVm);
		}
		[HttpPost, ActionName("Details")]
		public IActionResult DetailsPost(int id, DetailsVM detailsVm)
		{
			List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
			if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart) != null
			    && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).Count() > 0)
			{
				shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstant.SessionCart);
			}
			shoppingCartList.Add(new ShoppingCart{ProductId = id, SqFt = detailsVm.Product.TempSqFt});
			HttpContext.Session.Set(WebConstant.SessionCart, shoppingCartList);
			return RedirectToAction(nameof(Index));
		}
		public IActionResult RemoveFromCart(int id)
		{
			List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
			if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart) != null
			    && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).Count() > 0)
			{
				shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstant.SessionCart);
			}

			var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
			if (shoppingCartList != null)
			{
				shoppingCartList.Remove(itemToRemove);
			}
			HttpContext.Session.Set(WebConstant.SessionCart, shoppingCartList);
			return RedirectToAction(nameof(Index));
		}
		
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}