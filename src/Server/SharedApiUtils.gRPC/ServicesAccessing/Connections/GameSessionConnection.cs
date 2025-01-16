using Grpc.Core;
using Grpc.Net.Client;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace SharedApiUtils.gRPC.ServicesAccessing.Connections;

public class GameSessionConnection
{
    private readonly AuthenticationTokenAccessor tokenAccessor;
    private readonly string url;
    private readonly HttpClientHandler httpClientHandler;
    public GameSessionConnection(AuthenticationTokenAccessor tokenAccessor, AccessingConfiguration configuration)
    {
        this.tokenAccessor = tokenAccessor;
        url = configuration.GameSessionServiceUrl ?? throw new ArgumentNullException("Games session service url is null");
        bool trustAnySSLCertificate = configuration.IgnoreSslVerification;
        httpClientHandler = new HttpClientHandler();
        if (trustAnySSLCertificate)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }


    }
    public async Task<(GameSession.GameSessionClient? client, Metadata? headers)> GetClient()
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

            var client = new GameSession.GameSessionClient(chanel);
            Metadata headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");
            return (client, headers);
        }
        catch (Exception)
        {
            throw;
        }

    }
}
