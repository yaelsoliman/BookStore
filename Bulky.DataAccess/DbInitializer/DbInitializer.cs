using Bulky.Models;
using Bulky.Utility;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
               if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

            }catch (Exception ex) { }

            //Create roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();



                //if roles are not created,then we will create admin user as well
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@aspnet.com",
                    Email = "admin@aspnet.com",
                    Name = "Yael Soliman",
                    PhoneNumber = "0998644002",
                    StreetAddress = "B4 Street",
                    State = "IL",
                    PostalCode = "24324",
                    City = "Damascus"
                }, "Admin123*").GetAwaiter().GetResult();
                ApplicationUser user=_db.ApplicationUsers.FirstOrDefault(u=>u.Email== "admin@aspnet.com");
                _userManager.AddToRoleAsync(user,SD.Role_Admin).GetAwaiter().GetResult() ;
            }
            return;


        }
    }
}
