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

        public static List<TTarget> TryParseModelList<TSource, TTarget>(List<TSource> sourceList)
            where TSource : class
            where TTarget : class, new()
        {
            if (sourceList == null)
                return new List<TTarget>();

            // Build property dictionaries
            var sourcePropDict = typeof(TSource).GetProperties()
                .ToDictionary(p => p.Name, p => p);

            var targetPropDict = typeof(TTarget).GetProperties()
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p);

            var targetList = new List<TTarget>();

            foreach (var source in sourceList)
            {
                if (source == null)
                    continue;

                var target = new TTarget();

                // Map properties using dictionaries
                foreach (var sourceProp in sourcePropDict)
                {
                    if (targetPropDict.TryGetValue(sourceProp.Key, out var targetProp))
                    {
                        var sourceType = Nullable.GetUnderlyingType(sourceProp.Value.PropertyType)
                            ?? sourceProp.Value.PropertyType;
                        var targetType = Nullable.GetUnderlyingType(targetProp.PropertyType)
                            ?? targetProp.PropertyType;

                        if (sourceType == targetType)
                        {
                            var value = sourceProp.Value.GetValue(source);
                            if (value != null)
                                targetProp.SetValue(target, value);
                        }
                    }
                }

                targetList.Add(target);
            }

            return targetList;
        }
    }
}
