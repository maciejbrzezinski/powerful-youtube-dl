using System;
using System.IO;
using System.Net;

namespace powerful_youtube_dl.web {

    public class Http {

        public string Get(string uri) {
            uri = uri.Trim();
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
                if (stream != null)
                    using (StreamReader reader = new StreamReader(stream)) {
                        return reader.ReadToEnd();
                    }
            return String.Empty;
        }
    }
}