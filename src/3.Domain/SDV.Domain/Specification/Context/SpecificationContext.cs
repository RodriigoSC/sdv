using SDV.Domain.Exceptions;
using SDV.Domain.Specification.Interfaces;

namespace SDV.Domain.Specification.Context;

public class SpecificationContext<T>
{
    private readonly IEnumerable<IValidationSpecification<T>> _IoCSpecifications;
    private readonly List<Type> _validationSpecifications = new();

    public SpecificationContext(IEnumerable<IValidationSpecification<T>> validationSpecifications)
    {
        _IoCSpecifications = validationSpecifications;
    }

    public SpecificationContext<T> SetSpecification(Type specificationType)
    {
        _validationSpecifications.Add(specificationType);
        return this;
    }

    public void Validate(T entity)
    {
        if (!_validationSpecifications.Any())
        {
            throw new EntityValidationException(
                typeof(T).Name, "You must add at least one specification before calling Validate."
            );
        }
        
        foreach (var specType in _validationSpecifications)
        {
            var spec = _IoCSpecifications.FirstOrDefault(x => x.GetType() == specType);
            spec?.IsValid(entity);
        }
    }
}
