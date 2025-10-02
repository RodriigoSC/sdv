using System.Linq.Expressions;

namespace SDV.Domain.Specification.Interfaces;

public interface IExpressionSpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}
