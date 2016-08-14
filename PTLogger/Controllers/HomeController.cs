using Microsoft.Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PTLogger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PTLogger.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(UserStoryModel model, IEnumerable<HttpPostedFileBase> files)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

            String json = JsonConvert.SerializeObject(model, Formatting.Indented, jsonSerializerSettings);

            var pivotalUrl = GetUri();

            var result = GetClient().PostAsync(pivotalUrl, new StringContent(json, Encoding.UTF8, "application/json")).Result;

            if (result.IsSuccessStatusCode)
            {
               if (files.Count() > 0)
                {

                    var fileClient = new HttpClient();
                    fileClient.DefaultRequestHeaders.Add("X-TrackerToken", CloudConfigurationManager.GetSetting("Token"));

                    foreach (var file in files)
                    {
                        MultipartFormDataContent form = new MultipartFormDataContent();
                        HttpContent content = new StringContent(file.FileName);
                        form.Add(content, file.FileName);
                        var stream = file.InputStream;
                        content = new StreamContent(stream);
                        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "fileToUpload",
                            FileName = file.FileName
                        };
                        form.Add(content);
                        var imageUrl = String.Format("https://www.pivotaltracker.com/services/v5/projects/{0}/uploads", CloudConfigurationManager.GetSetting("ProjectId"));
                        var response = fileClient.PostAsync(imageUrl, form).Result;

                        //Attach file to story
                    }
                }

                return View();
            }

            return View();
        }

        protected String GetUri()
        {
            return 
                    String.Format(
                        "https://www.pivotaltracker.com/services/v5/projects/{0}/stories",
                        CloudConfigurationManager.GetSetting("ProjectId")
                        
                );
        }

        private HttpClient _client;

        protected HttpClient GetClient()
        {
            if (_client == null)
            {
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Add("X-TrackerToken", CloudConfigurationManager.GetSetting("Token"));
                _client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return _client;
        }
    }
}