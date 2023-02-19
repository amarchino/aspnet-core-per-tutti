using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Models.InputModels.Lessons
{
    public class LessonDeleteInputModel
    {
        [Required]
        public int Id { get; set; }
        public int CourseId { get; set; }
    }
}
