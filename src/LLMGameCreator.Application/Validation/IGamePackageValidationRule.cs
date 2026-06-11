using LLMGameCreator.Domain.Validation;

namespace LLMGameCreator.Application.Validation;

internal interface IGamePackageValidationRule
{
    void Validate(ValidationContext context, ValidationReport report);
}
