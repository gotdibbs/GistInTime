using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GistInTime.Model
{
    public class GistsResponse
    {
        public string url { get; set; }
        public string forks_url { get; set; }
        public string commits_url { get; set; }
        public string id { get; set; }
        public string git_pull_url { get; set; }
        public string git_push_url { get; set; }
        public string html_url { get; set; }
        public Dictionary<string, File> files { get; set; }
        public bool @public { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string description { get; set; }
        public int comments { get; set; }
        public User user { get; set; }
        public User owner { get; set; }
        public string comments_url { get; set; }

        [IgnoreDataMember]
        public DateTime updated_at_display
        {
            get
            {
                return DateTime.Parse(updated_at);
            }
            set { }
        }

        [IgnoreDataMember]
        public string first_file_name
        {
            get
            {
                var firstFile = files.FirstOrDefault();
                if (!string.IsNullOrEmpty(firstFile.Key))
                {
                    return firstFile.Value.filename;
                }
                else
                {
                    return "--";
                }
            }
            set { }
        }
    }
}
