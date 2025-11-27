using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace EventBudgetPlanner.Web.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class Rfc822EmailAttribute : ValidationAttribute
{
    public Rfc822EmailAttribute() : base("Please enter a valid RFC 822 compliant email address.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        try
        {
            var mailAddress = new MailAddress(text.Trim());
            return mailAddress.Address.Equals(text.Trim(), StringComparison.OrdinalIgnoreCase)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
        catch (FormatException)
        {
            return new ValidationResult(ErrorMessage);
        }
    }
}
