using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project.Net
{
    internal class Projection<T> : IProjection<T>
    {
        private static readonly Type iproj = typeof(IProjection<>);

        private readonly HttpClient client;
        private readonly string subDomain, plural;
        private readonly Dictionary<string, object> wheres;
        private readonly HashSet<string> projections;
        private readonly TypeInfo type;
        private readonly Delegate selector;

        internal Projection(HttpClient client)
        {
            this.client = client;

            type = typeof(T).GetTypeInfo();
            plural = type.GetCustomAttribute<PluralAttribute>()?.Plural ?? $"{type.Name}s";
            subDomain = type.GetCustomAttribute<SubDomainAttribute>()?.SubDomain ?? plural;

            wheres = new Dictionary<string, object>();
            projections = new HashSet<string>();
        }

        private Projection(HttpClient client, TypeInfo type, string plural, string subDomain, Dictionary<string, object> wheres, HashSet<string> projections, Delegate selector)
        {
            this.client = client;
            this.type = type;
            this.plural = plural;
            this.subDomain = subDomain;
            this.wheres = wheres;
            this.projections = projections;
            this.selector = selector;
        }

        public TaskAwaiter<T[]> GetAwaiter()
        {
            return Taskify().GetAwaiter();
        }

        public IProjection<T> Where(Expression<Func<T, bool>> predicate)
        {
            BuildQuery(predicate.Body);
            return this;
        }

        public IProjection<U> Select<U>(Expression<Func<T, U>> selector)
        {
            BuildProjection(selector.Body);
            return new Projection<U>(client, type, plural, subDomain, wheres, projections, (selector as LambdaExpression).Compile());
        }

        public IProjection<V> SelectMany<U, V>(Func<T, IProjection<U>> selector, Expression<Func<T, U, V>> resultSelector) { throw new NotImplementedException(); }

        private void BuildQuery(Expression query)
        {
            switch (query.NodeType)
            {
                case ExpressionType.Equal:
                    {
                        var bin = query as BinaryExpression;
                        var names = FindMember(bin.Left);
                        wheres[string.Join(".", names)] = Expression.Lambda(bin.Right).Compile().DynamicInvoke();
                    }
                    break;
                case ExpressionType.AndAlso:
                    {
                        var bin = query as BinaryExpression;
                        BuildQuery(bin.Left);
                        BuildQuery(bin.Right);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void BuildProjection(Expression projection)
        {
            switch (projection.NodeType)
            {
                case ExpressionType.MemberInit:
                    {
                        var init = projection as MemberInitExpression;
                        var assignments = init.Bindings.Cast<MemberAssignment>();
                        assignments.Where(b => !b.Expression.Type.IsConstructedGenericType || b.Expression.Type.GetGenericTypeDefinition() != iproj).Select(b => string.Join(".", FindMember(b.Expression))).Select(projections.Add).ToArray();
                        BuildProjection(init.NewExpression);
                    }
                    break;
                case ExpressionType.New:
                    {
                        var newExp = projection as NewExpression;
                        newExp.Arguments.Where(a => !a.Type.IsConstructedGenericType || a.Type.GetGenericTypeDefinition() != iproj).Select(a => string.Join(".", FindMember(a))).Select(projections.Add).ToArray();
                    }
                    break;
                case ExpressionType.Parameter:
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private List<string> FindMember(Expression param)
        {
            switch (param.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var mem = param as MemberExpression;
                    var names = FindMember(mem.Expression);
                    names.Add(mem.Member.Name);
                    return names;
                case ExpressionType.Parameter:
                    return new List<string>();
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<T[]> Taskify()
        {
            var proj = $"__projection={string.Join(",", projections.Select(Uri.EscapeUriString))}";
            var query = string.Join("&", wheres.Where(w => w.Value != null).Select(w => $"{Uri.EscapeUriString(w.Key)}={Uri.EscapeUriString(w.Value.ToString())}").Concat(Enumerable.Repeat(proj, 1)));
            //var response = await client.GetAsync($"http://{subDomain}.scit.amazon.com/{plural}?{query}");
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new[] { new Goose { M = new Goose { X = "inner" }, X = "outer" } })),
                StatusCode = System.Net.HttpStatusCode.OK
            };

            response.EnsureSuccessStatusCode();

            var temp = (object[])JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync(), type.MakeArrayType());
            return temp.Select(o => selector.DynamicInvoke(o)).Cast<T>().ToArray();
        }
    }
}