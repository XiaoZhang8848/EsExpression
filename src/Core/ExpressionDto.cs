using System.Linq.Expressions;

namespace Core;

public class ExpressionDto
{
    public Expression? ExpressionProperty { get; set; }
    public ExpressionType? ExpressionOperator { get; set; }
    public ConstantExpression? ExpressionValue { get; set; }
    public ExpressionType? ExpressionType { get; set; }
}