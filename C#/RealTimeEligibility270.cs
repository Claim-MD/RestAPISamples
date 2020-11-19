using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace HttpClientPost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Result result = await send_eligibility_request(@"C:\Users\13365\Downloads\sample.270","sample.270", "accountkey");
            string json = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(json);
        }
        static async Task<Result> send_eligibility_request(string path, string filename,string accountkey)
        {
            string url = "https://www.claim.md/services/elig/";
            HttpClient c = new HttpClient();
            byte[] file_bytes = File.ReadAllBytes(path);

            // build request
            var accountKeyContent = new StringContent(accountkey);
            accountKeyContent.Headers.Add("Content-Disposition", "form-data; name=\"AccountKey\"");

            var fileContent = new ByteArrayContent(file_bytes);
            fileContent.Headers.Add("Content-Disposition", $"form-data; name=\"File\"; filename=\"{filename}\"");
            fileContent.Headers.Add("Content-Type", "text/plain");

            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(accountKeyContent);
            content.Add(fileContent);
            using (var response = await c.PostAsync(url, content))
            {
                {
                    //Console.WriteLine("waiting on response");
                    string resultString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("-----");
                    Console.WriteLine(resultString);
                    Console.WriteLine("-----");
                    Result result = new Result();
                      using (StringReader sr = new StringReader(resultString))
                    {
                        XmlTextReader xmlReader = new XmlTextReader(sr);
                        while (xmlReader.Read())
                        {
                            if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "result"))
                            {
                                if (xmlReader.HasAttributes)
                                {
                                    //Console.WriteLine(xmlReader.GetAttribute("messages"));  
                                    Console.WriteLine("found result");
                                    result.Messages = xmlReader.GetAttribute("messages");
                                }
                            }

                            if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "error"))
                            {
                                if (xmlReader.HasAttributes)
                                {
                                    result.Error = new Error();
                                    result.Error.ErrorCode = xmlReader.GetAttribute("error_code");
                                    result.Error.Error_Mesg = xmlReader.GetAttribute("error_mesg");
                                }
                            }

                            if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "eligibility"))
                            {
                                if (xmlReader.HasAttributes)
                                {
                                    result.Eligibility = new Eligibility();
                                    result.Eligibility.Data = xmlReader.GetAttribute("data");
                                    result.Eligibility.EligId = xmlReader.GetAttribute("eligid");
                                }
                            }
                        }
                        return result;

                    }
                }
            }
        }

        public class MessageType
        {
            public string Fields { get; set; }
            public string MesgId { get; set; }
            public string Message { get; set; }
            public string Status { get; set; }
        }

        public class Error
        {
            public string ErrorCode { get; set; }
            public string Error_Mesg { get; set; }
        }

        public class Eligibility
        {
            public string Data { get; set; }
            public string EligId { get; set; }
        }

        public class Result
        {
            public string Messages { get; set; }
            public Error Error { get; set; }

            public Eligibility Eligibility { get; set; }
        }
    }
}
