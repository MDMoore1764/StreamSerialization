using Newtonsoft.Json;
using Models.Streaming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using StreamSerialization.Stream;
using StreamProvider.Services;


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

        public IEnumerable<Thingy> LinkedYielder()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int count = 0;
            foreach (var value in StreamService.YieldThingies())
            {
                if (count++ == 0) Console.WriteLine($"Time until first value yielded: {sw.ElapsedMilliseconds}ms.");
                yield return value;
            }

            Console.WriteLine($"Memory read complete in {sw.ElapsedMilliseconds}ms.");
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

                Stopwatch sw = new Stopwatch();
                var streamYielder = StreamYielder().ToEnumerable();
                QuickList<Thingy> memoryielder = new(StreamYielder().ToEnumerable());

                //var blockStream = await BlockStreamYielder().ToListAsync();
                //var block = await BlockOData().ToListAsync();
                //var linked = LinkedYielder().ToList();

                Console.WriteLine(streamYielder.Average(t => t.Value1) + "\n");
                Console.WriteLine(memoryielder.Average(t => t.Value1) + "\n");
                //Console.WriteLine(blockStream.Average(t => t.Value1) + "\n");
                //Console.WriteLine(block.Average(t => t.Value1) + "\n");
                //Console.WriteLine(linked.Average(t => t.Value1) + "\n");
                Console.WriteLine("Running again!");
                Console.WriteLine(streamYielder.Average(t => t.Value1) + "\n");
                Console.WriteLine(memoryielder.Average(t => t.Value1) + "\n");
                //Console.WriteLine(blockStream.Average(t => t.Value1) + "\n");
                //Console.WriteLine(block.Average(t => t.Value1) + "\n");
                //Console.WriteLine(linked.Average(t => t.Value1) + "\n");
            }
        }
    }
}
