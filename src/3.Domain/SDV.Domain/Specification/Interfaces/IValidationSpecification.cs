using System;

namespace SDV.Domain.Specification.Interfaces;

public interface IValidationSpecification<T>
{
    void IsValid(T entity);
}
