using FluentValidation;
using O_market.DTOs;

namespace O_market.Validators
{
    public class FavoriteToggleValidator : AbstractValidator<FavoriteToggleDto>
    {
        public FavoriteToggleValidator()
        {
            RuleFor(x => x.AdId).GreaterThan(0);
        }
    }
}