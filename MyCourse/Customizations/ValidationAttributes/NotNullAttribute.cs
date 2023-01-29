using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Customizations.ValidationAttributes
{
    public class NotNullAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value != null)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage, new [] { validationContext.MemberName });
        }
    }
}
