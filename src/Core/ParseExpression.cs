using System.Linq.Expressions;

namespace Core;

public class ParseExpression : ExpressionVisitor
{
    public List<ExpressionDto> CurrExpressions { get; set; } = new();

    public ParseExpression(Expression expression)
    {
        // 解析表达式
        base.Visit(expression);
        // 调整表达式类型
        AdjustExpressionType();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        CurrExpressions.Add(new ExpressionDto());
        Visit(node.Left);
        SetOperator(node.NodeType);
        Visit(node.Right);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        CurrExpressions.Last().ExpressionProperty = node;
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        CurrExpressions.Last().ExpressionValue = node;
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        CurrExpressions.Add(new ExpressionDto());
        Visit(node.Object);
        SetOperator(node.NodeType);
        Visit(node.Arguments[0]);
        return node;
    }

    private void SetOperator(ExpressionType type)
    {
        switch (type)
        {
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                CurrExpressions.Last().ExpressionType = type;
                break;
            default:
                CurrExpressions.Last().ExpressionOperator = type;
                break;
        }
    }

    private void AdjustExpressionType()
    {
        // 清理多余的表达式
        CurrExpressions.RemoveAll(x => x.ExpressionProperty is null && x.ExpressionValue is null);
        // 类型位移
        for (int i = CurrExpressions.Count - 1; i >= 0; i--)
        {
            if (i == 0)
            {
                CurrExpressions[i].ExpressionType = ExpressionType.AndAlso;
                break;
            }

            CurrExpressions[i].ExpressionType = CurrExpressions[i - 1].ExpressionType;
        }

        // 调整运算符
        foreach (var expressionDto in CurrExpressions.Where(x => x.ExpressionOperator == ExpressionType.NotEqual))
        {
            expressionDto.ExpressionType = ExpressionType.NotEqual;
        }
    }
}