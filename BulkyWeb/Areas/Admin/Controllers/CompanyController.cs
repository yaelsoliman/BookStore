using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var company = _unitOfWork.Company.GetAll();
            return View(company);
        }
        [HttpGet]
        public IActionResult Upsert(int? id) {

            if (id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company Created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }

        }
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }
        [HttpDelete]
        public IActionResult DeleteApi(int id)
        {
            var companyTobeDeleted=_unitOfWork.Company.Get(u=>u.Id==id);
            if(companyTobeDeleted == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });

            }
            _unitOfWork.Company.Remove(companyTobeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true,message = "Delete Successfully" });

        }
    }
}
