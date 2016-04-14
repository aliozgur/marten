﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Marten.Schema;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Marten.Linq
{
    public static class QueryModelExtensions
    {
        public static Type SourceType(this QueryModel query)
        {
            return query.MainFromClause.ItemType;
        }

        public static IEnumerable<ResultOperatorBase> AllResultOperators(this QueryModel query)
        {
            foreach (var @operator in query.ResultOperators)
            {
                yield return @operator;
            }

            if (query.MainFromClause.FromExpression is SubQueryExpression)
            {
                foreach (var @operator in query.MainFromClause.FromExpression.As<SubQueryExpression>().QueryModel.ResultOperators)
                {
                    yield return @operator;
                }
            }
        }

        public static IEnumerable<T> FindOperators<T>(this QueryModel query) where T : ResultOperatorBase
        {
            return query.AllResultOperators().OfType<T>();
        }

        public static bool HasOperator<T>(this QueryModel query) where T : ResultOperatorBase
        {
            return query.AllResultOperators().Any(x => x is T);
        }

        public static string ToOrderClause(this QueryModel query, IDocumentMapping mapping)
        {
            var orders = query.BodyClauses.OfType<OrderByClause>().SelectMany(x => x.Orderings).ToArray();
            if (!orders.Any()) return string.Empty;

            return " order by " + orders.Select(c => ToOrderClause(c, mapping)).Join(", ");
        }

        public static string ToOrderClause(this Ordering clause, IDocumentMapping mapping)
        {
            var locator = mapping.JsonLocator(clause.Expression);
            return clause.OrderingDirection == OrderingDirection.Asc
                ? locator
                : locator + " desc";
        }
    }
}