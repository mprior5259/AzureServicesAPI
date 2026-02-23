using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared.Helpers
{
    public static class ModelUtility
    {
        //Parse a model to a model using reflection
        public static TTarget? TryParseModel<TSource, TTarget>(TSource source) 
            where TSource : class
            where TTarget : class, new()
        {
            if (source == null)
            {
                return null;
            }
            var target = new TTarget();

            foreach (var sourceProp in typeof(TSource).GetProperties())
            {
                var sourcePropType = Nullable.GetUnderlyingType(sourceProp.PropertyType) ?? sourceProp.PropertyType;
                var targetProp = typeof(TTarget).GetProperty(sourceProp.Name);
                if (targetProp != null && targetProp.CanWrite)
                {
                    var targetPropType = Nullable.GetUnderlyingType(targetProp.PropertyType) ?? targetProp.PropertyType;
                    if (sourcePropType == targetPropType)
                    {
                        var value = sourceProp.GetValue(source);
                        if (value != null)
                        {
                            targetProp.SetValue(target, value);
                        }
                    }
                }
            }

            return target;
        }
    }
}
