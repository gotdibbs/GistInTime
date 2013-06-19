using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GistInTime.Model
{
    public class AuthorizationRequest
    {
        public List<string> scopes { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }
}
