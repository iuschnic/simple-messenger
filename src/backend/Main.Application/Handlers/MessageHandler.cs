using Newtonsoft.Json;
using Serilog;
using Main.Application.InPorts;
using Main.Application.Exceptions;
using Shared.Main.Auth;
using Shared.Main.Auth.Dtos;

namespace Main.Application.Handlers;

public class MessageHandler: IMessageHandler
{
    private readonly IUserService _userService;
    private readonly ILogger _logger;
    public MessageHandler(IUserService userService, ILogger logger)
    {
        _userService = userService;
        _logger = logger;
    }
    public async Task OnMessageReceivedFromBrokerAsync(EventType eventType, string dataJson)
    {
        _logger.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson);
        switch (eventType)
        {
            case EventType.CreateUser:
                await CreateUser(dataJson);
                break;
            case EventType.RemoveUser:
                await RemoveUser(dataJson);
                break;
        }
    }

    private async Task CreateUser(string dataJson)
    {
        UserCreateDto? user;
        try
        {
            user = JsonConvert.DeserializeObject<UserCreateDto>(dataJson);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to deserialize message");
            throw new DeserializeException();
        }
        if (user is null)
            throw new DeserializeException();

        await _userService.CreateUserAsync(user.Id, user.UniqueName, user.DisplayedName);
    }

    private async Task RemoveUser(string dataJson)
    {
        UserRemoveDto? user;
        try
        {
            user = JsonConvert.DeserializeObject<UserRemoveDto>(dataJson);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to deserialize message");
            throw new DeserializeException();
        }
        if (user is null)
            throw new DeserializeException();

        await _userService.RemoveUserAsync(user.Id);
    }
}
