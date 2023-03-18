using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;

namespace CourseLibrary.API.Helpers;

public class PropertyMappingService : IPropertyMappingService
{
  private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping = 
    new(StringComparer.OrdinalIgnoreCase)
    {
      { "Id", new(new [] { "Id" }) },
      { "MainCategory", new(new [] { "MainCategory" }) },
      { "Age", new(new [] { "DateOfBirth" }, true) },
      { "Name", new (new [] { "FirstName", "LastName" }) },
    };

  private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

  public PropertyMappingService()
  {
    _propertyMappings.Add(new PropertyMapping<AuthorDto, Author> (_authorPropertyMapping));
  }

  public Dictionary<string, PropertyMappingValue> GetPropertyMapping
    <TSource, TDestination>()
  {
    var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
    if (matchingMapping.Count() == 1)
    {
      return matchingMapping.First().MappingDictionary;
    }

    throw new Exception($"Can't find exact property mapping instance "
        + $"for <{typeof(TSource)}, {typeof(TDestination)}");
  }

  public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
  {
    if (string.IsNullOrWhiteSpace(fields))
      return true;

    var propertyMapping = GetPropertyMapping<TSource, TDestination>();
    var fieldsAfterSplit = fields.Split(",");

    foreach (var field in fieldsAfterSplit)
    {
      //can't trim the var in foreach, so we create another one
      var trimmedField = field.Trim();
      var orderDescending = trimmedField.EndsWith(" desc");

      var indexOfFirstSpace = trimmedField.IndexOf(" ");
      var propertyName = indexOfFirstSpace == -1 ? trimmedField :
        trimmedField.Remove(indexOfFirstSpace);

      if (!propertyMapping.ContainsKey(propertyName)) return false;
    }
    return true;
  }
}
