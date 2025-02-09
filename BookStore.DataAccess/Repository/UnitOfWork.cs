using BookStore.DataAccess.Data;

namespace BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
        private ApplicationDbContext db;


        public UnitOfWork(ApplicationDbContext db)
        {
            this.db = db;
            CategoryRepository = new CategoryRepository(db);
            ProductRepository = new ProductRepository(db);
            CompanyRepository = new CompanyRepository(db);
        }
        
        public void Save()
        {
            db.SaveChanges();
        }
    }
}