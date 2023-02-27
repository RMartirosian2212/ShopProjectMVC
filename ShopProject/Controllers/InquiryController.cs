using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Models;
using ShopProject_Models.ViewModels;
using ShopProject_Utility;

namespace ShopProject_DataAccess.Controllers
{
    [Authorize( Roles = WebConstant.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;
        [BindProperty]
        public InquiryVM InquiryVm { get; set; }

        public InquiryController(IInquiryHeaderRepository inquiryHeaderRepository,
            IInquiryDetailRepository inquiryDetailRepository)
        {
            _inquiryHeaderRepository = inquiryHeaderRepository;
            _inquiryDetailRepository = inquiryDetailRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            InquiryVm = new InquiryVM()
            {
                InquiryHeader = _inquiryHeaderRepository.FirstOrDefault(u => u.Id == id),
                InquiryDetails =
                    _inquiryDetailRepository.GetAll(u => u.InquiryHeaderId == id, includeProperties: "Product")
            };
            return View(InquiryVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            InquiryVm.InquiryDetails =
                _inquiryDetailRepository.GetAll(u => u.InquiryHeaderId == InquiryVm.InquiryHeader.Id);
            foreach (var item in InquiryVm.InquiryDetails)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = item.ProductId
                };
                shoppingCartList.Add(shoppingCart);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.Set(WebConstant.SessionCart, shoppingCartList);
            HttpContext.Session.Set(WebConstant.SessionInquiryId,InquiryVm.InquiryHeader.Id);
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader =
                _inquiryHeaderRepository.FirstOrDefault(u => u.Id == InquiryVm.InquiryHeader.Id);
            IEnumerable<InquiryDetail> inquiryDetails =
                _inquiryDetailRepository.GetAll(u => u.InquiryHeaderId == InquiryVm.InquiryHeader.Id);
            _inquiryDetailRepository.RemoveRange(inquiryDetails);
            _inquiryHeaderRepository.Remove(inquiryHeader);
            _inquiryHeaderRepository.Save();
            TempData[WebConstant.Success] = "Inquiry deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetInquiryList()
        {
            return Json(new { Data = _inquiryHeaderRepository.GetAll() });
        }

        #endregion
    }
}