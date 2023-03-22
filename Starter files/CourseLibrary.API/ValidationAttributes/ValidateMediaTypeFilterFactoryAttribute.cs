using Microsoft.AspNetCore.Mvc.Filters;

namespace CourseLibrary.API.ValidationAttributes;

public class ValidateMediaTypeFiterFactoryAttribute : Attribute, IFilterFactory
{
  /*
   * Indicates whether this filter can be used across multiple requests
   */
  public bool IsReusable => false;

  public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
  {
    throw new NotImplementedException();
  }
}
