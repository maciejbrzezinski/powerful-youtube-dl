using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace powerful_youtube_dl.thinkingPart
{
   public class JSONHandler
    {
        public static Dictionary<string, object> getValues(string json)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(json);
            Dictionary<string, object> obj2 = (Dictionary<string, object>)(result);

            foreach(object position in obj2)
            {

            }

            System.Object[] val = (System.Object[])obj2["items"];
            string response = null;
            try
            {
                Dictionary<string, object> id = (Dictionary<string, object>)val.GetValue(0);
                response = id[""].ToString();
            }
            finally { }
            return obj2;
        }
    }
}
