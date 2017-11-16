using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Project.Net
{
    public interface IProjection<T>
    {
        TaskAwaiter<T[]> GetAwaiter();
        IProjection<T> Where(Expression<Func<T, bool>> predicate);
        IProjection<U> Select<U>(Expression<Func<T, U>> selector);
        IProjection<V> SelectMany<U, V>(Func<T, IProjection<U>> selector, Expression<Func<T, U, V>> resultSelector);
    }

    public interface IProjector : IDisposable
    {
        IProjection<T> Project<T>();
    }

    public static class ProjectionExtensions
    {
        public static IProjection<T> AsProjection<T>(this IEnumerable<T> list)
        {
            throw new NotImplementedException();
        }
    }
}