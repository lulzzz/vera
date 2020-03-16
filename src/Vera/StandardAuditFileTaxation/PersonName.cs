namespace Vera.StandardAuditFileTaxation
{
  public sealed class PersonName
  {
    public string Title { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Initials { get; set; }

    /// <summary>
    /// Values such as Van, Von, etc.
    /// </summary>
    public string LastNamePrefix { get; set; }
    public string BirthName { get; set; }

    /// <summary>
    /// A formal sign of expression/greeting, expressed as text, such as: Right honourable, madam, monsignor.
    /// </summary>
    public string Salutation { get; set; }

    public string FullName => string.IsNullOrEmpty(LastNamePrefix)
      ? $"{FirstName} {LastName}"
      : $"{FirstName} {LastNamePrefix} {LastName}";
  }
}