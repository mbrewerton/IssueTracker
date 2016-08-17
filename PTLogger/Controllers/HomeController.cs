using Microsoft.Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PTLogger.App_Start;
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
        private int _projectId = int.Parse(CloudConfigurationManager.GetSetting("ProjectId"));

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
        public async Task<ActionResult> Create(NewUserStoryModel model, IEnumerable<HttpPostedFileBase> files)
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
                        if (file != null)
                        {
                            MultipartFormDataContent form = new MultipartFormDataContent();
                            // Create a new MemoryStream to enable us to create a byte[]
                            MemoryStream target = new MemoryStream();
                            // Copy our file Stream into our MemoryStream
                            file.InputStream.CopyTo(target);
                            // Convert our Stream to a byte[] to attach to our request
                            byte[] data = target.ToArray();
                            // Setup content of our request as a ByteArrayContent using our byte[] data
                            var content = new ByteArrayContent(data);

                            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "file",
                                FileName = file.FileName
                            };
                            form.Add(content);
                            var imageUrl = String.Format("https://www.pivotaltracker.com/services/v5/projects/{0}/uploads", _projectId);
                            var response = fileClient.PostAsync(imageUrl, form).Result;

                            // Only continue if the file was uploaded successfully
                            if (response.IsSuccessStatusCode)
                            {
                                // Deserialise the file upload json into a C# object
                                var savedAttachment = JsonConvert.DeserializeObject<AttachmentModel>(await response.Content.ReadAsStringAsync());
                                // Deserialise our newly created story. This is because we need a different model as there are slight differences in the json
                                var newUserStory = JsonConvert.DeserializeObject<UserStoryModel>(await result.Content.ReadAsStringAsync());

                                // @John: Is this redundant? Could we use the original fileClient and just change the settings?
                                using (var commentClient = new HttpClient())
                                {
                                    // Create a new List<AttachmentModel> and add our uploaded file
                                    var attachments = new List<AttachmentModel>();
                                    attachments.Add(savedAttachment);

                                    // Setup our comment data to be converted to json
                                    var comment = new NewCommentModel
                                    {
                                        Project_Id = _projectId,
                                        Story_Id = newUserStory.Id,
                                        Text = "File Upload.", // @john: Do we make this dynamic and let the user supply a comment?
                                        File_Attachments = new List<AttachmentModel> { savedAttachment }
                                    };

                                    // Serialise our comment into Json using a LowerCaseContractResolver. The endpoint doesn't accept camelCase.
                                    var commentJson = JsonConvert.SerializeObject(comment, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new LowerCaseContractResolver() });
                                    commentClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    commentClient.DefaultRequestHeaders.Add("X-TrackerToken", CloudConfigurationManager.GetSetting("Token"));
                                    var commentResult = 
                                        commentClient.PostAsync(String.Format("https://www.pivotaltracker.com/services/v5/projects/{0}/stories/{1}/comments",
                                        _projectId, newUserStory.Id), new StringContent(commentJson, Encoding.UTF8, "application/json")).Result;

                                    // Left this in for any debug purposes. It grabs the response message from the server to view errors.
                                    var commentResultAsString = await commentResult.Content.ReadAsStringAsync();
                                }
                            }
                        }
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
                        _projectId

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