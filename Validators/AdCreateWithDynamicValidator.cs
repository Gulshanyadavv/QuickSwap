using FluentValidation;
using O_market.DTOs;
using System.Text.Json;

namespace O_market.Validators
{
    public class AdCreateWithDynamicValidator : AbstractValidator<AdCreateWithDynamicDto>
    {
        public AdCreateWithDynamicValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(1, 200);
            RuleFor(x => x.Description).MaximumLength(4000);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Location).NotEmpty().Length(1, 100);
            RuleFor(x => x.CategoryId).GreaterThan(0);

            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count <= 20)
                .WithMessage("Max 20 images allowed.");

            RuleForEach(x => x.Images)
                .Must(BeValidImage)
                .WithMessage("Invalid image: Must be JPG/PNG, max 5MB.")
                .When(x => x.Images != null);

            // Validate dynamic values
            RuleFor(x => x.DynamicValues)
                .Must(BeValidDynamicValues)
                .WithMessage("Invalid dynamic field values.")
                .When(x => x.DynamicValues != null);
        }

        private bool BeValidImage(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return true;
            return file.Length <= 5 * 1024 * 1024 &&
                   (file.ContentType == "image/jpeg" ||
                    file.ContentType == "image/png" ||
                    file.ContentType == "image/jpg" ||
                    file.ContentType == "image/webp");
        }

        private bool BeValidDynamicValues(List<DynamicFieldValueDto> values)
        {
            if (values == null) return true;

            // Check for duplicate field IDs
            var fieldIds = values.Select(v => v.FieldId).Distinct();
            if (fieldIds.Count() != values.Count) return false;

            // Check that values are not empty for required fields
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value.Value)) return false;
            }

            return true;
        }
    }

    public class DynamicFieldCreateValidator : AbstractValidator<DynamicFieldCreateDto>
    {
        public DynamicFieldCreateValidator()
        {
            RuleFor(x => x.CategoryId).GreaterThan(0);
            RuleFor(x => x.Label).NotEmpty().Length(1, 100);
            RuleFor(x => x.FieldType).NotEmpty()
                .Must(type => new[] { "text", "number", "dropdown", "checkbox", "radio", "textarea" }.Contains(type))
                .WithMessage("Invalid field type. Must be: text, number, dropdown, checkbox, radio, or textarea.");

            RuleFor(x => x.Options)
                .Must(BeValidJsonArray)
                .WithMessage("Options must be a valid JSON array.")
                .When(x => !string.IsNullOrEmpty(x.Options) &&
                          (x.FieldType == "dropdown" || x.FieldType == "radio" || x.FieldType == "checkbox"));
        }

        private bool BeValidJsonArray(string json)
        {
            if (string.IsNullOrEmpty(json)) return true;

            try
            {
                var options = JsonSerializer.Deserialize<List<string>>(json);
                return options != null && options.Any();
            }
            catch
            {
                return false;
            }
        }
    }
}