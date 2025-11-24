namespace FinalProject.NET.Services
{
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }


        public static ServiceResponse Ok(string msg, object data = null) => new() { Success = true, Message = msg, Data = data };
        public static ServiceResponse Fail(string msg) => new() { Success = false, Message = msg };
    }
}
