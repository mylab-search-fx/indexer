using System.Linq;
using MyLab.Search.EsAdapter.Indexing;
using Nest;
using Newtonsoft.Json.Linq;

namespace MyLab.Search.Indexer.Models
{
    public class IndexingRequest
    {
        public string IndexId { get; set; }
        public JObject[] PostList { get; set; }
        public JObject[] PutList { get; set; }
        public JObject[] PatchList { get; set; }
        public string[] DeleteList { get; set; }

        public IndexingRequest Clone()
        {
            return new IndexingRequest
            {
                PostList = PostList,
                PutList = PutList,
                DeleteList = DeleteList,
                PatchList = PatchList,
                IndexId = IndexId
            };
        }

        public bool IsEmpty()
        {
            return PostList is not { Length: > 0 } && 
                   PutList is not { Length: > 0 } &&
                   PatchList is not { Length: > 0 } && 
                   DeleteList is not { Length: > 0 };
        }
    }
}