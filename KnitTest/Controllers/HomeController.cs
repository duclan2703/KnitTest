using OTPModule.Constants;
using OTPModule.Entities;
using OTPModule.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using OTPModule.Utils;

namespace OTPModule.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOTPService _otpService;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IOTPService otpService, ILogger<HomeController> logger)
        {
            _otpService = otpService;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
