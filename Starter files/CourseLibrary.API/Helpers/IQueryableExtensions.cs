using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Helpers;

static class IQueryableExtensions
{
  public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy
      ,Dictionary<string, PropertyMappingValue> mappingDictionary)
  {
    if (source == null)
      throw new ArgumentNullException(nameof(source));

    if (mappingDictionary == null)
      throw new ArgumentNullException(nameof(mappingDictionary));

    if (string.IsNullOrWhiteSpace(orderBy))
      return source;
    
    var orderByString = string.Empty;
    var orderByAfterSplit = orderBy.Split(",");

    foreach (var orderByClause in orderByAfterSplit)
    {
      //can't trim the var in foreach, so we create another one
      var trimmedOrderClause = orderByClause.Trim();
      var orderDescending = trimmedOrderClause.EndsWith(" desc");

      var indexOfFirstSpace = trimmedOrderClause.IndexOf(" ");
      var propertyName = indexOfFirstSpace == -1 ? trimmedOrderClause :
        trimmedOrderClause.Remove(indexOfFirstSpace);

      if (!mappingDictionary.ContainsKey(propertyName))
        throw new ArgumentException($"{nameof(mappingDictionary)} doesn't contain the property name");

      var propertyMappingVal = mappingDictionary[propertyName];

      if (propertyMappingVal == null)
        throw new ArgumentException($"{nameof(propertyMappingVal)} : {propertyMappingVal} key not present in the mapping Dictionary");

      orderDescending = propertyMappingVal.Revert ? !orderDescending : orderDescending;

      //find matching property
      //if (!mappingDictionary.ContainsKey(propertyName))
      foreach (var destinationProperty in propertyMappingVal.DestinationProperties)
      {
        orderByString = orderByString  + 
          (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
           + destinationProperty
           + (orderDescending ? " descending" : " ascending");
      }

    }
    return source.OrderBy(orderByString);
  }
  public static IQueryable<T> DoSort<T>(this IQueryable<T> source, string param)
  {
    return source.OrderBy(param);
  }
}

