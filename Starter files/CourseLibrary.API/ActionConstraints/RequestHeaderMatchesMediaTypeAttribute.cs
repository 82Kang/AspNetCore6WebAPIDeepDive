using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
{
    private readonly string _requestHeaderToMatch;
    private readonly MediaTypeCollection _mediaTypes = new();

    public int Order { get; }
    public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
        string mediaType, params string[] otherMediaTypes)
    {
      _requestHeaderToMatch = requestHeaderToMatch ??
        throw new ArgumentNullException(nameof(requestHeaderToMatch));

      foreach(var m in otherMediaTypes.Append(mediaType))
      {
        if (MediaTypeHeaderValue.TryParse(m, out var parsedMediaType))
        {
          _mediaTypes.Add(parsedMediaType);
        }

        else 
        {
          throw new ArgumentException(nameof(parsedMediaType));
        }
      }
    }

    public bool Accept(ActionConstraintContext context)
    {
      var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
      if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
      {
        return false;
      }

      var parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]!);
      return _mediaTypes.Any(m => parsedRequestMediaType.Equals(new MediaType(m)));
    }
}
