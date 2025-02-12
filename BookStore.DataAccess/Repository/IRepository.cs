using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BookStore.DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        //T - Category
        IEnumerable<T> GetAll(string? includeProperties = null);
        // T.GetFirstOrDefault(u => u.Id == id);
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false); 
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
    }
}