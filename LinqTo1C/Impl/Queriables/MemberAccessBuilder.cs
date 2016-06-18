using System.Collections.Generic;
using System.Linq.Expressions;
using LinqTo1C.Impl.Helpers;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace LinqTo1C.Impl.Queriables
{
    public class MemberAccessBuilder : RelinqExpressionVisitor
    {
        private readonly List<string> members = new List<string>();

        public string GetMembers(Expression expression)
        {
            members.Clear();
            Visit(expression);
            return members.JoinStrings(".");
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Visit(node.Expression);
            members.Add(node.Member.Name);
            return node;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            members.Add("src");
            return expression;
        }
    }
}