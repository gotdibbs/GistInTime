using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GistInTime.Model
{
    public class AuthorizationResponse
    {
        public int id { get; set; }
        public string url { get; set; }
        public List<string> scopes { get; set; }
        public string token { get; set; }
        public App app { get; set; }
        public string note { get; set; }
        public string note_url { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }
    }
}
