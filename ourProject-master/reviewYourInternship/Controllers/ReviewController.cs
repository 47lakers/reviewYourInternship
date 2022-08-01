using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using reviewYourInternship.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace reviewYourInternship.Controllers
{
    public class ReviewController : Controller
    {
        //Connect to mongoDB
        string connectionString = "mongodb+srv://iraP:XWor2IhCNsxSSOBz@cluster0.0i4a8.mongodb.net/test";
        string databaseName = "rateMyInternship";
        string collectionName = "Reviews";

        string[] states = { "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA", "WA", "WV", "WI", "WY" };

        public IActionResult CheckReview([Required] string company, [Required] string position, string salary, string lengthOfTime, string city, string state, [Required] int rating, string comments, string userId, [Required] string companyId)
        {
            string error = null;
            if (company == null || companyId == null)
            {
                error = "Review must have a company! ";
                return Json(new { message = error });
            }

            if (rating > 5 || rating < 1)
            {
                error += "Review must have a rating! ";
                return Json(new { message = error });
            }

            if (position == null)
            {
                error += "Review must have a position! ";
                return Json(new { message = error });
            }

            if (salary == null)
            {
                error += "Pay cannot be empty!";
                return Json(new { message = error });
            }

            var isNumeric = double.TryParse(salary, out double n);
            if (!isNumeric)
            {
                error += "Pay must be numeric!";
                return Json(new { message = error });
            }

            if (lengthOfTime == null)
            {
                error += "Months worked cannot be empty!";
                return Json(new { message = error });
            }

            var isMonthNumeric = int.TryParse(lengthOfTime, out int x);
            if (!isMonthNumeric)
            {
                error += "Months worked must be numeric!";
                return Json(new { message = error });
            }

            if (state != null && !states.Contains(state.ToUpper()))
            {
                error += "Not a valid state!";
                return Json(new { message = error });
            }

            return Json(new { message = error });
        }

        // Post
        public async Task<IActionResult> CreateReview([Required] string company, [Required] string position, string salary, string lengthOfTime, string city, string state, [Required] int rating, string comments, string userId, [Required] string companyId)
        {
            var isNumeric = double.TryParse(salary, out double n);
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);
            var collection = db.GetCollection<Review>(collectionName);
            var review = new Review { Company = company, Salary = n, Position = position, Rating = rating, Comments = comments, LengthOfTime = lengthOfTime, CompanyId = companyId, City = city, State = state, UserId = userId, };
            await collection.InsertOneAsync(review);
            return Json(new {});
        }

        // Post
        public async Task<IActionResult> EditReview(string id, [Required] string company, [Required] string position, string salary, string lengthOfTime, [Required] int rating, string comments, string userId, [Required] string companyId, string city, string state)
        {
            string error = null;
            bool e = false;
            if (company == null || companyId == null)
            {
                e = true;
                error = "Review must have a company! ";
                return Json(new { message = error });
            }

            if (position == null)
            {
                e = true;
                error += "Review must have a position! ";
                return Json(new { message = error });
            }

            if (rating > 5 || rating < 1)
            {
                e = true;
                error += "Review must have a rating! ";
                return Json(new { message = error });
            }

            if (salary == null)
            {
                e = true;
                error += "Pay cannot be empty!";
                return Json(new { message = error });
            }

            var isNumeric = double.TryParse(salary, out double n);
            if (!isNumeric)
            {
                e = true;
                error += "Pay must be numeric!";
                return Json(new { message = error });
            }

            if (lengthOfTime == null)
            {
                e = true;
                error += "Months worked cannot be empty!";
                return Json(new { message = error });
            }

            var isMonthNumeric = int.TryParse(lengthOfTime, out int x);
            if (!isMonthNumeric)
            {
                e = true;
                error += "Months worked must be numeric!";
                return Json(new { message = error });
            }

            if (state != null && !states.Contains(state.ToUpper()))
            {
                e = true;
                error += "Not a valid state!";
                return Json(new { message = error });
            }

            if (!e)
            {
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(databaseName);
                var collection = db.GetCollection<Review>(collectionName);
                var filter = Builders<Review>.Filter.Eq(u => u.Id, id);
                await collection.DeleteOneAsync(filter);
                var review = new Review { Company = company, Salary = n, Position = position, Rating = rating, Comments = comments, LengthOfTime = lengthOfTime, CompanyId = companyId, City = city, State = state, UserId = userId };
                await collection.InsertOneAsync(review);
                //await collection.ReplaceOneAsync(filter, review, new ReplaceOptions { IsUpsert = true });
            }

            return Json(new { message = error});
        }
    }
}
