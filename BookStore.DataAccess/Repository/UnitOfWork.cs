using BookStore.DataAccess.Data;

namespace BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public IApplicationUserRepository ApplicationUserRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
        public IShoppingCartRepository ShoppingCartRepository { get; private set; }
        public IOrderDetailRepository OrderDetailRepository { get; private set; }
        public IOrderHeaderRepository OrderHeaderRepository { get; private set; }

        
        private ApplicationDbContext db;


        public UnitOfWork(ApplicationDbContext db)
        {
            this.db = db;
            ApplicationUserRepository = new ApplicationUserRepository(db);
            CategoryRepository = new CategoryRepository(db);
            ProductRepository = new ProductRepository(db);
            CompanyRepository = new CompanyRepository(db);
            ShoppingCartRepository = new ShoppingCartRepository(db);
            OrderDetailRepository = new OrderDetailRepository(db);
            OrderHeaderRepository = new OrderHeaderRepository(db);
        }
        
        public void Save()
        {
            db.SaveChanges();
        }
    }
}