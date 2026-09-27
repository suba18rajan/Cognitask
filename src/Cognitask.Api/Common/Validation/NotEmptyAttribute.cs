using System.ComponentModel.DataAnnotations;

namespace Cognitask.Api.Common.Validation;

public class NotEmptyAttribute : ValidationAttribute
{
    public NotEmptyAttribute()
    {
        ErrorMessage = "The field cannot be empty or whitespace.";
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is string text &&
            !string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            ErrorMessage,
            new[] { validationContext.MemberName! });
    }
}