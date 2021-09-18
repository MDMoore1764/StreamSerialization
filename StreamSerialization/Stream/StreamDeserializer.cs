using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace StreamSerialization.Stream
{
    public class StreamDeserializer
    {
        private HttpClient client;
        public StreamDeserializer(HttpClient client)
        {
            this.client = client;
        }

        public async IAsyncEnumerable<T> ReadTokenizedStreamAsync<T>(HttpRequestMessage request)
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            using var s = await response.Content.ReadAsStreamAsync();
            using StreamReader sr = new(s);
            using JsonReader reader = new JsonTextReader(sr);
            reader.SupportMultipleContent = true;
            JsonSerializer serializer = new();
            while (reader.Read())
                if (reader.TokenType == JsonToken.StartObject)
                    yield return serializer.Deserialize<T>(reader);

            await s.DisposeAsync();
        }

        public async IAsyncEnumerable<T> ReadTokenizedStreamAsync<T>(string url)
        {
            await foreach (T value in ReadTokenizedStreamAsync<T>(GetRequestMessage(url)))
                yield return value;
        }

        public async IAsyncEnumerable<T> ReadTokenizedStreamAsync<T>(Uri uri)
        {
            await foreach (T value in ReadTokenizedStreamAsync<T>(GetRequestMessage(uri)))
                yield return value;
        }

        public async Task<T> ReadStreamAsync<T>(HttpRequestMessage request)
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            using var s = await response.Content.ReadAsStreamAsync();
            using StreamReader sr = new(s);
            using JsonReader reader = new JsonTextReader(sr);
            JsonSerializer serializer = new();
            return serializer.Deserialize<T>(reader);
        }

        public async Task<T> ReadStreamAsync<T>(string url) => 
            await ReadStreamAsync<T>(GetRequestMessage(url));


        public async Task<T> ReadStreamAsync<T>(Uri uri) =>
             await ReadStreamAsync<T>(GetRequestMessage(uri));

        private HttpRequestMessage GetRequestMessage(string url) =>
           new(HttpMethod.Get, url);

        private HttpRequestMessage GetRequestMessage(Uri url) =>
            new(HttpMethod.Get, url);
    }
}
