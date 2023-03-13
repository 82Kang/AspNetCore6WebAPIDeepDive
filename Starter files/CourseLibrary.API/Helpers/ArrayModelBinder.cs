using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Helpers;

public class ArrayModelBinder : IModelBinder
{
  public Task BindModelAsync(ModelBindingContext bindingContext)
  {
    //our binder works only on IEnumerable 
    if (!bindingContext.ModelMetadata.IsEnumerableType)
    {
      bindingContext.Result = ModelBindingResult.Failed();
      return Task.CompletedTask;
    }

    //get the inputted value
    var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

    //if the value is null or whitespace
    if (string.IsNullOrEmpty(value))
    {
      bindingContext.Result = ModelBindingResult.Success(null);
      return Task.CompletedTask;
    }

    //get the value of the passed parameter
    var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
    var converter = TypeDescriptor.GetConverter(elementType);

    //convert the each item in the value list to enumerable
    var values = value.Split(new [] { "," }, 
        StringSplitOptions.RemoveEmptyEntries)
        .Select(x => converter.ConvertFromString(x.Trim()))
        .ToArray();

    //create an array of that type, and set it as model value
    var typedValues = Array.CreateInstance(elementType, values.Length);
    values.CopyTo(typedValues, 0);
    bindingContext.Model = typedValues;

    //return a successful result, passing in the Model
    bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
    return Task.CompletedTask;
  }
}
