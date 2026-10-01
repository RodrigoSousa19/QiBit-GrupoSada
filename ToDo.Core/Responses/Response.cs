namespace ToDo.Core.Responses;

public class Response<T>(bool success, string message, T? data)
{
    public bool Success { get; private set; } = success;
    public string Message { get; private set; } = message;
    public T? Data { get; private set; } = data;

    public static Response<T> Ok(T data, string message = "") => new(true, message, data);
    
    public static Response<T> Error(string message) => new(false, message,default);
}