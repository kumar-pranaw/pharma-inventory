using System.Linq;
using System.Web.Mvc;
using Pharmacy.ViewModels;

namespace Pharmacy.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel loginModel)
        {
            bool check = (from admin in BaseClass.dbEntities.Auths
                         where admin.Username == loginModel.Username && 
                         admin.Password == loginModel.Password select admin).Any();
            if(check)
            {
                var adminDetails = "Raman";
                HttpContext.Session["Admin"] = adminDetails;
                return RedirectToAction("Home","Dashboard");
            } 
            else
            {
                ViewBag.failed = "Invalid username/password";
                return View();
            }

        }
    }
}