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
        static async Task Main (string[] args)
        {
            await getResponse();
        }

          static async Task getResponse()
        {
            HttpClient c = new HttpClient();
            var url = "https://www.claim.md/services/response/";
            var accountKey = "accountkey";
            // look for a file, if there then read value
            // responseid = 0 means start over         
            var filepath = @"c:\rlswork\getResponse\responseid.txt" ;



            var accountKeyContent = new StringContent(accountKey);
            accountKeyContent.Headers.Remove("Content-Type");
            accountKeyContent.Headers.Add("Content-Disposition", "form-data; name=\"AccountKey\"");
            string responseid = "0";
            StreamReader file;
            if (File.Exists(filepath)) 
            {
                file = new StreamReader(filepath);
                responseid = file.ReadLine();
                int number; 
                bool success = Int32.TryParse(responseid,out number);
                if (!success)
                {
                    responseid = "0";
                }
                file.Close();
            }
        
            var responseIdContent = new StringContent(responseid);
            responseIdContent.Headers.Remove("Content-Type");
            responseIdContent.Headers.Add("Content-Disposition", "form-data; name=\"ResponseID\"");

            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(accountKeyContent);
            content.Add(responseIdContent);
            using (var response = await c.PostAsync(url, content))
            {
                string resultString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("-----");
                Console.WriteLine(resultString);
                Console.WriteLine("-----");
                Result result = new Result();
                Claim curClaim = null;
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
                                result.LastResponseId = xmlReader.GetAttribute("last_responseid");
                            }
                        }

                        if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "claim"))
                        {
                            if (xmlReader.HasAttributes)
                            {
                                curClaim = new Claim();
                                result.Claims.Add(curClaim);
                                curClaim.BatchId = xmlReader.GetAttribute("batchid");
                                curClaim.ClaimId = xmlReader.GetAttribute("claimid");
                                curClaim.ClaimMDId = xmlReader.GetAttribute("claimmd_id");
                                curClaim.BillNpi = xmlReader.GetAttribute("bill_npi");
                                curClaim.BillTaxId = xmlReader.GetAttribute("bill_taxid");
                                curClaim.Fdos = xmlReader.GetAttribute("fdos");
                                curClaim.Fileid = xmlReader.GetAttribute("fileid");
                                curClaim.Filename = xmlReader.GetAttribute("filename");
                                curClaim.InsNumber = xmlReader.GetAttribute("ins_number");
                                curClaim.PayerId = xmlReader.GetAttribute("claimmd_id");
                                curClaim.Pcn = xmlReader.GetAttribute("pcn");
                                curClaim.SenderIcn = xmlReader.GetAttribute("sender_icn");
                                curClaim.SenderName = xmlReader.GetAttribute("sender_name");
                                curClaim.SenderId = xmlReader.GetAttribute("sender_id");
                                curClaim.Status = xmlReader.GetAttribute("status");
                                curClaim.TotalCharge = Double.Parse(xmlReader.GetAttribute("total_charge"));
                            };
                        }

                        if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "messages"))
                        {
                            if (xmlReader.HasAttributes)
                            {
                                MessageType message = new MessageType();
                                message.Fields = xmlReader.GetAttribute("fields");
                                message.ResponseId = xmlReader.GetAttribute("RepsonseId");
                                curClaim.Messages.Add(message);
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
                    }
                }

                // write the response id to the filepath
                if (result.LastResponseId != null)
                {
                    StreamWriter writer = new StreamWriter(filepath);
                    writer.WriteLine(result.LastResponseId);
                    writer.Close();
                }
                

                string json = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }

    public class MessageType
    {
        public string Fields { get; set; }
        public string MesgId { get; set; }
        public string Message { get; set; }
        public string ResponseId {get; set;}
        public string Status { get; set; }
    }
    public class Claim
    {
        public string ClaimId { get; set; }
        public string ClaimMDId { get; set; }
        public string BatchId { get; set; }

        public string BillNpi {get; set;}

        public string BillTaxId {get; set;}

        public string Fdos {get; set;}

        public string Fileid {get; set;}

        public string Filename {get; set;}

        public string InsNumber {get; set;}

        public string PayerId {get; set;}

        public string Pcn {get; set;}

        public string SenderIcn {get; set;}

        public string SenderName {get; set;}

        public string SenderId {get; set;}

        public string Status {get; set;}

        public double TotalCharge {get; set;}

        public List<MessageType> Messages = new List<MessageType>();
    }

    public class Error
    {
        public string ErrorCode { get; set; }
        public string Error_Mesg { get; set; }
    }

    public class Result
    {
        public string Messages { get; set; }
        public List<Claim> Claims = new List<Claim>();
        public Error Error { get; set; }

        public string LastResponseId {get; set;}
    }
}

