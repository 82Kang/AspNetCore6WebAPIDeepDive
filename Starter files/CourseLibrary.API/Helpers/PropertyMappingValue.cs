namespace CourseLibrary.API.Helpers;

public class PropertyMappingValue
{
  public IEnumerable<string> DestinationProperties { get; private set; }
  public bool Revert { get; private set; } = false;

  public PropertyMappingValue(IEnumerable<string> destinationProperties, bool revert = false)
  {
    DestinationProperties = destinationProperties ?? throw new ArgumentNullException(
                                      nameof(destinationProperties));
    Revert = revert; 
  }
}
