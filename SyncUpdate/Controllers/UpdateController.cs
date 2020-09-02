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
        public IActionResult GetPluginInfo([FromRoute] string author, [FromRoute] string name)
        {
            return new ObjectResult(DB.SyncUpdates.FirstOrDefault(p => p.Name == name && p.Author == author));
        }

        [HttpGet("plugin/{guid}")]
        public IActionResult GetPluginInfo([FromRoute] string guid)
        {
            return new ObjectResult(DB.SyncUpdates.FirstOrDefault(p => p.GUID == guid));
        }

        [HttpGet("search/{keyword}")]
        public IActionResult SearchPlugin([FromRoute] string keyword)
        {
            var fullLikedKeyword = $"%{keyword}%";
            return new ObjectResult(DB.SyncUpdates.Where(p =>
                EF.Functions.Like(p.Author, fullLikedKeyword)
                || EF.Functions.Like(p.Name, fullLikedKeyword)
                || EF.Functions.Like(p.Description, fullLikedKeyword)));
        }

        [HttpPost("add")]
        public IActionResult Add([FromForm] string pwd, [FromForm] string name, [FromForm] string author, [FromForm] string desc, [FromForm] string url, [FromForm] string md5, [FromForm] string fileName)
        {
            if (pwd != "ssyynnccppwwdd") return NotFound();
            if(DB.SyncUpdates.FirstOrDefault(p=>p.Name == name && p.Author == author) != null)
            {
                return BadRequest();
            }
            DB.SyncUpdates.Add(new SyncUpdates() { Name = name, Author = author, DownloadUrl = url, LatestHash = md5, GUID = Guid.NewGuid().ToString(), Description = desc, FileName = fileName });
            DB.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("update/{guid}")]
        public IActionResult Update([FromForm] string pwd, [FromRoute] string guid, [FromForm] string hash, [FromForm] string url, [FromForm] string name, [FromForm] string author, [FromForm] string desc, [FromForm] string fileName)
        {
            if(pwd != "ssyynnccppwwdd") return NotFound();
            if (DB.SyncUpdates.FirstOrDefault(p=>p.GUID == guid) is SyncUpdates data)
            {
                if (hash != null) data.LatestHash = hash;
                if (url != null) data.DownloadUrl = url;
                if (name != null) data.Name = name;
                if (author != null) data.Author = author;
                if (desc != null) data.Description = desc;
                if (fileName != null) data.FileName = fileName;
                DB.SyncUpdates.Update(data);
                DB.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        

        private bool CheckPassword(string pwd)
        {
            return pwd == "ssyynnccppwwdd";

        }
        /// <summary>
        /// Fetch version data from database and cache in memory
        /// </summary>
        /// <returns></returns>
        [HttpPost("fetchLatest")]
        public IActionResult UpdateSyncVersion([FromForm] string pwd)
        {
            if (CheckPassword(pwd)) return NotFound();
            UpdateVersion();
            return Ok();
        }

        private static readonly Models.Version Ver = new Models.Version();
        private void UpdateVersion()
        {
            Ver.VersionId = DB.SyncVars.Where(p => p.Id == 1).FirstOrDefault().Value;
            Ver.VersionHash = DB.SyncVars.Where(p => p.Id == 2).FirstOrDefault().Value;
            Ver.DownloadURL = DB.SyncVars.Where(p => p.Id == 3).FirstOrDefault().Value;

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
