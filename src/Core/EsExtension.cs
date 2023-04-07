using System.Linq.Expressions;
using Nest;

namespace Core;

public static class EsExtension
{
    public static ISearchResponse<T> Where<T>(this ElasticClient esClient, Expression<Func<T, bool>> expression)
        where T : class
    {
        var list = new ParseExpression(expression).CurrExpressions;

        var musts = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
        var mustNots = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
        var shoulds = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
        var filters = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();

        foreach (var item in list)
        {
            switch (item.ExpressionType)
            {
                case ExpressionType.AndAlso:
                    ParseOperator(musts, item);
                    break;
                case ExpressionType.OrElse:
                    ParseOperator(shoulds, item);
                    break;
                case ExpressionType.NotEqual:
                    ParseOperator(mustNots, item);
                    break;
                default:
                    throw new Exception("未知运算符");
            }
        }

        var searchResults = esClient.Search<T>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(musts.ToArray())
                    .MustNot(mustNots.ToArray())
                    .Should(shoulds.ToArray())
                    .Filter(filters.ToArray())
                )
            )
        );

        return searchResults;
    }

    private static void ParseOperator<T>(List<Func<QueryContainerDescriptor<T>, QueryContainer>> list,
        ExpressionDto expression) where T : class
    {
        var rightType = expression.ExpressionValue!.Type.ToString().ToLower();
        if (rightType.Contains("int") || rightType.Contains("double"))
        {
            switch (expression.ExpressionOperator)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    list.Add(x => x.Term(expression.ExpressionProperty, expression.ExpressionValue.Value));
                    break;
                case ExpressionType.GreaterThan:
                    list.Add(x => x.Range(y =>
                        y.Field(expression.ExpressionProperty)
                            .GreaterThan(Convert.ToDouble(expression.ExpressionValue.Value))));
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    list.Add(x => x.Range(y =>
                        y.Field(expression.ExpressionProperty)
                            .GreaterThanOrEquals(Convert.ToDouble(expression.ExpressionValue.Value))));
                    break;
                case ExpressionType.LessThan:
                    list.Add(x => x.Range(y =>
                        y.Field(expression.ExpressionProperty)
                            .LessThan(Convert.ToDouble(expression.ExpressionValue.Value))));
                    break;
                case ExpressionType.LessThanOrEqual:
                    list.Add(x => x.Range(y =>
                        y.Field(expression.ExpressionProperty)
                            .LessThanOrEquals(Convert.ToDouble(expression.ExpressionValue.Value))));
                    break;
            }
        }
        else if (rightType.Contains("string"))
        {
            switch (expression.ExpressionOperator)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    list.Add(x => x.Term(expression.ExpressionProperty, expression.ExpressionValue.Value));
                    break;
                case ExpressionType.Call:
                    list.Add(x => x.MatchPhrase(y =>
                        y.Field(expression.ExpressionProperty).Query(expression.ExpressionValue.Value!.ToString())));
                    break;
            }
        }
    }
}
