using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class IEnumerableExtensions
{
  public static IEnumerable<ExpandoObject> ShapeData<TSource>
    (this IEnumerable<TSource> source,
     string? fields)
    {
      if (source == null) throw new ArgumentNullException(nameof(source));

      var expObjList = new List<ExpandoObject>();
      var propertyInfoList = new List<PropertyInfo>();
      var bindingInfo = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

      if (string.IsNullOrWhiteSpace(fields))
      {
        var propertyInfo = typeof(TSource).GetProperties(bindingInfo);
        propertyInfoList.AddRange(propertyInfo);

      }
      else
      {
        /* Using AddRange might have performance overhead instead of just using a foreach loop */
        propertyInfoList.AddRange(fields.Split(",").Select<string, PropertyInfo> (f => {
                                      f = f.Trim();
                                      return typeof(TSource).GetProperty(f, bindingInfo) ?? 
                                      throw new InvalidOperationException(
                                          $"Property (field) {f} not found on {typeof(TSource)}");
                                   }));
      }

      /* Using AddRange might have performance overhead instead of just using a foreach loop */
      expObjList.AddRange(
        source.Select<TSource, ExpandoObject>(srcElem => {
          IDictionary<string, object?> expObj = new ExpandoObject();
          propertyInfoList.ForEach( propInfo => {
              expObj.Add(propInfo.Name, propInfo.GetValue(srcElem));
          });
          return (expObj as ExpandoObject)!;
      }));

      return expObjList;
    }

  public static ExpandoObject ShapeData<TSource>
    (this TSource source,
     string? fields)
    {
      if (source == null) throw new ArgumentNullException(nameof(source));

      var propertyInfoList = new List<PropertyInfo>();
      var bindingInfo = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

      if (string.IsNullOrWhiteSpace(fields))
      {
        var propertyInfo = typeof(TSource).GetProperties(bindingInfo);
        propertyInfoList.AddRange(propertyInfo);

      }
      else
      {
        /* Using AddRange might have performance overhead instead of just using a foreach loop */
        propertyInfoList.AddRange(fields.Split(",").Select<string, PropertyInfo> (f => {
                                      f = f.Trim();
                                      return typeof(TSource).GetProperty(f, bindingInfo) ?? 
                                      throw new InvalidOperationException(
                                          $"Property (field) {f} not found on {typeof(TSource)}");
                                   }));
      }

      /* Using AddRange might have performance overhead instead of just using a foreach loop */
      IDictionary<string, object?> expObj = new ExpandoObject();
      propertyInfoList.ForEach( propInfo => {
          expObj.Add(propInfo.Name, propInfo.GetValue(source));
      });

      return (expObj as ExpandoObject)!;
    }
}

      /* was not thinking that we have a ForEach construct in c#
      expObjList.AddRange(
        (source.Select<TSource, Dictionary<string, object?>>(srcElem => {
            return new Dictionary<string, object?>(
              propertyInfoList.Select<PropertyInfo, KeyValuePair<string, object?>>(propInfo => {
                return KeyValuePair.Create(propInfo.Name, propInfo.GetValue(srcElem));
            }));
      }) as IEnumerable<ExpandoObject>)!);
      */
