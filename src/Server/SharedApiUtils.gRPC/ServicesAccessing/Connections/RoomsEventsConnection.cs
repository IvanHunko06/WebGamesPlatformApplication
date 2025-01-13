using Grpc.Core;
using Grpc.Net.Client;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace SharedApiUtils.gRPC.ServicesAccessing.Connections;

public class RoomsEventsConnection
{
    private readonly AuthenticationTokenAccessor tokenAccessor;
    private readonly string url;
    private readonly HttpClientHandler httpClientHandler;
    public RoomsEventsConnection(AuthenticationTokenAccessor tokenAccessor, AccessingConfiguration configuration)
    {
        this.tokenAccessor = tokenAccessor;
        url = configuration.RoomsEventsHandlerUrl ?? throw new ArgumentNullException("Rooms events handler url is null");
        bool trustAnySSLCertificate = configuration.IgnoreSslVerification;
        httpClientHandler = new HttpClientHandler();
        if (trustAnySSLCertificate)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        }


    }
    public async Task<(RoomsEvents.RoomsEventsClient? client, Metadata? headers)> GetClient()
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

            var client = new RoomsEvents.RoomsEventsClient(chanel);
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