using Microsoft.AspNetCore.Mvc;
using Auth.BL.Models;
using Auth.BL.OutputPorts;
using Auth.BL.InputPorts;
using Auth.BL.Utils;
using Shared.Main.Auth.Models;

namespace Auth.BL.Services;
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IKafkaProducer _kafkaProducer;

    public AuthService(
        IUserRepository userRepository,
        ITokenGenerator tokenGenerator,
        IKafkaProducer kafkaProducer)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<ActionResult> RegisterAsync(RegisterRequest registerRequest)
    {
        // Проверка на существующего пользователя
        if (await _userRepository.FindUserByNameAsync(registerRequest.UniqueName).ConfigureAwait(false) is not null)
            return new ConflictResult(); // 409 Conflict

        var id = Guid.NewGuid();
        var passwordHash = PasswordHasher.HashPassword(registerRequest.Password);

        var user = new User(id, registerRequest.UniqueName, registerRequest.Email, passwordHash);

        await _userRepository.AddUserAsync(user).ConfigureAwait(false);

        var message = new UserCreateDto(user.Id, user.UniqueName, registerRequest.DisplayedName);
        await _kafkaProducer.ProduceUserRegisteredAsync(message);
        
        return new CreatedResult();
    }

    public async Task<ActionResult> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _userRepository.FindUserByNameAsync(loginRequest.UniqueName).ConfigureAwait(false);
        if (user is null)
            return new UnauthorizedResult(); // 404

        // Проверка пароля
        if (!PasswordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            await _userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            return new UnauthorizedResult();
        }

        await _userRepository.UpdateUserAsync(user).ConfigureAwait(false);

        var jwt = _tokenGenerator.CreateJwtToken(user.Id);
        var response = new AuthResponse(jwt);

        // await _kafkaProducer.SendAsync("auth-events", user.Id.ToString(), jwt).ConfigureAwait(false);

        return new OkObjectResult(response);
    }
}
