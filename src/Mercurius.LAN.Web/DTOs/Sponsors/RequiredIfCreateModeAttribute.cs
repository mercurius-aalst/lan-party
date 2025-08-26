using System;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Sponsors
{
    public class RequiredIfCreateModeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance as SponsorManagementDTO;
            if (instance!.IsCreateMode && value == null)
                return new ValidationResult(ErrorMessage ?? "Logo is required in create mode.", [ErrorMessageResourceName ?? string.Empty]);

            return ValidationResult.Success;
        }
    }
}
