using StreamSerialization.Stream;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamConsumer
{
    public class Program
    {
        public static void Main()
        {
           _ = MainAsync();
        }

        public static async Task MainAsync()
        {
            await Task.Delay(3000);
            Console.WriteLine("Console reader initialized.\n");

            HttpClient client = new();

            Consumers.StreamConsumer consumer = new(new StreamDeserializer(client), client);

            await consumer.PromptAsync();
        }
    }
}
