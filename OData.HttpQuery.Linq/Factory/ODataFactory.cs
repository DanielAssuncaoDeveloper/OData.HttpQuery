using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;

namespace OData.HttpQuery.Linq.Factory
{
    public class ODataFactory<TEntity>
    {
        private string _filter { get; set; }

        public void Filter(Expression<Func<TEntity, bool>> expression)
        {
            _filter = ODataFilterRecursive(expression.Body as BinaryExpression);
            Console.WriteLine(_filter);
        }

        private string ODataFilterRecursive(BinaryExpression binaryExp)
        {
            string filter = string.Empty;
            if (binaryExp is null) return filter;

            if (binaryExp.NodeType == ExpressionType.OrElse || binaryExp.NodeType == ExpressionType.AndAlso)
            {
                string leftCondition = ODataFilterRecursive(binaryExp.Left as BinaryExpression);
                string rightCondition = ODataFilterRecursive(binaryExp.Right as BinaryExpression);

                string logicalOperator = binaryExp.NodeType switch
                {
                    ExpressionType.OrElse => "or",
                    ExpressionType.AndAlso => "and"
                };

                string newLogicalOperation = $"({leftCondition} {logicalOperator} {rightCondition})";
                filter += newLogicalOperation;
            }
            else
            {
                // Verificando o método binário da expressão e fazendo sua transcrição para o padrão OData
                string method = binaryExp.NodeType switch
                {
                    // todo: Adicionar suporte a Not Operator
                    ExpressionType.Equal => "eq",
                    ExpressionType.NotEqual => "ne",
                    ExpressionType.GreaterThan => "gt",
                    ExpressionType.GreaterThanOrEqual => "ge",
                    ExpressionType.LessThan => "lt",
                    ExpressionType.LessThanOrEqual => "le",
                };

                // Verificando os parametros da condição
                string leftParameter = GetConditionParameter(binaryExp.Left);
                string rightParameter = GetConditionParameter(binaryExp.Right);

                filter = $"{leftParameter} {method} {rightParameter}";
            }

            return filter;
        }

        private string GetConditionParameter(Expression expression)
        {
            string paramCondition = string.Empty;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberAccess = expression as MemberExpression;
                if (memberAccess.Expression.NodeType == ExpressionType.Parameter)
                {
                    paramCondition = memberAccess.Member.Name;
                    return paramCondition;
                }

                // Os MemberExpression não guardam nenhum valor consigo, a única Expression que de fato guarda valor é a "ConstantExpression".
                // Por conta disso, quando temos o acesso de uma propriedade na expressão, devemos primeiro obter o valor da variável na raiz do objeto (FieldInfo),
                // e posteriormente voltar obtendo os valores de todas as propriedades acessadas até chegar na final.
                var memberInfoStack = new Stack<MemberInfo>();
                var currentMemberExp = memberAccess;

                while (currentMemberExp.Member.MemberType != MemberTypes.Field)
                {
                    memberInfoStack.Push(currentMemberExp.Member);
                    currentMemberExp = currentMemberExp.Expression as MemberExpression;
                }
                var constantExp = currentMemberExp.Expression as ConstantExpression;
                var field = currentMemberExp.Member as FieldInfo;

                var lastFildValue = field.GetValue(constantExp.Value);
                while (memberInfoStack.Any())
                {
                    var memberInfo = memberInfoStack.Pop();
                    var propertyInfo = memberInfo as PropertyInfo;

                    lastFildValue = propertyInfo.GetValue(lastFildValue);
                }

                return GetValueNormalized(lastFildValue);
            }
            else if (expression.NodeType == ExpressionType.Constant)
            {
                var constantExp = expression as ConstantExpression;
                return GetValueNormalized(constantExp.Value);
            }

            return paramCondition;
        }


        private string GetValueNormalized(object value)
        {
            if (value is null) return "null";
            if (value is string strValue) return $"'{strValue.Replace("'", "''")}'";
            if (value is DateTime datetimeValue) return datetimeValue.ToString("yyyy-MM-ss HH:mm:ss.d");
            if (value is decimal decimalValue) return decimalValue.ToString(CultureInfo.InvariantCulture);
            if (value is double doubleValue) return doubleValue.ToString(CultureInfo.InvariantCulture);
            if (value is float floatValue) return floatValue.ToString(CultureInfo.InvariantCulture);
            if (value is long longValue) return longValue.ToString(CultureInfo.InvariantCulture);
            if (value is int intValue) return intValue.ToString(CultureInfo.InvariantCulture);
            if (value is short shortValue) return shortValue.ToString(CultureInfo.InvariantCulture);
            if (value is true) return "true";
            if (value is false) return "false";

            return string.Empty;
        }
    }

}
