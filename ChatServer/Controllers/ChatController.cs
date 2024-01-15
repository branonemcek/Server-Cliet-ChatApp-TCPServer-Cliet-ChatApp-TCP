using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatLogApi.Models;

namespace ChatLogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ChatLogApiController : ControllerBase
    {
        private readonly ChatDbContext _context;

        public ChatLogApiController(ChatDbContext context)
        {
            _context = context;
        }

         // GET: api/ConnectionLog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectionLog>>> GetConnectionLog()
        {
            return await _context.ConnectionLogs.ToListAsync();
        }

        // GET: api/ConnectionLog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectionLog>> GetConnectionLog(long id)
        {
            var todoItem = await _context.ConnectionLogs.FindAsync(id);

            if (todoItem == null) {
                return NotFound();
            }

            return todoItem;
        }

        // PUT: api/ConnectionLog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, ConnectionLog log)
        {
            if (id != log.Id) {
                return BadRequest();
            }

            _context.Entry(log).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) {
                if (!ExistsConnectionLogs(id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }
         private bool ExistsConnectionLogs(long id)
        {
            return _context.ConnectionLogs.Any(e => e.Id == id);
        }
    }
}