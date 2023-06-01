using Akavache;
using FASTER.core;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace SistemskoProgramiranjeLab2;

public class Program
{
    public static async Task Main(string[] args)
    {
        await AsyncHttpServer.Start();
    }
}
