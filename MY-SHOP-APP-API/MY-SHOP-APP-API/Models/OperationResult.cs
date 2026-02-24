namespace MY_SHOP_APP_API.Models
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static OperationResult<T> Ok(T data) => new OperationResult<T> { Success = true, Data = data };
        public static OperationResult<T> Fail(string message) => new OperationResult<T> { Success = false, Message = message };
    }
}
