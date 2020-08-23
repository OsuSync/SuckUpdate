using System;
using System.Collections.Generic;

namespace SyncUpdate.Models
{
    public partial class SyncUpdates
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string LatestHash { get; set; }
        public string DownloadUrl { get; set; }
        public string Description { get; set; }
        public string GUID { get; set; }
        public string FileName { get; set; }
    }
}
