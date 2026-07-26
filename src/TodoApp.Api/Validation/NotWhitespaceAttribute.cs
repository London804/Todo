using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Validation;

// Fails validation when a string is present but consists only of whitespace
// (e.g. "   "). Pair it with [Required], which already handles null/empty —
// together they guarantee a genuinely non-blank value.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotWhitespaceAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        // Non-strings and null are not our concern — let other attributes judge them.
        if (value is not string s)
            return true;

        // The whole point: reject a string that is empty or only whitespace.
        return !string.IsNullOrWhiteSpace(s);
    }
}
