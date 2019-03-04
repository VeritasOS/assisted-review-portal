/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace garb.Helpers
{
  /// <summary>    
  /// Enables the efficient, dynamic composition of query predicates.    
  /// </summary>    
  public static class PredicateBuilder
  {
    /// <summary>    
    /// Creates a predicate that evaluates to true.    
    /// </summary>    
    public static Expression<Func<T, bool>> True<T>() { return param => true; }

    /// <summary>    
    /// Creates a predicate that evaluates to false.    
    /// </summary>    
    public static Expression<Func<T, bool>> False<T>() { return param => false; }

    /// <summary>    
    /// Creates a predicate expression from the specified lambda expression.    
    /// </summary>    
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate) { return predicate; }

    /// <summary>    
    /// Combines the first predicate with the second using the logical "and".    
    /// </summary>    
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
      return first.Compose(second, Expression.AndAlso);
    }

    /// <summary>    
    /// Combines the first predicate with the second using the logical "or".    
    /// </summary>    
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
      return first.Compose(second, Expression.OrElse);
    }

    /// <summary>    
    /// Negates the predicate.    
    /// </summary>    
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
      var negated = Expression.Not(expression.Body);
      return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }

    /// <summary>    
    /// Combines the first expression with the second using the specified merge function.    
    /// </summary>    
    static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
    {
      // zip parameters (map from parameters of second to parameters of first)    
      var map = first.Parameters
          .Select((f, i) => new { f, s = second.Parameters[i] })
          .ToDictionary(p => p.s, p => p.f);

      // replace parameters in the second lambda expression with the parameters in the first    
      var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

      // create a merged lambda expression with parameters from the first expression    
      return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    class ParameterRebinder : ExpressionVisitor
    {
      readonly Dictionary<ParameterExpression, ParameterExpression> map;

      ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
      {
        this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
      }

      public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
      {
        return new ParameterRebinder(map).Visit(exp);
      }

      protected override Expression VisitParameter(ParameterExpression p)
      {
        ParameterExpression replacement;

        if (map.TryGetValue(p, out replacement))
        {
          p = replacement;
        }

        return base.VisitParameter(p);
      }
    }
  }

}
