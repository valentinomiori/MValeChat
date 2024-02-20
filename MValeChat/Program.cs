using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Orleans.Runtime;

namespace MValeChat;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .UseOrleans(static b =>
            {
                b.UseLocalhostClustering();
                b.AddMemoryGrainStorage("PubSubStore");
                b.AddMemoryStreams("chat");
            })
            .ConfigureServices(services =>
            {
            })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }
}