using BookStore.DataAccess.Repository;
using BookStore.Models;
using BookStore.Utility;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Stripe;

namespace BookStore.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new ()
            {
                OrderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = unitOfWork.OrderDetailRepository.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult UpdateOrderDetails()
        {
            var dbOrderDetail = unitOfWork.OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);
            dbOrderDetail.Name = OrderVM.OrderHeader.Name;
            dbOrderDetail.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            dbOrderDetail.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            dbOrderDetail.City = OrderVM.OrderHeader.City;
            dbOrderDetail.State = OrderVM.OrderHeader.State;
            dbOrderDetail.PostalCode = OrderVM.OrderHeader.PostalCode;

            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                dbOrderDetail.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                dbOrderDetail.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            unitOfWork.OrderHeaderRepository.Update(dbOrderDetail);
            unitOfWork.Save();


            TempData["Success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new {orderId = dbOrderDetail.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, SD.STATUS_IN_PROGRESS);
            unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new {orderId = OrderVM.OrderHeader.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult ShipOrder()
        {
            var orderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.STATUS_SHIPPED;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus == SD.PAYMENT_STATUS_DELAYED_PAYMENT)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

            }

            unitOfWork.OrderHeaderRepository.Update(orderHeader);
            unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";

            return RedirectToAction(nameof(Details), new {orderId = OrderVM.OrderHeader.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult CancelOrder()
        {
            var orderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if(orderHeader.PaymentStatus == SD.PAYMENT_STATUS_APPROVED)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.STATUS_CANCELLED, SD.STATUS_REFUNDED);
            }
            else
            {
                unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.STATUS_CANCELLED, SD.STATUS_CANCELLED);
            }
            unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";

            return RedirectToAction(nameof(Details), new {orderId = OrderVM.OrderHeader.Id});
        }

        [HttpPost]
        [ActionName("Details")]
        public IActionResult DetailsPayNow()
        {

            OrderVM.OrderHeader = unitOfWork.OrderHeaderRepository
                .Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = unitOfWork.OrderDetailRepository
                .GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach(var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == orderHeaderId);
            if(orderHeader.PaymentStatus == SD.PAYMENT_STATUS_DELAYED_PAYMENT)
            {
                var service = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PAYMENT_STATUS_APPROVED);
                    unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        #region APICalls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;
            if(User.IsInRole(SD.ROLE_ADMIN) || User.IsInRole(SD.ROLE_EMPLOYEE))
            {
                objOrderHeaders = unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = unitOfWork.OrderHeaderRepository.
                                    GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PAYMENT_STATUS_DELAYED_PAYMENT);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.STATUS_IN_PROGRESS);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.STATUS_SHIPPED);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.STATUS_APPROVED);
                    break;
                default:
                    objOrderHeaders = unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
                    break;
            }

            return Json(new {data = objOrderHeaders });
        }
        #endregion
   }
}