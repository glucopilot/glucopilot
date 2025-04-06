using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GlucoPilot.Api.Models
{
    public sealed record ListReadingsRequest : IParsable<ListReadingsRequest>
    {
        [Required]
        public DateTimeOffset From { get; set; }

        [Required]
        public DateTimeOffset? To { get; set; }

        public static ListReadingsRequest Parse(string s, IFormatProvider? provider)
        {
            if (TryParse(s, provider, out var result))
            {
                return result!;
            }
            throw new FormatException($"Input string '{s}' was not in a correct format.");
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out ListReadingsRequest? result)
        {
            result = null;
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            var parts = s.Split(',');
            if (parts.Length != 2)
            {
                return false;
            }

            if (DateTimeOffset.TryParse(parts[0], provider, DateTimeStyles.None, out var from) &&
                DateTimeOffset.TryParse(parts[1], provider, DateTimeStyles.None, out var to))
            {
                result = new ListReadingsRequest
                {
                    From = from,
                    To = to
                };
                return true;
            }

            return false;
        }
    }

    public sealed class Validator : AbstractValidator<ListReadingsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).LessThanOrEqualTo(DateTimeOffset.UtcNow);
        }
    }
}
