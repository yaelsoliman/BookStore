using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public UserController(IUnitOfWork unitOfWork,ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }
        public IActionResult Index()
        {
           return View();
        }
       
        public IActionResult RoleManagement(string userId)
        {
            string RoleID=_db.UserRoles.FirstOrDefault(u=>u.UserId==userId).RoleId;
            RoleManagementVM RoleVM = new()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").FirstOrDefault(u => u.Id == userId),
                RoleList=_db.Roles.Select(i=>new SelectListItem
                {
                    Text = i.Name,
                    Value= i.Name
                }),
                CompanyList=_unitOfWork.Company.GetAll().Select(i=>new SelectListItem
                {
                    Text = i.Name,
                    Value= i.Id.ToString(),
                })
            };
            RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
            return View(RoleVM);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();
            var userRoles=_db.UserRoles.ToList();
            var roles=_db.Roles.ToList();

            foreach (var user in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }
            
            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/UnLocking" });
            }
            if(objFromDb.LockoutEnd!=null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd=DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = false, message = " Operation Successfuly" });

        }
        //[HttpGet]
        //public async Task<IActionResult> LockUnLocks(string? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    var user = await _db.ApplicationUsers.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
        //    {
        //        user.LockoutEnd = DateTime.Now.AddYears(1000);
        //    }
        //    else
        //    {
        //        user.LockoutEnd = DateTime.Now;
        //    }
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
    }
}
