using System.ComponentModel.DataAnnotations;

namespace MyCourse.Customizations.ValidationAttributes;

public class NotNullAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value != null)
        {
            return ValidationResult.Success!;
        }
        return new ValidationResult(ErrorMessage, new List<string> { validationContext.MemberName! });
    }
}
