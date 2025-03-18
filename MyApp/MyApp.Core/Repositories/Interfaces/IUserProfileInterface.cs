using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyApp.Core.Models;

namespace MyApp.Core.Repositories.Interfaces
{
    public interface IUserProfileInterface
    {
        Task<vm_UserProfile> GetOneUser(string email);
        Task<int> Update(vm_UserProfile user);

        Task<int> ResetPassword(vm_UserProfile user, string currentPassword);

    }
}