using Newtonsoft.Json;
using StreamingTest.Models;
using StreamReaderTest.ObjectReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Consumers
{
    public class StreamConsumer
    {
        private StreamDeserializer deserializer;
        private HttpClient client;
        public StreamConsumer(StreamDeserializer deserializer, HttpClient client)
        {
            this.deserializer = deserializer;
            this.client = client;
        }

        public async IAsyncEnumerable<Thingy> StreamYielder()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string url = "http://localhost:5001/Stream";
            int count = 0;
            await foreach (Thingy thing in deserializer.ReadTokenizedStreamAsync<Thingy>(url))
            {
                if (count++ == 0) Console.WriteLine($"Time until first value yielded: {sw.ElapsedMilliseconds}ms.");
                yield return thing;
            }

            Console.WriteLine($"Tokenized stream read complete in {sw.ElapsedMilliseconds}ms.");
        }

        public async IAsyncEnumerable<Thingy> BlockOData()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string url = "http://localhost:5001/Block";

            HttpRequestMessage request = new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new Uri(url) };

            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
            {
                var enumerable = JsonConvert.DeserializeObject<IEnumerable<Thingy>>(await response.Content.ReadAsStringAsync());

                int count = 0;
                foreach (var value in enumerable)
                {
                    if (count++ == 0) Console.WriteLine($"Time until first value yielded: {sw.ElapsedMilliseconds}ms.");
                    yield return value;
                }
                sw.Stop();
                Console.WriteLine($"String read complete in {sw.ElapsedMilliseconds}ms.");
            }
        }
        public async IAsyncEnumerable<Thingy> BlockStreamYielder()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string url = "http://localhost:5001/Stream2";

            int count = 0;
            foreach (Thingy thing in await deserializer.ReadStreamAsync<IEnumerable<Thingy>>(url))
            {
                if (count++ == 0) Console.WriteLine($"Time until first value yielded: {sw.ElapsedMilliseconds}ms.");
                yield return thing;
            }

            Console.WriteLine($"Stream read complete in {sw.ElapsedMilliseconds}ms.");
        }

        public async Task PromptAsync()
        {
            while (true)
            {
                Console.WriteLine("Pull data?\n");
                string line = Console.ReadLine();

                if (line.ToLower().Trim() == "n")
                {
                    return;
                }
                Console.WriteLine("Pulling data!");

                Console.WriteLine(await StreamYielder().AverageAsync(t => t.Value1) + "\n");
                Console.WriteLine(await BlockStreamYielder().AverageAsync(t => t.Value1) + "\n");
                Console.WriteLine(await BlockOData().AverageAsync(t => t.Value1) + "\n");
            }
        }
    }
}
