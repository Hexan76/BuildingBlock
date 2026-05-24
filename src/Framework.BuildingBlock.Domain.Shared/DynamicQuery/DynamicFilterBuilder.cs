using Framework.BuildingBlock.Domain.Shared;
using HashtApp.Soft.Client.Utilities;
using System.Collections;
using System.Linq.Expressions;
using System.Text.Json;

namespace Framework.BuildingBlock
{
    public static class DynamicFilterBuilder
    {
        public static Expression<Func<T, bool>> BuildPredicate<T>(FilterGroup filterGroup)
        {
            if (filterGroup == null) throw new ArgumentNullException(nameof(filterGroup));
            var param = Expression.Parameter(typeof(T), "x");
            var body = BuildGroupExpression<T>(filterGroup, param);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static Expression BuildGroupExpression<T>(FilterGroup group, ParameterExpression param)
        {
            if (group == null)
                return Expression.Constant(true);

            Expression? finalExpr = null;

            foreach (var filter in group.Items ?? Enumerable.Empty<FilterItem>())
            {
                if (filter == null) continue;

                var expr = BuildFilterExpression<T>(filter, param);
                if (expr == null) continue;

                finalExpr = finalExpr == null
                    ? expr
                    : group.LogicalOperator == FilterLogicalOperator.And
                        ? Expression.AndAlso(finalExpr, expr)
                        : Expression.OrElse(finalExpr, expr);
            }

            foreach (var subGroup in group.SubGroups ?? Enumerable.Empty<FilterGroup>())
            {
                var subExpr = BuildGroupExpression<T>(subGroup, param);

                if (subExpr == null || IsAlwaysTrue(subExpr)) continue;

                finalExpr = finalExpr == null
                    ? subExpr
                    : group.LogicalOperator == FilterLogicalOperator.And
                        ? Expression.AndAlso(finalExpr, subExpr)
                        : Expression.OrElse(finalExpr, subExpr);
            }

            return finalExpr ?? Expression.Constant(true);
        }

        private static bool IsAlwaysTrue(Expression expr)
        {
            return expr is ConstantExpression c && c.Value is bool b && b;
        }

        private static Expression? BuildFilterExpression<T>(FilterItem filter, ParameterExpression param)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            Expression prop = param;
            foreach (var part in filter.Property.Split('.'))
                prop = Expression.PropertyOrField(prop, part);

            object? rawValue = filter.Value;

            if (rawValue is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Array)
                {
                    var list = je.EnumerateArray()
                                 .Select(x => ConvertJsonElement(x, prop.Type))
                                 .Where(x => x != null)
                                 .ToList();
                    rawValue = list;
                }
                else
                {
                    rawValue = ConvertJsonElement(je, prop.Type); // تبدیل به نوع مقصد
                }
            }

            Expression? expr = null;

            switch (filter.Operator)
            {
                case FilterOperator.Equal:
                case FilterOperator.NotEqual:
                case FilterOperator.GreaterThan:
                case FilterOperator.GreaterThanOrEqual:
                case FilterOperator.LessThan:
                case FilterOperator.LessThanOrEqual:
                    {
                        // Numeric / DateTime / String coercion
                        if (!TryConvertToType(rawValue, prop.Type, out var converted))
                            return null;

                        var constant = Expression.Constant(converted, prop.Type);

                        expr = filter.Operator switch
                        {
                            FilterOperator.Equal => Expression.Equal(prop, constant),
                            FilterOperator.NotEqual => Expression.NotEqual(prop, constant),
                            FilterOperator.GreaterThan => Expression.GreaterThan(prop, constant),
                            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(prop, constant),
                            FilterOperator.LessThan => Expression.LessThan(prop, constant),
                            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(prop, constant),
                            _ => throw new NotSupportedException()
                        };
                        break;
                    }

                case FilterOperator.Contains:
                case FilterOperator.StartsWith:
                case FilterOperator.EndsWith:
                    {
                        Expression stringProp = prop.Type == typeof(string)
                            ? prop
                            : Expression.Call(prop, prop.Type.GetMethod("ToString", Type.EmptyTypes)!);

                        var pattern = rawValue?.ToString() ?? string.Empty;

                        var method = filter.Operator switch
                        {
                            FilterOperator.Contains => typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                            FilterOperator.StartsWith => typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
                            FilterOperator.EndsWith => typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!,
                            _ => throw new NotSupportedException()
                        };

                        expr = Expression.Call(stringProp, method, Expression.Constant(pattern));
                        break;
                    }

                case FilterOperator.In:
                case FilterOperator.ListContains:
                    {
                        if (rawValue is IEnumerable rawList)
                        {
                            var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(prop.Type))!;
                            foreach (var item in rawList)
                            {
                                if (TryConvertToType(item, prop.Type, out var converted))
                                {
                                    typedList.Add(converted);
                                }
                            }

                            if (typedList.Count == 0) return null;

                            var method = typeof(Enumerable).GetMethods()
                                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                                .MakeGenericMethod(prop.Type);

                            return Expression.Call(method, Expression.Constant(typedList), prop);
                        }

                        return null;
                    }
                case FilterOperator.AnyEqual:
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(prop.Type))
                        {
                            var genericType = prop.Type.GenericTypeArguments[0];
                            var method = typeof(Enumerable).GetMethods()
                                .First(m => m.Name == "Any" && m.GetParameters().Length == 1).MakeGenericMethod(genericType);

                            var constant = Expression.Constant(rawValue);
                            var anyEx = Expression.Call(method, prop);
                            return Expression.Equal(anyEx, constant);

                        }

                        return null;
                    }
                case FilterOperator.AnyNotEqual:
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(prop.Type))
                        {
                            var genericType = prop.Type.GenericTypeArguments[0];
                            var method = typeof(Enumerable).GetMethods()
                                .First(m => m.Name == "Any" && m.GetParameters().Length == 1).MakeGenericMethod(genericType);

                            var constant = Expression.Constant(rawValue);
                            var anyEx = Expression.Call(method, prop);
                            return Expression.NotEqual(anyEx, constant);

                        }

                        return null;
                    }
                case FilterOperator.AnyGreaterThan:
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(prop.Type))
                        {
                            var genericType = prop.Type.GenericTypeArguments[0];
                            var method = typeof(Enumerable).GetMethods()
                                .First(m => m.Name == "Any" && m.GetParameters().Length == 1).MakeGenericMethod(genericType);

                            var constant = Expression.Constant(rawValue);
                            var anyEx = Expression.Call(method, prop);
                            return Expression.GreaterThan(anyEx, constant);

                        }

                        return null;
                    }
                case FilterOperator.AnyLessThan:
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(prop.Type))
                        {
                            var genericType = prop.Type.GenericTypeArguments[0];
                            var method = typeof(Enumerable).GetMethods()
                                .First(m => m.Name == "Any" && m.GetParameters().Length == 1).MakeGenericMethod(genericType);

                            var constant = Expression.Constant(filter.Value);
                            var anyEx = Expression.Call(method, prop);
                            return Expression.LessThan(anyEx, constant);

                        }

                        return null;
                    }
                default:
                    expr = Expression.Constant(true);
                    break;
            }

            return expr;
        }

        private static object ConvertJsonElement(JsonElement je, Type? targetType)
        {
            if (je.ValueKind == JsonValueKind.String)
            {
                if (targetType == typeof(DateTime) && DateTime.TryParse(je.GetString(), out var dt))
                    return dt;
                if (targetType == typeof(int) && int.TryParse(je.GetString(), out var i))
                    return i;
                if (targetType == typeof(long) && long.TryParse(je.GetString(), out var l))
                    return l;

                return je.GetString()!;
            }

            if (je.ValueKind == JsonValueKind.Number)
            {
                if (targetType == typeof(int)) return je.GetInt32();
                if (targetType == typeof(long)) return je.GetInt64();
                if (targetType == typeof(double)) return je.GetDouble();
                return je.GetDecimal();
            }

            if (je.ValueKind == JsonValueKind.True) return true;
            if (je.ValueKind == JsonValueKind.False) return false;

            return je.GetRawText();
        }

        private static bool TryConvertToType(object? value, Type targetType, out object? converted)
        {
            converted = null;
            if (value == null) return true;

            try
            {
                if (targetType == typeof(string))
                {
                    converted = value.ToString();
                    return true;
                }

                if (targetType == typeof(int))
                {
                    if (value is int i) { converted = i; return true; }
                    if (value is long l) { converted = (int)l; return true; }
                    if (value is double d) { converted = (int)d; return true; }
                    if (int.TryParse(value.ToString(), out var pi)) { converted = pi; return true; }
                    return false;
                }

                if (targetType == typeof(long))
                {
                    if (value is long l) { converted = l; return true; }
                    if (value is int i) { converted = (long)i; return true; }
                    if (value is double d) { converted = (long)d; return true; }
                    if (long.TryParse(value.ToString(), out var pl)) { converted = pl; return true; }
                    return false;
                }

                if (targetType == typeof(double))
                {
                    if (value is double d) { converted = d; return true; }
                    if (value is int i) { converted = (double)i; return true; }
                    if (value is long l) { converted = (double)l; return true; }
                    if (double.TryParse(value.ToString(), out var pd)) { converted = pd; return true; }
                    return false;
                }

                if (targetType == typeof(bool))
                {
                    if (value is bool b) { converted = b; return true; }
                    if (bool.TryParse(value.ToString(), out var pb)) { converted = pb; return true; }
                    return false;
                }

                if (targetType == typeof(DateTime))
                {
                    if (value is DateTime dt) { converted = dt; return true; }
                    if (DateTime.TryParse(value.ToString(), out var pdt)) { converted = pdt; return true; }
                    return false;
                }

                // fallback
                converted = Convert.ChangeType(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
