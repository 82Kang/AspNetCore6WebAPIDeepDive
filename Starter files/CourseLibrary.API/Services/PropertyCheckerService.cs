using System.Reflection;

namespace CourseLibrary.API.Services;

public class PropertyCheckerService : IPropertyCheckerService
{
  public bool TypeHasProperties<T>(string? fields)
  {
    if (string.IsNullOrWhiteSpace(fields)) return true;

    var propertyInfoList = new List<PropertyInfo>();
    var bindingInfo = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

    foreach (var field in fields.Split(","))
    {
      var trimField = field.Trim();
      var property = typeof(T).GetProperty(trimField, bindingInfo);
      if (property == null) return false;
      propertyInfoList.Add(property);
    }

    return true;
  }
}
