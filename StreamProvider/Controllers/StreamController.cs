using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Streaming;
using StreamProvider.Services;
using StreamSerialization.Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamProvider.Controllers
{
    [ApiController]
    public class StreamController : ControllerBase
    {
 
        [HttpGet]
        [Route("Block")]
        public ActionResult GetBlock()
        {
            return Ok(StreamService.YieldThingies());
        }
 

        [HttpGet]
        [Route("Block2")]
        public ActionResult GetBlock2()
        {
            return Ok(System.Text.Json.JsonSerializer.Serialize(StreamService.YieldThingies()));
        }

        [HttpGet]
        [Route("Block3")]
        public IEnumerable<Thingy> GetBlock3()
        {
            return StreamService.YieldThingies();
        }

        [HttpGet]
        [Route("Stream")]
        public async Task GetStream()
        {

            await using (StreamSerializer writer = new StreamSerializer(Response.Body))
                await writer.WriteAsTokenizedSeries(StreamService.YieldThingies());
        }


        [HttpGet]
        [Route("Stream2")]
        public async Task GetStream2()
        {

            await using (StreamSerializer writer = new StreamSerializer(Response.Body))
                await writer.WriteAllAsync(StreamService.YieldThingies());
        }
        static Random random = new Random();





    }
}
