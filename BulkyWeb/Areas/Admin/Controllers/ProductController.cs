
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public ProductVM productVM { get; set; }
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

        }
        public IActionResult Index()
        {
            var product = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            //var x=context.Products.Include(m=>m.Category).ToList();

            return View(product);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
            }


            //ViewBag.CategoryList = CategoryList;


            return View(productVM);
        }
        [HttpPost]
        public IActionResult Upsert()
        {

            if (ModelState.IsValid)
            {
                //string wwwRootPath = _webHostEnvironment.WebRootPath;
                //if(file != null)
                //{
                //    string fileName=Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                //    string productPath=Path.Combine(wwwRootPath, @"images\product");
                //    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                //    {
                //        file.CopyTo(fileStream);
                //    }
                //    productVM.Product.ImageUrl = @"\images\product" + fileName;
                //}

                if (productVM.Product.Id == 0)
                {
                    string ImagePath = @"\images\Untitled.jpg.jpg";
                    var files = HttpContext.Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string WebRootPath = _webHostEnvironment.WebRootPath;
                        string ImageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);
                        FileStream fileStream = new FileStream(Path.Combine(WebRootPath, "images", ImageName), FileMode.Create);
                        files[0].CopyTo(fileStream);

                        ImagePath = @"\images\" + ImageName;
                    }
                    productVM.Product.ImageUrl = ImagePath;
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    var files = HttpContext.Request.Form.Files;
                    if (files.Count > 0)
                    {


                        string WebRootPath = _webHostEnvironment.WebRootPath;
                        string ImageName = DateTime.Now.ToFileTime().ToString() + Path.GetExtension(files[0].FileName);
                        //var oldpath = Path.Combine(WebRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        //if (System.IO.File.Exists(oldpath))
                        //{
                        //    System.IO.File.Delete(oldpath);
                        //}
                        FileStream fileStream = new FileStream(Path.Combine(WebRootPath, "Images", ImageName), FileMode.Create);
                        files[0].CopyTo(fileStream);

                        string ImagePath = @"\Images\" + ImageName;
                        productVM.Product.ImageUrl = ImagePath;


                    }

                    _unitOfWork.Product.Update(productVM.Product);

                }
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select
                    (u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString(),
                    });
                return View(productVM);

            }
        }

        //public IActionResult Edit(int? id)
        //{
        //    if(id== null || id==0)
        //        return NotFound();

        //    ProductVM productVM = new()
        //    {
        //        CategoryList = _unitOfWork.Category.GetAll()
        //        .Select(u => new SelectListItem
        //        {
        //            Text = u.Name,
        //            Value = u.Id.ToString(),
        //        }),
        //        Product = _unitOfWork.Product.Get(u => u.Id==id)

        //    };
        //    return View(productVM);


        //}
        //[HttpPost]
        //public IActionResult Edit(ProductVM productVM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(productVM.Product);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product Update Successfully";

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        productVM.CategoryList = _unitOfWork.Category.GetAll().Select
        //            (u => new SelectListItem
        //            {
        //                Text = u.Name,
        //                Value = u.Id.ToString(),
        //            });
        //        return View(productVM);

        //    }

        //}
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var product = _unitOfWork.Product.Get(u => u.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product Deleted Successfully";
            return RedirectToAction("Index");

        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var product = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = product });
        }

       
        public IActionResult DeleteApi(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new {success=false,message="Error while Deleting"});
            }

            var oldpath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldpath))
            {
                System.IO.File.Delete(oldpath);
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted Successfully" });
        }
    }
}
