using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CourseLibrary.API.ValidationAttributes;

public class ValidateMediaTypeFilter : Attribute, IActionFilter
{
  private ProblemDetailsFactory _problemDetailsFactory { get; set;}
  public ValidateMediaTypeFilter (ProblemDetailsFactory problemDetailsFactory)
  {
    _problemDetailsFactory = problemDetailsFactory;
  }

  public void OnActionExecuted(ActionExecutedContext context)
  {
    //don't touch the response
  }

  public void OnActionExecuting(ActionExecutingContext context)
  {
    var result = context.Result as ObjectResult;

    var mediaTypeRequested = context.ActionArguments["mediaType"];

    if (!(result?.ContentTypes.Contains(mediaTypeRequested) ?? 
          throw new Exception($"Result is not constructed in the filter, issue with the ASP .NET core.")))
    {
      result = new BadRequestObjectResult(_problemDetailsFactory.CreateProblemDetails(context.HttpContext,
                             statusCode: 400,
                             detail: $"Accept header media type is not a valid media type"));
    }
  }
}
