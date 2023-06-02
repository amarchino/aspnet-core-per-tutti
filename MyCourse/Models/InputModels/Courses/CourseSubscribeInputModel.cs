using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.InputModels.Courses
{
    public class CourseSubscribeInputModel
    {
        public int CourseId { get; set; }
        public Money Paid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; }
        public string TransactionId { get; set; }
        public string UserId { get; set; }
    }
}
