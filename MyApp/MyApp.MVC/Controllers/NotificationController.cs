using Microsoft.AspNetCore.Mvc;
using MyApp.Core.Models;
using MyApp.Core.Repositories;

using System.Threading.Tasks;
using MyApp.Core.Services;

namespace MyApp.MVC.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotification _notificationRepository;
        private readonly RedisService _redisService;
        private readonly RabbitMQService _rabbitMQService;

        public NotificationController(INotification notificationRepository, RedisService redisService, RabbitMQService rabbitMQService)
        {
            _notificationRepository = notificationRepository;
            _redisService = redisService;
            _rabbitMQService = rabbitMQService;

            _rabbitMQService.ConsumeAdminMessages(message => Console.WriteLine($"Received Admin Notification: {message}"));
        }

        public async Task<IActionResult> Index()
        {
            var adminId = GetCurrentAdminId();
            var notifications = await _notificationRepository.GetNotificationsByUserId(adminId);
            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var adminId = GetCurrentAdminId();
            var count = _redisService.GetAdminUnreadNotificationCount(adminId);
            if (count == 0)
            {
                count = await _notificationRepository.GetUnreadNotificationCount(adminId);
                _redisService.SetAdminUnreadNotificationCount(adminId, count);
            }
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {

            await _notificationRepository.MarkAsRead(notificationId);
            var adminId = GetCurrentAdminId();
            var unreadCount = await _notificationRepository.GetUnreadNotificationCount(adminId);
            _redisService.SetAdminUnreadNotificationCount(adminId, unreadCount);
            return Json(new { success = true });
        }

        public async Task CreateNotification(int adminId, string title, int? taskId = null)
        {
            var notification = new Notification { c_title = title, c_taskid = taskId, c_userid = adminId, c_isread = false };
            await _notificationRepository.CreateNotification(notification);
            var unreadCount = await _notificationRepository.GetUnreadNotificationCount(adminId);
            _redisService.SetAdminUnreadNotificationCount(adminId, unreadCount);
            _redisService.PublishAdminNotification(adminId, title);
            _rabbitMQService.PublishAdminMessage($"Notification for Admin {adminId}: {title}");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private int GetCurrentAdminId() => 2;
    }
}