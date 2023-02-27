using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ShopProject_Models;
using ShopProject_Models.ViewModels;
using ShopProject_Utility;
using System.Security.Claims;
using System.Text;
using Braintree;
using ShopProject_DataAccess.Data;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Utility.BrainTree;

namespace ShopProject_DataAccess.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;

        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IBrainTreeGate _brainTreeGate;
        [BindProperty] public ProductUserVM ProductUserVM { get; set; }

        public CartController(IWebHostEnvironment webHostEnvironment, IEmailSender emailSender
            , IApplicationUserRepository applicationUserRepository, IProductRepository productRepository
            , IInquiryHeaderRepository inquiryHeaderRepository, IInquiryDetailRepository inquiryDetailRepository
            , IOrderHeaderRepository orderHeaderRepository, IOrderDetailRepository orderDetailRepository
            , IBrainTreeGate brainTreeGate)
        {
            _applicationUserRepository = applicationUserRepository;
            _productRepository = productRepository;
            _inquiryHeaderRepository = inquiryHeaderRepository;
            _inquiryDetailRepository = inquiryDetailRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;

            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _brainTreeGate = brainTreeGate;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).ToList();
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productListTemp = _productRepository.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> productList = new List<Product>();
            foreach (var item in shoppingCartList)
            {
                Product productTemp = productListTemp.FirstOrDefault(u => u.Id == item.ProductId);
                productTemp.TempSqFt = item.SqFt;
                productList.Add(productTemp);
            }

            return View((List<Product>)productList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Index))]
        public IActionResult IndexPost(IEnumerable<Product> productList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (var item in productList)
            {
                shoppingCartList.Add(new ShoppingCart() { ProductId = item.Id, SqFt = item.TempSqFt });
            }

            HttpContext.Session.Set(WebConstant.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            ApplicationUser applicationUser;
            if (User.IsInRole(WebConstant.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WebConstant.SessionInquiryId) != 0)
                {
                    InquiryHeader inquiryHeader = _inquiryHeaderRepository.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WebConstant.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber,
                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }
                var gateway = _brainTreeGate.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                applicationUser = _applicationUserRepository.FirstOrDefault(u => u.Id == claim.Value);

            }

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).ToList();
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productList = _productRepository.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
            };
            foreach (var item in shoppingCartList)
            {
                Product productTemp = _productRepository.FirstOrDefault(u => u.Id == item.ProductId);
                productTemp.TempSqFt = item.SqFt;
                ProductUserVM.ProductList.Add(productTemp);
            }
            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection,ProductUserVM productUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WebConstant.AdminRole))
            {
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = productUserVM.ProductList.Sum(x => x.TempSqFt * x.Price),
                    City = productUserVM.ApplicationUser.City,
                    StreetAddress = productUserVM.ApplicationUser.StreetAddress,
                    State = productUserVM.ApplicationUser.State,
                    PostalCode = productUserVM.ApplicationUser.PostalCode,
                    FullName = productUserVM.ApplicationUser.FullName,
                    Email = productUserVM.ApplicationUser.Email,
                    PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WebConstant.StatusPending

                };
                _orderHeaderRepository.Add(orderHeader);
                _orderHeaderRepository.Save();
                foreach (var item in productUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = item.Price,
                        Sqft = item.TempSqFt,
                        ProductId = item.Id
                    };
                    _orderDetailRepository.Add(orderDetail);
                }
                _orderDetailRepository.Save();
                
                string nonceFromTheClient = collection["payment_method_nonce"];
                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = orderHeader.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };
                var gateway = _brainTreeGate.GetGateway();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if (result.Target.ProcessorResponseText == "Approved")
                {
                    orderHeader.TransactionId = result.Target.Id;
                    orderHeader.OrderStatus = WebConstant.StatusApproved;
                }
                else
                {
                    orderHeader.OrderStatus = WebConstant.StatusCancelled;
                }
                
                return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });
            }
            else
            {
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                                 "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";
                var subject = "New Inquiry";
                string htmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    htmlBody = sr.ReadToEnd();
                }

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in productUserVM.ProductList)
                {
                    productListSB.Append(
                        $" - Name: {prod.Name} <span style='font-size:14px;'>(ID: {prod.Id}) </span><br />");
                }

                string messageBody = string.Format(htmlBody,
                    productUserVM.ApplicationUser.FullName,
                    productUserVM.ApplicationUser.Email,
                    productUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString()
                );
                await _emailSender.SendEmailAsync(WebConstant.EmailAdmin, subject, messageBody);
                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = productUserVM.ApplicationUser.FullName,
                    Email = productUserVM.ApplicationUser.Email,
                    PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now
                };
                _inquiryHeaderRepository.Add(inquiryHeader);
                _inquiryHeaderRepository.Save();
                foreach (var item in productUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = item.Id
                    };
                    _inquiryDetailRepository.Add(inquiryDetail);
                }

                _inquiryDetailRepository.Save();
                TempData[WebConstant.Success] = "Inquiry submitted successfully";
            }
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(int id = 0)
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WebConstant.SessionCart).ToList();
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WebConstant.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveAll(int id)
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> productList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (var item in productList)
            {
                shoppingCartList.Add(new ShoppingCart() { ProductId = item.Id, SqFt = item.TempSqFt });
            }

            HttpContext.Session.Set(WebConstant.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}