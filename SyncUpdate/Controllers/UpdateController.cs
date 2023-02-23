using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyncUpdate.Models;
using System;
using System.Linq;

namespace SyncUpdate.Controllers
{
    [Produces("application/json")]
    [Route("api/Update")]
    public partial class UpdateController : Controller
    {
        protected readonly MinecraftContext DB;

        public UpdateController(MinecraftContext context)
        {
            DB = context;
        }
        
        [HttpGet("plugin/{author}/{name}")]
        public async Task<IActionResult> GetPluginInfo([FromRoute] string author, [FromRoute] string name)
        {
            return new ObjectResult(await DB.SyncUpdates.FirstOrDefaultAsync(p => p.Name == name && p.Author == author));
        }

        [HttpGet("plugin/{guid}")]
        public async Task<IActionResult> GetPluginInfo([FromRoute] string guid)
        {
            return new ObjectResult(await DB.SyncUpdates.FirstOrDefaultAsync(p => p.GUID == guid));
        }

        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> SearchPlugin([FromRoute] string keyword)
        {
            var fullLikedKeyword = $"%{keyword}%";
            return new ObjectResult(await DB.SyncUpdates.Where(p =>
                EF.Functions.Like(p.Author, fullLikedKeyword)
                || EF.Functions.Like(p.Name, fullLikedKeyword)
                || EF.Functions.Like(p.Description, fullLikedKeyword)).ToListAsync());
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] string pwd, [FromForm] string name, [FromForm] string author, [FromForm] string desc, [FromForm] string url, [FromForm] string md5, [FromForm] string fileName)
        {
            if (pwd != "ssyynnccppwwdd") return NotFound();
            if(await DB.SyncUpdates.FirstOrDefaultAsync(p=>p.Name == name && p.Author == author) != null)
            {
                return BadRequest();
            }
            await DB.SyncUpdates.AddAsync(new SyncUpdates() {
                Name = name, Author = author,
                DownloadUrl = url, LatestHash = md5,
                GUID = Guid.NewGuid().ToString(),
                Description = desc, FileName = fileName,
            });
            await DB.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update/{guid}")]
        public async Task<IActionResult> Update([FromForm] string pwd, [FromRoute] string guid, [FromForm] string hash, [FromForm] string url, [FromForm] string name, [FromForm] string author, [FromForm] string desc, [FromForm] string fileName)
        {
            if (!CheckPassword("ssyynnccppwwdd")) return NotFound();
            if (await DB.SyncUpdates.FirstOrDefaultAsync(p => p.GUID == guid) is SyncUpdates data)
            {
                if (hash != null) data.LatestHash = hash;
                if (url != null) data.DownloadUrl = url;
                if (name != null) data.Name = name;
                if (author != null) data.Author = author;
                if (desc != null) data.Description = desc;
                if (fileName != null) data.FileName = fileName;
                DB.SyncUpdates.Update(data);
                await DB.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        

        private bool CheckPassword(string pwd, bool allowEmpty = false)
        {
            return (allowEmpty || password.Length != 0)
                && password == pwd;

        }
        /// <summary>
        /// Fetch version data from database and cache in memory
        /// </summary>
        /// <returns></returns>
        [HttpPost("fetchLatest")]
        public async Task<IActionResult> UpdateSyncVersion([FromForm] string pwd)
        {
            if (!CheckPassword(pwd, true)) return NotFound();
            await UpdateVersion();
            return Ok();
        }

        private static readonly Models.Version Ver = new Models.Version();
        private static string password = string.Empty;

        private async Task<string> loadVar(int id) {
            return (await DB.SyncVars.FirstOrDefaultAsync(p => p.Id == id)).Value;
        }

        private async Task UpdateVersion()
        {
            Ver.VersionId = await loadVar(1);
            Ver.VersionHash = await loadVar(2);
            Ver.DownloadURL = await loadVar(3);
            password = await loadVar(4);
            System.Console.WriteLine($"assword {password} loaded");
        }

        /// <summary>
        /// Get cached version data from database
        /// </summary>
        /// <returns></returns>
        [HttpGet("latest")]
        public ActionResult<Models.Version> LatestSync()
        {
            return Ok(Ver);
        }
    }
}
