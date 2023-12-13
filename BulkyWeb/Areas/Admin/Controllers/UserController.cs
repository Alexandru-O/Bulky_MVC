using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM roleManagementVM = new()
            {
                ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId),
                CompanyList = _db.Companies.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
                RoleList = _db.Roles.Select(u => new SelectListItem 
                { 
                    Text = u.Name, Value = u.Id 
                })
            };
            return View(roleManagementVM);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() 
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in objUserList) 
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

                if(user.Company == null)
                {
                    user.Company = new()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new {data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new {success = false, message = "Error while Locking/Unlocking"});
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked and we need to unclock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
