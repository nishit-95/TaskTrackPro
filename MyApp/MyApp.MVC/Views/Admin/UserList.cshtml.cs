using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MyApp.MVC.Views.Admin
{
    public class UserList : PageModel
    {
        private readonly ILogger<UserList> _logger;

        public UserList(ILogger<UserList> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}