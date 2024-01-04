using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using NewsFlowAPI.Models;
using NewsFlowAPI.Services;
using StackExchange.Redis;

namespace NewsFlowAPI.Controllers
{
    [ApiController]
    [Route("tag")]
    public class TagController: Controller
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;

        public TagController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids
            )
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
        }

        //[Authorize]
        [HttpPost("create/{name}")]
        public async Task<ActionResult> CreateTag([FromRoute]string  name)
        {
            var newTag = new Tag
            {
                Id = await _ids.TagNext(),
                Name = name
            };

            await _neo4j.Cypher
                .Create("(t:Tag $tag)")
                .WithParam("tag", newTag)
                .ExecuteWithoutResultsAsync();

            return Ok("Tag successfully added!");
        }
        //[Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteTag([FromRoute] long id)
        {
            var tag =await  _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .Return(t => new
                {
                    t.As<Tag>().Id,
                    t.As<Tag>().Name
                })
                .Limit(1)
                .ResultsAsync;

            if (tag.Count()==0)
            {
                return NotFound($"Tag with Id:{id} not found");
            }

            await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .DetachDelete("t")
                .ExecuteWithoutResultsAsync();

            return Ok("Tag deleted");

        }
        //[Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateTag([FromRoute] long id, [FromQuery] string name)
        {
            var tag = await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t) => t.Id == id)
                .Return(t => new
                {
                    t.As<Tag>().Id,
                    t.As<Tag>().Name
                })
                .Limit(1)
                .ResultsAsync;

            if (tag.Count() == 0)
            {
                return NotFound($"Tag with Id:{id} not found");
            }
            await _neo4j.Cypher
                .Match("(t:Tag)")
                .Where((Tag t)=>t.Id==id)
                .Set("t.Name=$name")
                .WithParam("name", name)
                .ExecuteWithoutResultsAsync();

            return Ok("Tag updated");
        }

        //[Authorize(Roles ="writer")]
        [HttpGet("get/{id}")]
        public async Task<ActionResult> GetTag([FromRoute] long id)
        {
            var tag = await _neo4j.Cypher
               .Match("(t:Tag)")
               .Where((Tag t) => t.Id == id)
               .Return(t => new 
               {
                   t.As<Tag>().Id,
                   t.As<Tag>().Name
               })
               .ResultsAsync;

            if (tag.Count()==0)
            {
                return NotFound($"Tag with Id:{id} not found");
            }
            return Ok(tag.ToList()[0].Name);
        }
    }
}
