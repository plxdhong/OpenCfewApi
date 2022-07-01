
using CFEW.Shared;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xray.App.Proxyman.Command;
using Xray.App.Stats.Command;

namespace CFEW.Server.Services;
public class XrayGrpcService
{
    private GrpcChannel GrpcChannel;
    private Xray.App.Proxyman.Command.HandlerService.HandlerServiceClient _client;
    private StatsService.StatsServiceClient _statClient;
    private readonly IConfiguration Configuration;

    public XrayGrpcService(IConfiguration configuration)
    {
        Configuration = configuration;
        GrpcChannel = GrpcChannel.ForAddress(Configuration["XrayServer"]);
        _client = new HandlerService.HandlerServiceClient(GrpcChannel);
        _statClient = new StatsService.StatsServiceClient(GrpcChannel);
    }
    public async Task<AlterInboundResponse> AddUserOperation(UserDetail user,string inbound_tag="vless-443",uint level=0)
    {
        AddUserOperation addUser = new AddUserOperation();
        addUser.User = new Xray.Common.Protocol.User { Email=user.Email,Level=level };
        Xray.Proxy.Vless.Account account = new Xray.Proxy.Vless.Account { Id = user.Uuid, Flow = Configuration["XrayFlow"] };
        addUser.User.Account = new Xray.Common.Serial.TypedMessage { 
            Type= "xray.proxy.vless.Account",
            Value=account.ToByteString()
        };

        var response = await _client.AlterInboundAsync(new AlterInboundRequest
        {
            Tag = inbound_tag,
            Operation = new Xray.Common.Serial.TypedMessage
            {
                Type = "xray.app.proxyman.command.AddUserOperation",
                Value = addUser.ToByteString()
            }
        });
        return response;

    }
    public async Task<AlterInboundResponse> RemoveUserOperation(UserDetail user, string inbound_tag = "vless-443")
    {
        RemoveUserOperation removeUser = new RemoveUserOperation();
        removeUser.Email = user.Email;
        var response = await _client.AlterInboundAsync(new AlterInboundRequest
        {
            Tag = inbound_tag,
            Operation = new Xray.Common.Serial.TypedMessage
            {
                Type = "xray.app.proxyman.command.RemoveUserOperation",
                Value = removeUser.ToByteString()
            }
        });
        return response;
    }
    public async Task<(long up,long down)> GetUserStatAsync(UserDetail user,bool reset=false)
    {
        var responseUp = await _statClient.GetStatsAsync(new GetStatsRequest
        {
            Name = $"user>>>{user.Email}>>>traffic>>>uplink",
            Reset = reset
        });
        var responseDown = await _statClient.GetStatsAsync(new GetStatsRequest
        {
            Name = $"user>>>{user.Email}>>>traffic>>>downlink",
            Reset = reset
        });
        return (up: responseUp.Stat.Value, down: responseDown.Stat.Value);
    }
}
