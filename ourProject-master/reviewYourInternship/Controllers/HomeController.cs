using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using reviewYourInternship.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http;
using MongoDB.Bson;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;

namespace reviewYourInternship.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        //Connect to mongoDB
        string connectionString = "mongodb+srv://iraP:XWor2IhCNsxSSOBz@cluster0.0i4a8.mongodb.net/test";
        string databaseName = "rateMyInternship";
        string collectionName = "Companies";

        HttpClient httpClient = new HttpClient();

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        public IActionResult Index()
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<Company>(collectionName);
            var list = collection.Find(new BsonDocument()).ToList();

            ViewBag.userId = _userManager.GetUserId(HttpContext.User);
            return View(list);
        }

        //GET
        public IActionResult SpecificCompany(string comp)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<Company>(collectionName);
            var filter = Builders<Company>.Filter.Eq("Name", comp);
            var company = collection.Find(filter).FirstOrDefault();

            ViewBag.userId = _userManager.GetUserId(HttpContext.User);

            var reviewsCollection = db.GetCollection<Review>("Reviews");
            var reviewsFilter = Builders<Review>.Filter.Eq("CompanyId", company.Id);
            var reviews = reviewsCollection.Find(reviewsFilter).SortByDescending(i => i.Id).ToList();
            ViewBag.reviewList = reviews;

            double averageRating = 0;
            foreach (var review in ViewBag.reviewList)
            {
                averageRating += review.Rating;
            }
            averageRating /= reviews.Count;
            ViewBag.average = String.Format("{0:0.0}", averageRating);

            //To show the company name in the navbar
            ViewBag.companyName = company.Name;

            return View((object)company);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(string name)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<Company>(collectionName);

            string error = null;
            bool e = false;

            if(name == null)
            {
                e = true;
                error = "Company must have a name!";
                return Json(new { message = error });
            }

            //Might have to change to equals or contains later
            //Also need to account for punctuation eventually
            var query = collection.AsQueryable().Where(v =>
            v.Name.ToLower().StartsWith(name)).Any();

            if (query == true)
            {
                e = true;
                error += "\nThis company already exists!";
            }

            if(!e)
            {
                var company = new Company { Name = name };
                await collection.InsertOneAsync(company);
            }

            return Json(new { message = error });
        }

        //Get
        public IActionResult AboutUs()
        {
            return View();
        }

        //Get
        public IActionResult TermsAndConditions()
        {
            return View();
        }

        //Get
        public IActionResult LoginSignup()
        {
            return View();
        }

        //Post
        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, string email, string password)
        {
                List<string> arr1 = new List<string>();
                List<string> arr2 = new List<string>();
                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = name,
                    Email = email

                };

            IdentityResult result = await _userManager.CreateAsync(appUser, password);
            if (result.Succeeded)
            {
                await _signInManager.PasswordSignInAsync(appUser, password, false, false);
                return Json(new { });
            }
            else
            {
                return Json(new { message = "Something went wrong"});
            }
        }

        //Post
        [HttpPost]
        public async Task<IActionResult> CheckUser(string name, string email, string password)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<User>("Users");
            List<string> arr1 = new List<string>();
            List<string> arr2 = new List<string>();
            ApplicationUser appUser = new ApplicationUser
            {
                UserName = name,
                Email = email

            };

            if (name == null)
            {
                arr1.Add("First Name Required");
            }

            if (email == null)
            {
                arr1.Add("Email Required");
            }
            else
            {

                //Check if email is correct. It's correct if f@f
                bool correctFormat = true;
                int numOfAtSymbols = 0;
                for (int i = 0; email != null && i < email.Length; i++)
                {
                    if (email[i] == '@' && i == 0)
                    {
                        arr1.Add("Email not formmatted correctly");
                        correctFormat = false;
                        break;
                    }
                    if (email[i] == '@' && i == email.Length - 1)
                    {
                        arr1.Add("Email not formmatted correctly");
                        correctFormat = false;
                        break;
                    }
                    if (email[i] == '@')
                    {
                        numOfAtSymbols += 1;
                    }
                }

                //email.Contains('@');

                if (numOfAtSymbols != 1 && correctFormat)
                {
                    arr1.Add("Email not formmatted correctly");
                }

            }

            if (password == null)
            {
                arr1.Add("Password Required");
                return Json(new { message1 = arr1, message2 = arr2 });
            }

            if (arr1.Count == 0)
            {
                IdentityResult result = await _userManager.CreateAsync(appUser, password);
                if (result.Succeeded)
                {
                    //Delete user because they haven't agreed to terms and conditions
                    var deleteFilter = Builders<User>.Filter.Eq("UserName", name);
                    collection.DeleteOne(deleteFilter);
                    return Json(new { });
                }
                else
                {
                    return Json(new { message1 = arr1, message2 = result.Errors });
                }
            }
            else
            {
                return Json(new { message1 = arr1, message2 = arr2 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser()
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<User>("Users");
            await _signInManager.SignOutAsync();
            var deleteFilter = Builders<User>.Filter.Eq("UserName", User.Identity.Name);
            collection.DeleteOne(deleteFilter);
            return Json(new { });
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
                if(email == null || password == null)
                {
                    return Json(new { message1 = "Both email and password are required to login." });
                }
                ApplicationUser appUser = await _userManager.FindByEmailAsync(email);
                if (appUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, password, false, false);
                    if (result.Succeeded)
                    {
                        return Json(new {});
                    }
                    else
                    {
                        return Json(new { message1 = "Login failed. Password is invalid." });
                    }
                }
                else
                {
                    return Json(new { message1 = "Login failed. Could not find user." });
                }

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SendUserId(string email)
        {
            if(email == null)
            {
                return Json(new { message1 = "Email field cannot be empty." });
            }
            ApplicationUser appUser = await _userManager.FindByEmailAsync(email);
            if(appUser == null)
            {
                return Json(new { message1 = "Email doesn't exist in our database." });
            }

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ira.porchia@pepperdine.edu", "yJ9siO#47"),
                EnableSsl = true,
            };

            string body = "This is your id. Copy and paste into the text box if you want to change your password: \n";
            body += appUser.Id;
            
            smtpClient.Send("ira.porchia@pepperdine.edu", email, "Forgot Password", body);

            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string id, string newPassword, string newConfirmedPassword)
        {
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<User>("Users");

            if (newPassword == null)
            {
                return Json(new { message1 = "You must have a password!" });
            }

            if (newPassword != newConfirmedPassword)
            {
                return Json(new { message1 = "Passwords do not match!" });
            }
            
            ApplicationUser appUser = await _userManager.FindByIdAsync(id);

            if(appUser == null)
            {
                return Json(new { message1 = "Id is not correct!" });
            }

            ApplicationUser newAppUser = new ApplicationUser
            {
                Id = appUser.Id,
                UserName = appUser.UserName,
                Email = appUser.Email
            };

            //delete old one
            var deleteFilter = Builders<User>.Filter.Eq("UserName", appUser.UserName);
            collection.DeleteOne(deleteFilter);

            //create new one
            IdentityResult result = await _userManager.CreateAsync(newAppUser, newPassword);

            if (result.Succeeded)
            {
                await _signInManager.PasswordSignInAsync(newAppUser, newPassword, false, false);
                return Json(new { });
            }
            else
            {
                //Need to create a new user regardless with temporary password
                await _userManager.CreateAsync(appUser, "TemporaryPassword#123");

                var str = "";
                foreach(var error in result.Errors)
                {
                    str += error.Description;
                    str += "\n";
                }
                return Json(new { message1 = str });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
