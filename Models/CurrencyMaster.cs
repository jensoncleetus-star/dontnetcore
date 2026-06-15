using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QuickSoft.Models
{
    public class CurrencyMaster
    {       
        public long Id { get; set; }

        [StringLength(20)]
        [Required]
        public string CurrencyCode { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

        [StringLength(10)]
        [Required]
        public string ConvertionRate { get; set; }

        [StringLength(25)]
        [Required]
        public string Fraction { get; set; }

        [StringLength(10)]
        public string Symbol { get; set; }
        
        [StringLength(10)]
        public string MinConvertionRate { get; set; }
        
        [StringLength(10)]
        public string MaxConvertionRate { get; set; }

        public long Branch { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }

    //[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    //public sealed class DecimalPrecisionAttribute : Attribute
    //{
    //    public DecimalPrecisionAttribute(byte precision, byte scale)
    //    {
    //        Precision = precision;
    //        Scale = scale;
    //    }
    //    public byte Precision { get; set; }
    //    public byte Scale { get; set; }
    //}


    //protected override void OnModelCreating(CurrencyMaster modelBuilder)
    //{
    //    foreach (Type classType in from t in Assembly.GetAssembly(typeof(DecimalPrecisionAttribute)).GetTypes()
    //                               where t.IsClass && t.Namespace == "YOURMODELNAMESPACE"
    //                               select t)
    //    {
    //        foreach (var propAttr in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<DecimalPrecisionAttribute>() != null).Select(
    //               p => new { prop = p, attr = p.GetCustomAttribute<DecimalPrecisionAttribute>(true) }))
    //        {
    //            var entityConfig = modelBuilder.GetType().GetMethod("Entity").MakeGenericMethod(classType).Invoke(modelBuilder, null);
    //            ParameterExpression param = ParameterExpression.Parameter(classType, "c");
    //            Expression property = Expression.Property(param, propAttr.prop.Name);
    //            LambdaExpression lambdaExpression = Expression.Lambda(property, true,
    //                                                                     new ParameterExpression[]
    //                                                                         {param});
    //            DecimalPropertyConfiguration decimalConfig;
    //            if (propAttr.prop.PropertyType.IsGenericType && propAttr.prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
    //            {
    //                MethodInfo methodInfo = entityConfig.GetType().GetMethods().Where(p => p.Name == "Property").ToList()[7];
    //                decimalConfig = methodInfo.Invoke(entityConfig, new[] { lambdaExpression }) as DecimalPropertyConfiguration;
    //            }
    //            else
    //            {
    //                MethodInfo methodInfo = entityConfig.GetType().GetMethods().Where(p => p.Name == "Property").ToList()[6];
    //                decimalConfig = methodInfo.Invoke(entityConfig, new[] { lambdaExpression }) as DecimalPropertyConfiguration;
    //            }
    //            decimalConfig.HasPrecision(propAttr.attr.Precision, propAttr.attr.Scale);
    //        }
    //    }
    //}
}