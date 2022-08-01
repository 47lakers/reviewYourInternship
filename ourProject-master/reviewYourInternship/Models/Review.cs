using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace reviewYourInternship.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Company { get; set; }

        public int Rating { get; set; }

        public string Comments { get; set; }

        public string Position { get; set; }

        public string City { get; set; }

        public string Location { get; set; }

        public string State { get; set; }

        public double Salary { get; set; }

        public string LengthOfTime { get; set; }

        public string CompanyId { get; set; }

        public string UserId { get; set; }
    }
}
