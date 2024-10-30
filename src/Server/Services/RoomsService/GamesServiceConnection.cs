using Grpc.Core;
using Grpc.Net.Client;

namespace RoomsService;

public class GamesServiceConnection
{
    private readonly AuthenticationTokenAccessor tokenAccessor;
    private readonly IConfiguration configuration;
    private readonly ILogger<GamesServiceConnection> logger;
    private readonly string url;
    private readonly HttpClientHandler httpClientHandler;
    public GamesServiceConnection(AuthenticationTokenAccessor tokenAccessor, IConfiguration configuration, ILogger<GamesServiceConnection> logger)
    {
        this.tokenAccessor = tokenAccessor;
        this.configuration = configuration;
        this.logger = logger;
        var gamesServiceSection = configuration.GetRequiredSection("GamesService");
        url = gamesServiceSection.GetValue<string>("Url") ?? throw new ArgumentNullException("Games service url is null");
        bool trustAnySSLCertificate = gamesServiceSection.GetValue<bool>("IgnoreSslVerification");
        httpClientHandler = new HttpClientHandler();
        if (trustAnySSLCertificate)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }
        

    }
    public async Task<(RoomsService.Protos.Games.GamesClient? client, Metadata? headers)> GetClient()
    {
        try
        {
            string token = await tokenAccessor.GetNewToken();
            if (string.IsNullOrEmpty(token))
                return default;

            Uri uri = new Uri(url);
            var handler = new SubdirectoryHandler(httpClientHandler, uri.LocalPath);
            var chanel = GrpcChannel.ForAddress(url, new GrpcChannelOptions()
            {
                HttpHandler = handler,
                DisposeHttpClient = true,

            });

            var client = new RoomsService.Protos.Games.GamesClient(chanel);
            Metadata headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");
            return (client, headers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            return default;
        }

    }
}
