using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Model;

namespace MyApp.MVC.Models
{
    public interface IAdminInterface
    {
        List<User> GetAllUsers();

    }
}