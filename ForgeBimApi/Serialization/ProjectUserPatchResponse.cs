using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.Forge.BIM360.Serialization
{
    public class ProjectUserPatchResponse
    {
        public string email;
        public string company_id;
        public string[] industry_roles;
        public Services services;
        public string user_id;
        public string project_id;
        public string account_id;
        public ResponseContent[] error;
    }
}
