using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Com.PhilChuang.Utils
{
    internal static class Extensions
    {
        [JetBrains.Annotations.ContractAnnotation("self:null => halt")]
        public static void ThrowIfNull(this object self, string objName)
        {
            if (objName == null) throw new ArgumentNullException(nameof(objName));
            if (self == null) throw new ArgumentNullException(objName);
        }

        [JetBrains.Annotations.ContractAnnotation("self:null => halt")]
        public static void ThrowIfNullOrWhiteSpace(this string self, string objName)
        {
            objName.ThrowIfNull(nameof(objName));
            if (self.IsNullOrWhiteSpace()) throw new ArgumentNullException(objName);
        }

        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }

        public static string FormatWith(this string self, params object[] args)
        {
            self.ThrowIfNull(nameof(self));
            return string.Format(self, args);
        }

        public static MethodInfo GetMethodInfo(this Expression<Action> self)
        {
            self.ThrowIfNull(nameof(self));

            if (self.Body.NodeType == ExpressionType.Call)
            {
                var methodExpr = (MethodCallExpression) self.Body;
                return methodExpr.Method;
            }

            throw new Exception($"Expected MethodCallExpression, got {self.Body.NodeType}");
        }

        public static string GetMethodName(this Expression<Action> self)
        {
            return GetMethodInfo(self).Name;
        }

        public static PropertyInfo GetPropertyInfo<TProperty>(this Expression<Func<TProperty>> self)
        {
            self.ThrowIfNull(nameof(self));

            if (self.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) self.Body;
                return (PropertyInfo) memberExpr.Member;
            }

            if (self.Body.NodeType == ExpressionType.Convert
                && self.Body is UnaryExpression
                && ((UnaryExpression) self.Body).Operand.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) ((UnaryExpression) self.Body).Operand;
                return (PropertyInfo) memberExpr.Member;
            }

            throw new Exception($"Expected MemberAccess expression, got {self.Body.NodeType}");
        }

        public static string GetPropertyName<TProperty>(this Expression<Func<TProperty>> self)
        {
            return GetPropertyInfo(self).Name;
        }

        public static PropertyInfo GetPropertyInfo<TPropertyParent, TProperty>(this Expression<Func<TPropertyParent, TProperty>> self)
        {
            self.ThrowIfNull(nameof(self));

            if (self.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) self.Body;
                return (PropertyInfo) memberExpr.Member;
            }

            if (self.Body.NodeType == ExpressionType.Convert
                && self.Body is UnaryExpression
                && ((UnaryExpression) self.Body).Operand.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) ((UnaryExpression) self.Body).Operand;
                return (PropertyInfo) memberExpr.Member;
            }

            throw new Exception($"Expected MemberAccess expression, got {self.Body.NodeType}");
        }

        public static string GetPropertyName<TPropertyParent, TProperty>(this Expression<Func<TPropertyParent, TProperty>> self)
        {
            return GetPropertyInfo(self).Name;
        }

        public static string GetPropertyOrFieldName<TProperty>(this Expression<Func<TProperty>> self)
        {
            self.ThrowIfNull(nameof(self));

            // handles () => SomeProperty or () => mySomeProperty
            if (self.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) self.Body;
                return memberExpr.Member.Name;
            }

            throw new Exception($"Expected MemberAccess expression, got {self.Body.NodeType}");
        }

        public static string GetPropertyOrFieldName<TPropertyParent, TProperty>(this Expression<Func<TPropertyParent, TProperty>> self)
        {
            self.ThrowIfNull(nameof(self));

            // handles () => SomeProperty or () => mySomeProperty
            if (self.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) self.Body;
                return memberExpr.Member.Name;
            }

            throw new Exception($"Expected MemberAccess expression, got {self.Body.NodeType}");
        }
    }
}