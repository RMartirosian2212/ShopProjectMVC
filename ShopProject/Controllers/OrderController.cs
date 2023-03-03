using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ShopProject_DataAccess.Repository.IRepository;
using ShopProject_Models;
using ShopProject_Models.ViewModels;
using ShopProject_Utility;
using ShopProject_Utility.BrainTree;

namespace ShopProject.Controllers
{
    [Authorize(Roles = WebConstant.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        [BindProperty] public OrderVM orderVM { get; set; }

        private readonly IBrainTreeGate _brainTreeGate;

        public OrderController(IOrderHeaderRepository orderHeaderRepository
            , IOrderDetailRepository orderDetailRepository
            , IBrainTreeGate brainTreeGate)
        {
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;

            _brainTreeGate = brainTreeGate;
        }

        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null,
            string status = null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHeaderList = _orderHeaderRepository.GetAll(),
                StatusList = WebConstant.listStatus.Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = i,
                    Value = i
                }),
            };
            if (string.IsNullOrEmpty(searchName))
            {
                orderListVM.OrderHeaderList =
                    orderListVM.OrderHeaderList.Where(u => u.FullName.ToLower() == searchName);
            }

            if (string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.Email.ToLower() == searchEmail);
            }

            if (string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHeaderList =
                    orderListVM.OrderHeaderList.Where(u => u.PhoneNumber.ToLower() == searchPhone);
            }

            if (string.IsNullOrEmpty(status) && status != "--Order Status--")
            {
                orderListVM.OrderHeaderList =
                    orderListVM.OrderHeaderList.Where(u => u.FullName.ToLower() == searchName);
            }

            return View(orderListVM);
        }

        public IActionResult Details(int id)
        {
            orderVM = new OrderVM()
            {
                OrderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == id),
                OrderDetails = _orderDetailRepository.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };
            return View(orderVM);
        }

        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WebConstant.StatusInProcess;
            _orderHeaderRepository.Save();
            TempData[WebConstant.Success] = "Order Is In Process";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WebConstant.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            _orderHeaderRepository.Save();
            TempData[WebConstant.Success] = "Order Shipped Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);

            var gateway = _brainTreeGate.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);
            if (transaction.Status == TransactionStatus.AUTHORIZED ||
                transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                Result<Transaction> resultVoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);
            }

            orderHeader.OrderStatus = WebConstant.StatusRefunded;
            _orderHeaderRepository.Save();
            TempData[WebConstant.Success] = "Order Canceled Successfully";
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            OrderHeader orderHeaderFromDb = _orderHeaderRepository.FirstOrDefault(u => u.Id == orderVM.OrderHeader.Id);
            orderHeaderFromDb.FullName = orderVM.OrderHeader.FullName;
            orderHeaderFromDb.Email = orderVM.OrderHeader.Email;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            _orderHeaderRepository.Save();
            TempData[WebConstant.Success] = "Order Details Updated Successfully";
            
            return RedirectToAction(nameof(Details), "OrderController", new {id = orderHeaderFromDb.Id});
        }
    }
}