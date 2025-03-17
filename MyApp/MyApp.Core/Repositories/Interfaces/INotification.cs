using MyApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Core.Repositories;

public interface INotification
{
    Task CreateNotification(Notification notification);
    Task<List<Notification>> GetNotificationsByUserId(int userId);
    Task<int> GetUnreadNotificationCount(int userId);
    Task MarkAsRead(int notificationId);
}

