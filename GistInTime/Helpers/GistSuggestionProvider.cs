using GistInTime.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfControls;

namespace GistInTime.Helpers
{
    public class GistSuggestionProvider : ISuggestionProvider
    {
        public IEnumerable GetSuggestions(string filter)
        {
            filter = filter.ToLower();

            var results = App.Gists
                .Where(x => x.description.ToLower().Contains(filter) || 
                    x.first_file_name.ToLower().Contains(filter) ||
                    x.user.login.Contains(filter))
                .OrderBy(x => x.description);

            return results;
        }
    }
}
