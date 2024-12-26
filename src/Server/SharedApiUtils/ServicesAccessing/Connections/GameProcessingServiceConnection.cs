using Grpc.Core;
using Grpc.Net.Client;
using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.ServicesAccessing.Connections;

public class GameProcessingServiceConnection
{
    private readonly AuthenticationTokenAccessor tokenAccessor;

    public GameProcessingServiceConnection(AuthenticationTokenAccessor tokenAccessor)
    {
        this.tokenAccessor = tokenAccessor;
    }
    public async Task<(GameProcessing.GameProcessingClient? client, Metadata? headers)> GetClient(string url, bool trustAnySSLCertificate)
    {
        try
        {
            var httpClientHandler = new HttpClientHandler();
            if (trustAnySSLCertificate)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
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

            var client = new GameProcessing.GameProcessingClient(chanel);
            Metadata headers = new Metadata
            {
                { "Authorization", $"Bearer {token}" }
            };
            return (client, headers);
        }
        catch (Exception)
        {
            throw;
        }

    }
}
