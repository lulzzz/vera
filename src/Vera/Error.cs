namespace Vera
{
    public sealed class Error
    {
        public Error(ErrorCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public ErrorCode Code { get; set; }
        public string Message { get; set; }
    }

    public enum ErrorCode
    {
        None,
        Exists
    }
}