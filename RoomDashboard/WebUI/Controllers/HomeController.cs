using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Shared;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly RoomRepository _roomRepository;

        public HomeController()
        {
            var connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            _roomRepository = new RoomRepository(connectionString);
        }

        public async Task<ActionResult> Index()
        {
            var model = new IndexViewModel
            {
                Rooms = (await _roomRepository.GetRoomsAsync())
                    .Select(r => new RoomViewModel
                    {
                        RoomId = r.RoomId,
                        RoomName = r.RoomName
                    })
                    .ToList()
            };

            return View(model);
        }

        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _roomRepository.AddRoomAsync(
                    model.RoomName,
                    model.StorageQueueConnectionString,
                    model.StorageQueueName);

                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
    }
}