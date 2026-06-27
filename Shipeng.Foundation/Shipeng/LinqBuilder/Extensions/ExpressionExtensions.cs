using Shipeng.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Shipeng.LinqBuilder
{
    /// <summary>
    /// 表达式拓展类
    /// </summary>
    [SuppressSniffer]
    public static class ExpressionExtensions
    {
        /// <summary>
        /// 组合两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <param name="mergeWay">组合方式</param>
        /// <returns>新的表达式</returns>
        public static Expression<TSource> Compose<TSource>(this Expression<TSource> expression, Expression<TSource> extendExpression, Func<Expression, Expression, Expression> mergeWay)
        {
            var parameterExpressionSetter = expression.Parameters
                .Select((u, i) => new { u, Parameter = extendExpression.Parameters[i] })
                .ToDictionary(d => d.Parameter, d => d.u);

            var extendExpressionBody = ParameterReplaceExpressionVisitor.ReplaceParameters(parameterExpressionSetter, extendExpression.Body);
            return Expression.Lambda<TSource>(mergeWay(expression.Body, extendExpressionBody), expression.Parameters);
        }

        /// <summary>
        /// 与操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> And<TSource>(this Expression<Func<TSource, bool>> expression, Expression<Func<TSource, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.AndAlso);
        }

        /// <summary>
        /// 与操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> And<TSource>(this Expression<Func<TSource, int, bool>> expression, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.AndAlso);
        }

        /// <summary>
        /// 根据条件成立再与操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> AndIf<TSource>(this Expression<Func<TSource, bool>> expression, bool condition, Expression<Func<TSource, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.AndAlso) : expression;
        }

        /// <summary>
        /// 根据条件成立再与操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> AndIf<TSource>(this Expression<Func<TSource, int, bool>> expression, bool condition, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.AndAlso) : expression;
        }

        /// <summary>
        /// 或操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> Or<TSource>(this Expression<Func<TSource, bool>> expression, Expression<Func<TSource, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.OrElse);
        }

        /// <summary>
        /// 或操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> Or<TSource>(this Expression<Func<TSource, int, bool>> expression, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return expression.Compose(extendExpression, Expression.OrElse);
        }

        /// <summary>
        /// 根据条件成立再或操作合并两个表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, bool>> OrIf<TSource>(this Expression<Func<TSource, bool>> expression, bool condition, Expression<Func<TSource, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.OrElse) : expression;
        }

        /// <summary>
        /// 根据条件成立再或操作合并两个表达式，支持索引器
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式1</param>
        /// <param name="condition">布尔条件</param>
        /// <param name="extendExpression">表达式2</param>
        /// <returns>新的表达式</returns>
        public static Expression<Func<TSource, int, bool>> OrIf<TSource>(this Expression<Func<TSource, int, bool>> expression, bool condition, Expression<Func<TSource, int, bool>> extendExpression)
        {
            return condition ? expression.Compose(extendExpression, Expression.OrElse) : expression;
        }

        /// <summary>
        /// 获取Lambda表达式属性名，只限 u=>u.Property 表达式
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>属性名</returns>
        public static string GetExpressionPropertyName<TSource>(this Expression<Func<TSource, object>> expression)
        {
            if (expression.Body is UnaryExpression unaryExpression)
            {
                return ((MemberExpression)unaryExpression.Operand).Member.Name;
            }
            else if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            else if (expression.Body is ParameterExpression parameterExpression)
            {
                return parameterExpression.Type.Name;
            }

            throw new InvalidCastException(nameof(expression));
        }

        /// <summary>
        /// 是否是空集合
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="sources">集合对象</param>
        /// <returns>是否为空集合</returns>
        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> sources)
        {
            return sources == null || !sources.Any();
        }

        /// <summary>
        ///  根据字符串构建条件支持的比较符 >,<,>= ,<=,==, like  like两种模式,contain,startwith. %x% x%;
        ///  使用示例:
        ///  Expression<Func<ViewTestModel, bool>> condition = (item) => true;
        ///  string str = @$"(ViewTestModel.Range==1||ViewTestModel.Range>7)&&ViewTestModel.Tel like '%135%' && ViewTestModel.Id>0";
        ///  condition= condition.BuildCondition(str);
        ///  return _sqlSugarClient.Queryable<TestModel>().Select<ViewTestModel>().Where(condition).ToPage(_httpParameter);
        /// </summary>
        /// <typeparam name="TSource">泛型类型</typeparam>
        /// <param name="source">集合对象</param>
        /// <param name="condition"> condition中的条件格式为 类名.属性名</param>
        /// <returns></returns>
        public static Expression<TSource> BuildCondition<TSource>(this Expression<TSource> source, string condition)
        {
            try
            {
                if (string.IsNullOrEmpty(condition))
                {
                    return source;
                }
                string specchar = "&|()";
                string tempconditon = "";
                int i = 0;
                Stack<string> stack = new Stack<string>();//条件符号，和(
                Stack<Expression> expressions = new Stack<Expression>();
                while (i < condition.Length)
                {
                    if (!specchar.Contains(condition[i]))
                    {
                        tempconditon += condition[i];
                    }
                    else
                    {
                        switch (condition[i])
                        {
                            case '&':
                                i++;
                                stack.Push("&&");
                                if (!string.IsNullOrEmpty(tempconditon))
                                {
                                    expressions.Push(BuildExpression(source, tempconditon));
                                }
                                tempconditon = "";
                                break;
                            case '|':
                                i++;
                                stack.Push("||");
                                if (!string.IsNullOrEmpty(tempconditon))
                                {
                                    expressions.Push(BuildExpression(source, tempconditon));
                                }
                                tempconditon = "";
                                break;
                            case '(':
                                stack.Push("(");
                                tempconditon = "";
                                break;
                            case ')':
                                string top = stack.Pop();
                                while (top != "(")
                                {
                                    if (!string.IsNullOrEmpty(tempconditon))
                                    {
                                        expressions.Push(BuildExpression(source, tempconditon));
                                        tempconditon = "";
                                    }
                                    if (expressions.Count > 1)
                                    {
                                        string smby = top;//不是括号肯定是符号，不是&&就是||
                                        var exp1 = expressions.Pop();
                                        var exp2 = expressions.Pop();
                                        switch (smby)
                                        {
                                            case "&&":
                                                expressions.Push(Expression.And(exp1, exp2));
                                                break;
                                            case "||":
                                                expressions.Push(Expression.Or(exp1, exp2));
                                                break;
                                        }
                                    }
                                    top = stack.Pop();
                                }
                                if (!string.IsNullOrEmpty(tempconditon))
                                {
                                    expressions.Push(BuildExpression(source, tempconditon));
                                    tempconditon = "";
                                }
                                break;
                        }
                    }
                    i++;
                }
                if (!string.IsNullOrWhiteSpace(tempconditon))
                {
                    expressions.Push(BuildExpression(source, tempconditon));
                }
                while (expressions.Count > 1)
                {
                    var exp1 = expressions.Pop();
                    var exp2 = expressions.Pop();
                    string smby = stack.Pop();
                    switch (smby)
                    {
                        case "&&":
                            expressions.Push(Expression.And(exp1, exp2));
                            break;
                        case "||":
                            expressions.Push(Expression.Or(exp1, exp2));
                            break;
                    }
                }
                return Expression.Lambda<TSource>(expressions.Pop(), source.Parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 构建表达式
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Expression BuildExpression<TSource>(Expression<TSource> source, string condition)
        {
            Regex r = new Regex("([a-zA-Z][a-zA-Z0-9_]*\\.*[a-zA-Z0-9_]*\\s*)([><=]=*|like)(.+)");
            var res = r.Match(condition);
            if (res.Groups.Count < 4)
            {
                throw new Exception("条件错误");
            }
            string left = res.Groups[1].ToString().Trim();
            string symbol = res.Groups[2].ToString().Trim();
            string right = res.Groups[3].ToString().Trim();
            //根据左边表达式找到propertyinfo

            string[] leftgroup = left.Split('.');
            if (leftgroup.Length < 2)
            {
                throw new Exception("表达式错误");
            }
            var parmExp = source.Parameters.ToList().FirstOrDefault(o => o.Type.Name == leftgroup[0].Trim());
            if (parmExp == null)
            {
                throw new Exception("表达式错误,请确保表达式格式为 TableName.FeildName >= 0");
            }
            var propertyInfo = parmExp.Type.GetProperty(leftgroup[1].Trim());
            MemberExpression leftexp = Expression.Property(parmExp, propertyInfo);
            if (symbol.Trim().ToLower() == "like")
            {
                right = right.Trim().TrimStart('\'').TrimEnd('\'').TrimStart('"').TrimEnd('"').Trim();
                string methodName = "StartsWith";
                if (right.StartsWith("%") && right.EndsWith("%"))
                {
                    methodName = "Contains";
                }
                MethodInfo methodInfo = typeof(string).GetMethod(methodName, new Type[] { typeof(string) });
                ConstantExpression c = Expression.Constant(right.TrimStart('%').TrimEnd('%').Trim(), typeof(string));
                return Expression.Call(leftexp, methodInfo, c);
            }
            else
            {
                object rightValue = null;
                if (propertyInfo.PropertyType == typeof(bool))
                {
                    if (right == "0")
                    {
                        rightValue = false;
                    }
                    else
                    {
                        rightValue = true;
                    }
                }
                else if (propertyInfo.PropertyType.BaseType == typeof(System.Enum))
                {
                    if (int.TryParse(right, out int rightintvalue))
                    {
                        rightValue = System.Enum.Parse(propertyInfo.PropertyType, System.Enum.GetName(propertyInfo.PropertyType, rightintvalue));
                    }
                    else
                    {
                        rightValue = System.Enum.Parse(propertyInfo.PropertyType, right);
                    }
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    if (int.TryParse(right, out int rightintvalue))
                    {
                        rightValue = rightintvalue;
                    }
                    else
                    {
                        throw new Exception($"表达式错误类型转换失败{propertyInfo.Name}");
                    }
                }
                else //string类型的 ==
                {
                    rightValue = right;
                }
                ConstantExpression rightexp = Expression.Constant(rightValue, propertyInfo.PropertyType);
                switch (symbol)
                {
                    case ">":
                        return Expression.GreaterThan(leftexp, rightexp);
                    case "<":
                        return Expression.LessThan(leftexp, rightexp);
                    case ">=":
                        return Expression.GreaterThanOrEqual(leftexp, rightexp);
                    case "==":
                        return Expression.Equal(leftexp, rightexp);
                    case "<=":
                        return Expression.LessThanOrEqual(leftexp, rightexp);
                    default:
                        throw new Exception($"表达式错误,未定义符号{symbol}");
                }
            }
        }

    }
}