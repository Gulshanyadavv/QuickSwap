using FluentValidation;
using O_market.DTOs;
using Microsoft.AspNetCore.Http;  // NEW: For IFormFile in BeValidImage

namespace O_market.Validators
{
    public class AdCreateValidator : AbstractValidator<AdCreateDto>
    {
        public AdCreateValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(1, 200);
            RuleFor(x => x.Description).MaximumLength(4000);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.Location).NotEmpty().Length(1, 100);
            RuleFor(x => x.CategoryId).GreaterThan(0);

            // NEW: Separate rule for collection count (max 10 images)
            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count <= 10)
                .WithMessage("Max 10 images allowed.");

            // Keep RuleForEach for per-file validation (type/size)
            RuleForEach(x => x.Images)
                .Must(BeValidImage)
                .WithMessage("Invalid image: Must be JPG/PNG, max 5MB.");
        }

        private bool BeValidImage(IFormFile file)
        {
            if (file == null) return true;
            return file.Length <= 5 * 1024 * 1024 &&  // 5MB limit
                   (file.ContentType == "image/jpeg" || file.ContentType == "image/png");
        }
    }

    public class AdUpdateValidator : AbstractValidator<AdUpdateDto>
    {
        public AdUpdateValidator()
        {
            RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title != null);
            RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue);
            // Add more as needed
        }
    }
}