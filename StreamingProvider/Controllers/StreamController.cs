using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StreamingTest.Models;
using StreamingTest.Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamingTest.Controllers
{
    [ApiController]
    public class StreamController : ControllerBase
    {
 
        [HttpGet]
        [Route("Block")]
        public ActionResult GetBlock()
        {
            return Ok(YieldThingies());
        }

        [HttpGet]
        [Route("Block2")]
        public ActionResult GetBlock2()
        {
            return Ok(System.Text.Json.JsonSerializer.Serialize(YieldThingies()));
        }

        [HttpGet]
        [Route("Stream")]
        public async Task GetStream()
        {

            await using (StreamSerializer writer = new StreamSerializer(Response.Body))
                await writer.WriteAsTokenizedSeries(YieldThingies());
        }


        [HttpGet]
        [Route("Stream2")]
        public async Task GetStream2()
        {

            await using (StreamSerializer writer = new StreamSerializer(Response.Body))
                await writer.WriteAllAsync(YieldThingies());
        }
        static Random random = new Random();


        private static IEnumerable<Thingy> YieldThingies()
        {
            int count = 0;
            while (count++ < 100)
            {
                yield return new Thingy() { Value1 = random.Next(), Value2 = (float)random.NextDouble(), Value3 = random.NextDouble(), SomeString = GenerateBiggun() };
            }
        }

        private static string GenerateBiggun()
        {
            int min = 0;
            int max = 5000;
            string s = "";
            while (min++ < max)
            {
                s += (char)random.Next(0, 255);
            }
            return s;
        }
    }
}
