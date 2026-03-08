using FluentValidation;
using O_market.DTOs;

namespace O_market.Validators
{
    public class MessageCreateValidator : AbstractValidator<MessageCreateDto>
    {
        public MessageCreateValidator()
        {
            RuleFor(x => x.Content).NotEmpty().Length(1, 2000);
            RuleFor(x => x.ReceiverId).GreaterThan(0);
            RuleFor(x => x.AdId).GreaterThan(0).When(x => x.AdId.HasValue);
        }
    }
}