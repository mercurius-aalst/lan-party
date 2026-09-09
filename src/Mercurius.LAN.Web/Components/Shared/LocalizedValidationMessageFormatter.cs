using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mercurius.LAN.Web.Localization;

namespace Mercurius.LAN.Web.Components.Shared;

public sealed class LocalizedValidationMessageFormatter
{
    private readonly ILocalizationService _localization;

    public LocalizedValidationMessageFormatter(ILocalizationService localization)
    {
        _localization = localization;
    }

    public string Format(
        object model,
        ValidationResult result,
        IReadOnlyDictionary<string, string>? fieldLabelKeys = null,
        IReadOnlyDictionary<string, string>? validationMessageKeys = null)
    {
        var errorMessage = result.ErrorMessage ?? string.Empty;
        if(validationMessageKeys?.TryGetValue(errorMessage, out var messageKey) == true)
            return _localization[messageKey];

        var fieldName = result.MemberNames?.FirstOrDefault();
        if(string.IsNullOrWhiteSpace(fieldName))
            return _localization["form.invalid"];

        var property = model.GetType().GetProperty(fieldName);
        if(property is null)
            return _localization["form.invalid"];

        var value = property.GetValue(model);
        var context = new ValidationContext(model) { MemberName = fieldName };
        var fieldLabel = GetFieldLabel(fieldName, fieldLabelKeys);

        foreach(var attribute in property.GetCustomAttributes<ValidationAttribute>())
        {
            if(attribute.GetValidationResult(value, context) is null)
                continue;

            return attribute switch
            {
                RequiredAttribute => _localization.Get("form.requiredField", fieldLabel),
                StringLengthAttribute stringLength => FormatStringLength(stringLength, value, fieldLabel),
                RangeAttribute range => FormatRange(range, fieldLabel),
                _ => _localization["form.invalid"]
            };
        }

        return _localization["form.invalid"];
    }

    private string GetFieldLabel(string fieldName, IReadOnlyDictionary<string, string>? fieldLabelKeys) =>
        fieldLabelKeys?.TryGetValue(fieldName, out var key) == true
            ? _localization[key]
            : fieldName;

    private string FormatStringLength(StringLengthAttribute attribute, object? value, string fieldLabel)
    {
        if(value is string text && text.Length < attribute.MinimumLength)
            return _localization.Get("form.minLengthField", fieldLabel, attribute.MinimumLength);

        return _localization.Get("form.maxLengthField", fieldLabel, attribute.MaximumLength);
    }

    private string FormatRange(RangeAttribute attribute, string fieldLabel)
    {
        var minimum = FormatValue(attribute.Minimum);
        var maximum = FormatValue(attribute.Maximum);
        if(IsUnboundedMaximum(attribute.Maximum))
            return _localization.Get("form.minValueField", fieldLabel, minimum);

        return _localization.Get("form.rangeField", fieldLabel, minimum, maximum);
    }

    private string FormatValue(object value) =>
        Convert.ToString(value, _localization.Culture) ?? string.Empty;

    private static bool IsUnboundedMaximum(object value) =>
        value switch
        {
            int number => number == int.MaxValue,
            long number => number == long.MaxValue,
            _ => false
        };
}
