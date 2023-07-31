using System.Diagnostics.Eventing.Reader;
using System.Web.Http;
using System.Security.AccessControl;
using System.IO;
using System.Web.Http.Results;
using Microsoft.AspNetCore.Mvc;

namespace winlogApi.Controllers
{
    public class EventLogController : ApiController
    {

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("api/eventlog")]
        
        public IHttpActionResult GetEventLogData(string log_name)
        {

            string logPath = $@"C:\Windows\System32\winevt\Logs\{log_name}.evtx";
                string jsonString = ConvertEventLogToJson(logPath);

                return Ok(jsonString);
            
          
        }

        private static string ConvertEventLogToJson(string logPath)
        {
            string json = "";
            EventLogReader eventLogReader;
            var events = new List<Dictionary<string, object>>();

            if (File.Exists(logPath))
            {
                try
                {
                    using (eventLogReader = new EventLogReader(logPath, PathType.FilePath))
                    {
                        for (EventRecord eventInstance = eventLogReader.ReadEvent(); eventInstance != null; eventInstance = eventLogReader.ReadEvent())
                        {
                            var eventData = new Dictionary<string, object>();

                            try{eventData.Add("TaskName", eventInstance.TaskDisplayName);}catch(EventLogNotFoundException)
                            {
                                eventData.Add("TaskName", null);
                            }
                            try{eventData.Add("EventId", eventInstance.Id);}catch(EventLogNotFoundException)
                            {
                                eventData.Add("EventId", null);
                            }
                            try{eventData.Add("Level", eventInstance.LevelDisplayName);}catch(EventLogNotFoundException)
                            {
                                eventData.Add("Level", null);
                            }
                            try{eventData.Add("Task", eventInstance.TaskDisplayName);} catch (EventLogNotFoundException) 
                            {
                                eventData.Add("Task", null);
                            }
                            try{ eventData.Add("ProviderName", eventInstance.ProviderName);} catch (EventLogNotFoundException) 
                            {
                                eventData.Add("ProviderName",null);
                            }
                            try{eventData.Add("LogName", eventInstance.LogName);}catch(EventLogNotFoundException) 
                            {
                                eventData.Add("LogName", null);
                            }
                            try {eventData.Add("MachineName", eventInstance.MachineName);} catch (EventLogNotFoundException)
                            {
                                eventData.Add("MachineName", null);
                            }
                            try{eventData.Add("Message", eventInstance.FormatDescription());} catch (EventLogNotFoundException)
                            {
                                eventData.Add("Message", null);
                            }
                            try{eventData.Add("TimeCreated", eventInstance.TimeCreated);} catch (EventLogNotFoundException) 
                            {
                                eventData.Add("TimeCreated", null);
                            }
                            events.Add(eventData);
                        }
                    }

                    json = Newtonsoft.Json.JsonConvert.SerializeObject(events);
                }
                catch (UnauthorizedAccessException ex)
                {
                    FileInfo file_info = new FileInfo(logPath);

                    FileSecurity fileSecurity =  file_info.GetAccessControl();

                    FileSystemAccessRule accessRule = new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow);
                    fileSecurity.AddAccessRule(accessRule);
                    file_info.SetAccessControl(fileSecurity);
                   
                }
            }
            return json;
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

        }

    }
}