using FluentValidation;
using O_market.DTO;
using O_market.DTOs;

namespace O_market.Validators
{
    public class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
    {
        public CategoryCreateValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Length(1, 100);
            RuleFor(x => x.Description).MaximumLength(500);
            RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);  // Optional, but valid if set
        }
    }

    public class CategoryUpdateValidator : AbstractValidator<CategoryUpdateDto>
    {
        public CategoryUpdateValidator()
        {
            RuleFor(x => x.Name).Length(1, 100).When(x => !string.IsNullOrEmpty(x.Name));
            RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}