namespace BL.Exceptions;

public static class ExceptionMapper
{
    public static Exception Map(ApiException ex)
    {
        return ex switch
        {
            // ================= AUTH =================
            UnauthorizedException =>
                new AuthException("Требуется авторизация"),

            ForbiddenException =>
                new AuthException("Недостаточно прав"),

            // ================= VALIDATION =================
            BadRequestException e =>
                new ValidationException(
                    string.IsNullOrWhiteSpace(e.Message)
                        ? "Некорректные данные"
                        : e.Message
                ),

            ConflictException e =>
                new ValidationException(
                    string.IsNullOrWhiteSpace(e.Message)
                        ? "Конфликт данных"
                        : e.Message
                ),

            // ================= NOT FOUND =================
            NotFoundException e =>
                new NotFoundAppException(
                    string.IsNullOrWhiteSpace(e.Message)
                        ? "Ресурс не найден"
                        : e.Message
                ),

            // ================= SERVER =================
            ServerException e =>
                new AppException(
                    $"Ошибка сервера ({e.StatusCode}): {e.Message}"
                ),

            // ================= FALLBACK =================
            _ => MapUnknown(ex)
        };
    }

    private static Exception MapUnknown(ApiException ex)
    {
        return ex.StatusCode switch
        {
            400 => new ValidationException(ex.Message),
            401 => new AuthException("Требуется авторизация"),
            403 => new AuthException("Недостаточно прав"),
            404 => new NotFoundAppException(ex.Message),
            409 => new ValidationException(ex.Message),

            408 => new AppException("Таймаут запроса"),
            429 => new AppException("Слишком много запросов"),

            >= 500 => new AppException($"Ошибка сервера ({ex.StatusCode}): {ex.Message}"),

            _ => new AppException(ex.Message)
        };
    }
}