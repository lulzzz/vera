namespace Vera.Poland.Models.Enums
{
  public enum ParameterTypeEnum
  {
    Type1 = 1,
    Type2 = 2,
    Type3 = 3,
    // Type4 = 4, no known cases
    Type5Q = 5, // Q edge case, treated as a new parameter type
    Type6Q = 6, // Q edge case, treated as a new parameter type
    Ignored = 100 // represents "-" and "---" from the docs, I don't know what - and --- could represent otherwise
  }
}