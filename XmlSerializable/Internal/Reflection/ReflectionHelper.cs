using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Serialization.Xml.Internal.Reflection {
    public static class ReflectionHelper {
        public static MethodInfo GetMethod<T>(Expression<Action<T>> expression) {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetMethod(Expression<Action> expression) {
            return ((MethodCallExpression)expression.Body).Method;
        }
        public static MethodInfo GetGenericMethodDefinition<T>(Expression<Action<T>> expression) {
            return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition();
        }

        public static MethodInfo GetGenericMethodDefinition(Expression<Action> expression) {
            return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition();
        }
    }
}
