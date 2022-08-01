using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using reviewYourInternship.Models;

namespace reviewYourInternship.Controllers
{
    [Authorize]
    public class SecuredController : Controller
    {
        //For roles, uncomment this and uncomment
        //the comment in UserController Create method

        //[Authorize(Roles ="Admin")]

        string connectionString = "mongodb+srv://iraP:XWor2IhCNsxSSOBz@cluster0.0i4a8.mongodb.net/test";
        string databaseName = "rateMyInternship";
        string collectionName = "Reviews";

        private UserManager<ApplicationUser> _userManager;

        public SecuredController(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }

        public IActionResult Index()
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<Review>(collectionName);
            var filter = Builders<Review>.Filter.Eq("UserId", _userManager.GetUserId(HttpContext.User));
            var reviews = collection.Find(filter).ToList();
            ViewBag.reviewList = reviews;
            ViewBag.userId = _userManager.GetUserId(HttpContext.User);
            return View();
        }
    }
}
