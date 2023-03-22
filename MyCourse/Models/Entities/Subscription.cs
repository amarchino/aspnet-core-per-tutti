using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Entities
{
    public class Subscription
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; }
        public Money Paid { get; set; }
        public string TransactionId { get; set; }
        public int? Vote { get; set; }

        public virtual Course Course { get; set; }
        public virtual ApplicationUser User { get; set; }

        public Subscription(string userId, int courseId)
        {
            UserId = userId;
            CourseId = courseId;
        }
    }
}
