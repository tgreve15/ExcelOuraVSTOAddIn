using System;
using System.Net;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace OuraAPIInterface
{

    /// <summary>
    /// Wrapper class to handle all calls to the Oura API.
    /// </summary>
    static public class OuraAPIWrapper
    {
        private static string ouraToken;

        //private static void OnError(ErrorEventArgs e)
        //{
        //    //Console.WriteLine(args.ErrorContext.Error.Message);
        //    //args.ErrorContext.Handled = true;
        //}

        public static string APIToken()
        {
            if (String.IsNullOrEmpty(ouraToken))
            {
                ouraToken = ConfigurationManager.AppSettings["OuraApiKey"];
            }
            return ouraToken;
        }

        public static void APIToken(string token)
        {
            ouraToken = token;
        }

        public static string BaseURL()
        {
            return "https://api.ouraring.com";
        }

        public static UserInfoResponse PerformAuthentication(string apiToken)
        {
            string urlRequest = BaseURL() + "/v1/userinfo?access_token=" + apiToken;
            return MakeRequest<UserInfoResponse>(urlRequest);
        }

        public static UserInfoResponse PerformAuthentication()
        {
            string urlRequest = BaseURL() + "/v1/userinfo?access_token=" + APIToken();
            return MakeRequest<UserInfoResponse>(urlRequest);
        }

        public static SleepSummaryResponse PerformSleepSummaryRequest(DateTime startDate, DateTime endDate)
        {
            // https://api.ouraring.com/v1/sleep?start=2021-08-30&end=2021-09-01
            string urlRequest = BaseURL() + "/v1/sleep?start=" + startDate.ToString("yyyy-MM-dd") + "&end=" + endDate.ToString("yyyy-MM-dd");
            return MakeRequest<SleepSummaryResponse>(urlRequest);
        }

        public static ActivitySummaryResponse PerformActivitySummaryRequest(DateTime startDate, DateTime endDate)
        {
            // https://api.ouraring.com/v1/activity?start=2021-08-30&end=2021-09-01
            string urlRequest = BaseURL() + "/v1/activity?start=" + startDate.ToString("yyyy-MM-dd") + "&end=" + endDate.ToString("yyyy-MM-dd");
            return MakeRequest<ActivitySummaryResponse>(urlRequest);
        }

        public static ReadinessSummaryResponse PerformReadinessSummaryRequest(DateTime startDate, DateTime endDate)
        {
            // https://api.ouraring.com/v1/readiness?start=2021-08-30&end=2021-09-01
            string urlRequest = BaseURL() + "/v1/readiness?start=" + startDate.ToString("yyyy-MM-dd") + "&end=" + endDate.ToString("yyyy-MM-dd");
            return MakeRequest<ReadinessSummaryResponse>(urlRequest);
        }

        /// <summary>
        /// Perform the API request passed in and return the result as an object of type passed in
        /// </summary>
        /// <typeparam name="T">Type of the return object</typeparam>
        /// <param name="requestUrl">API call to make</param>
        /// <returns>Object of requested type or the default for that object type</returns>
        public static T MakeRequest<T>(string requestUrl)
        {
            try
            {
                //1. HTTP header: Include the Personal Access Token in the Authorization header like this:
                //    GET /v1/userinfo HTTP/1.1
                //    Host: api.ouraring.com
                //    Authorization: Bearer PHCW3OVMXQZX5FJUR6ZK4FAA2MK2CWWA
                //2. URL parameter: Add access_token parameter to the API call:
                //    https://api.ouraring.com/v1/userinfo?access_token=PHCW3OVMXQZX5FJUR6ZK4FAA2MK2CWWA

                // The following HAS TO BE Before the WebRequest creation
                // Although not in Console app ... go figure
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                request.Headers.Add("Authorization", "Bearer " + APIToken());
                request.Host = "api.ouraring.com";      // For some reason need this format for this one (unless command prompt interface)
                //request.Headers.Add("Host", "api.ouraring.com");

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));

                    // get response as a string and convert JSON to a real object
                    String jsonResponse;
                    using (StreamReader Reader = new StreamReader(response.GetResponseStream()))
                    {
                        jsonResponse = Reader.ReadToEnd();
                    }

                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    // The following will only trigger when running through the debugger
                    // and will test if any additional attributes are returned in the API
                    // that aren't currently handled
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        settings.MissingMemberHandling = MissingMemberHandling.Error;
                    }
                    T objectResult = JsonConvert.DeserializeObject<T>(jsonResponse,settings);
                    return objectResult;
                }
            }
            catch(Newtonsoft.Json.JsonSerializationException jex)
            {
                // 'Could not find member 'score_total' on object of type 'SleepResponse'. Path 'sleep[0].score_total', line 1, position 1640.'
                MessageBox.Show(String.Format("--== Oura API Extended ==--. {0}", jex.Message));
                return default(T);
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("An error occurred while attempting to retrieve your data. Error: {0}", e.Message));
                return default(T);
            }
        }



        /// <summary>
        /// Perform the API request passed in and return the result as a JSON string.
        /// Specifically created to support troubleshooting
        /// </summary>
        /// <param name="requestUrl">API call to make</param>
        /// <returns>JSON string results of API call made OR null</returns>
        public static String MakeRequestString(string requestUrl)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                request.Headers.Add("Authorization", "Bearer " + APIToken());
                request.Host = "api.ouraring.com";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    String jsonResponse;
                    using (StreamReader Reader = new StreamReader(response.GetResponseStream()))
                    {
                        jsonResponse = Reader.ReadToEnd();
                    }
                    return jsonResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static List<OuraCombinedObject> GetAllOuraDataForDateRange(DateTime startDate, DateTime endDate)
        {
            // Request data from Oura for the selected date range
            SleepSummaryResponse sleepResponse = PerformSleepSummaryRequest(startDate, endDate);
            ActivitySummaryResponse activityResponse = PerformActivitySummaryRequest(startDate, endDate);
            ReadinessSummaryResponse readinessResponse = PerformReadinessSummaryRequest(startDate, endDate);

            // Consolidate all the Oura Data into a single object so we can expose data across what is collected side by side
            List<OuraCombinedObject> ouraObjects = new List<OuraCombinedObject>();

            // If one or more of the service requests didn't work, just quit as there is something bigger going wrong
            if (activityResponse != null && readinessResponse != null && sleepResponse != null)
            {
                // Every day will have some amount of activity information, even if just woke up,
                // but there may not be sleep and readiness data. As such there will likely be a day
                // with only activity data.

                // The OURA service now returns all data between the requested dates, but if data in the middle is missing
                // potentially due to the ring running out of charge or not being worn for a few days this can
                // relate to days with nothing, and some with only some activity data.
                // Ensure we always get the right data for the right days and return an empty object for a date
                // if nothing for that date.
                DateTime loopDate = startDate;
                while (loopDate <= endDate)
                {
                    string loopDateString = loopDate.ToString("yyyy-MM-dd");

                    SleepResponse sleep = null;
                    if (sleepResponse != null && sleepResponse.Sleep != null)
                        sleep = sleepResponse.Sleep.FirstOrDefault(s => s.SummaryDate == loopDateString);

                    ActivityResponse activity = null;
                    if (activityResponse != null && activityResponse.Activity != null)
                        activity = activityResponse.Activity.FirstOrDefault(s => s.SummaryDate == loopDateString);

                    ReadinessResponse readiness = null;
                    if (readinessResponse != null && readinessResponse.Readiness != null)
                        readiness = readinessResponse.Readiness.FirstOrDefault(s => s.SummaryDate == loopDateString);

                    //Create a combined object with all facets we received
                    OuraCombinedObject oObj = new OuraCombinedObject(loopDateString);
                    oObj.UpdateFrom(sleep, readiness, activity);
                    ouraObjects.Add(oObj);

                    // add one day to the counter
                    loopDate = loopDate.AddDays(1);
                }

            }
            return ouraObjects;
        }
    }
}

