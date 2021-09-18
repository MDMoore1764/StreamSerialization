using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StreamingTest.Stream
{
    public class StreamSerializer : IDisposable, IAsyncDisposable
    {
        private StreamWriter writer;
        private SerializationMechanism mechanism;

        private XmlSerializer xmlSerializer;


        public bool Disposed { get; private set; }

        public StreamSerializer(System.IO.Stream stream)
        {
            Disposed = false;
            this.writer = new StreamWriter(stream, Encoding.UTF8, -1, true);
            mechanism = SerializationMechanism.Json;
        }

        public StreamSerializer(System.IO.Stream stream, Encoding encoding)
        {
            Disposed = false;
            this.writer = new StreamWriter(stream, encoding, -1, true);
            mechanism = SerializationMechanism.Json;
        }

        /// <summary>
        /// If using XML serialiation, T must be the provided object type and not a type derived from type T specified.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="mechanism"></param>
        public StreamSerializer(System.IO.Stream stream, SerializationMechanism mechanism)
        {
            Disposed = false;
            this.writer = new StreamWriter(stream, Encoding.UTF8, -1, true);
            this.mechanism = mechanism;
        }

        /// <summary>
        /// If using XML serialiation, T must be the provided object type and not a type derived from type T specified.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="mechanism"></param>
        public StreamSerializer(System.IO.Stream stream, Encoding encoding, SerializationMechanism mechanism)
        {
            Disposed = false;
            this.writer = new StreamWriter(stream, encoding, -1, true);
            this.mechanism = mechanism;
        }

        private void CreateXmlSerializer(Type type)
        {

            ConstructorInfo[] constructors = type.GetConstructors();
            bool hasParameterlessConstructor = constructors.Any(c => !c.GetParameters().GetEnumerator().MoveNext());

            if (!hasParameterlessConstructor)
            {
                throw new TypeAccessException("Xml serialization requires a public, parameterless constructor.");
            }
            xmlSerializer = new XmlSerializer(type);
        }


        /// <summary>
        /// Write all values of an enumeration to the response streamas individually-parsable tokens and finalize the response message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public async Task WriteAsTokenizedSeries<T>(IEnumerable<T> enumeration)
        {
            foreach (T value in enumeration)
            {
                await writer.WriteLineAsync(Serialize(value));
            }

            await Finalize();
        }

        /// <summary>
        /// Write all values of an enumeration to the response stream as a single object readable upon request completion and finalize sthe response message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public async Task WriteAllAsync<T>(T value)
        {
            await writer.WriteAsync(Serialize(value));

            await Finalize();
        }
        public async Task WriteObjectAsync<T>(T value)
        {
            await writer.WriteLineAsync(Serialize(value));
            await Finalize();
        }

        private JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.General);
        private StringWriter textWriter = new StringWriter();

        private string Serialize<T>(T value)
        {
            if (this.mechanism == SerializationMechanism.Json)
            {
                return JsonSerializer.Serialize(value, jsonOptions);
            }

            CreateXmlSerializer(value.GetType());
            xmlSerializer.Serialize(textWriter, value);
            return textWriter.ToString();
        }

        /// <summary>
        /// Finalizes the stream and response body.
        /// This call is not required when writing all lines to the stream, which utilizes the writer's full suite of options automatically.
        /// </summary>
        /// <returns></returns>
        public async Task Finalize()
        {
            await writer.FlushAsync();
            //writer.Close();
        }

        /// <summary>
        /// Whether the stream can currently read.
        /// </summary>
        public bool CanRead => !this.Disposed;

        /// <summary>
        /// Whether the stream can currently seek.
        /// </summary>
        public bool CanSeek => !this.Disposed;

        /// <summary>
        /// Always false.
        /// </summary>
        public bool CanTimeout => false;

        /// <summary>
        /// Whether the stream can currently write.
        /// </summary>
        public bool CanWrite => !this.Disposed;

        public async ValueTask DisposeAsync()
        {
            Disposed = true;
            await textWriter.DisposeAsync();
            await writer.DisposeAsync();
        }

        public void Dispose()
        {
            Disposed = true;
            textWriter.Dispose();
            writer.Dispose();
        }
    }
}
