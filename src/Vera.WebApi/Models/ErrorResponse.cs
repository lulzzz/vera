namespace Vera.WebApi.Models
{
    public class ErrorResponse
    {
        public ErrorResponse(Error error)
        {
            Code = error.Code.ToString().ToUpper();
            Message = error.Message;
        }

        public string Code { get; set; }
        public string Message { get; set; }
    }
}