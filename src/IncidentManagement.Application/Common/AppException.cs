namespace IncidentManagement.Application.Common;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message, 401) { }
}
