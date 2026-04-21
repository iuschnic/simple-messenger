namespace BL.Exceptions;

public static class ExceptionMapper
{
    public static Exception Map(ApiException ex)
    {
        return ex switch
        {
            UnauthorizedException => new AuthException("Неверный логин или пароль"),

            BadRequestException e => new ValidationException(e.Message),

            NotFoundException e => new NotFoundAppException(e.Message),

            ConflictException e => new ValidationException(e.Message),

            ForbiddenException e => new AuthException("Нет доступа"),

            ServerException e => new AppException("Ошибка сервера"),

            _ => new AppException(ex.Message)
        };
    }
}