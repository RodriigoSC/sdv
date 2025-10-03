using System;

namespace SDV.Domain.Specification.Interfaces;

public interface IValidationSpecification<in T>
{
    void IsValid(T entity);
}
