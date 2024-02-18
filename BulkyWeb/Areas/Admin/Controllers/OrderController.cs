using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            //var order = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                
                 OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail=_unitOfWork.OrderDetail.GetAll(u=>u.OrderHeader.Id==orderId,includeProperties:"Product")
            };
            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Customer)]
        public IActionResult UpdateOrderDetails() {

            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.Phonenumber = OrderVM.OrderHeader.Phonenumber;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Customer)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Customer)]
        public IActionResult ShipOrder()
        {
            var orderHeader=_unitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber= OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier=OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShoppingDate= DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["success"] = "Order Status Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Customer)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason=RefundReasons.RequestedByCustomer,
                    PaymentIntent=orderHeader.PaymentIntentId
                };
                var service=new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);

            }
            _unitOfWork.Save();

            TempData["success"] = "Order Status Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }


        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders= _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId== userId,includeProperties: "ApplicationUser").ToList();
            }
            //switch (status)
            //{
            //    case "pending":
            //        orderHeaders = orderHeaders.Where(u=>u.PaymentStatus==SD.PaymentStatusDelayedPayment);
            //        break;
            //    case "inprocess":
            //        orderHeaders = orderHeaders.Where(x=>x.OrderStatus==SD.StatusInProcess);
            //        break;
            //    case "completed":
            //        orderHeaders = orderHeaders.Where(i=>i.OrderStatus==SD.StatusShipped);
            //        break;
            //    case "approved":
            //        orderHeaders = orderHeaders.Where(o=>o.OrderStatus==SD.StatusApproved);
            //        break;
            //    default:
            //        break;
            //}


            return Json(new { data = orderHeaders });
        }

    }
}
