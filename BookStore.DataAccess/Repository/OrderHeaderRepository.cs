using BookStore.DataAccess.Data;
using BookStore.Models;

namespace BookStore.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext db;


        public OrderHeaderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            db = dbContext;
        }

        public void Update(OrderHeader orderHeader)
        {
            db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if(!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}