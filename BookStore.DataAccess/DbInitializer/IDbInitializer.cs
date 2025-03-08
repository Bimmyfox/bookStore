using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace BookStore.DataAccess.DbInitializer
{
    public interface IDbInitializer 
    {
        void Initialize();
    }
}