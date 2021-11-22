using System;
using System.Net;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace OuraAPIInterface
{
    /// <summary>
    /// Wrapper class to handle all calls to the Oura API.
    /// </summary>
    static public class OuraAPIWrapper
    {
        private static string ouraToken;

        public static string APIToken()
        {
            if (String.IsNullOrEmpty(ouraToken))
            {
                ouraToken = ConfigurationManager.AppSettings["OuraApiKey"];
            }
            return ouraToken;
        }

        public static string BaseURL()
        {
            return "https://api.ouraring.com";
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
                    T objectResult = JsonConvert.DeserializeObject<T>(jsonResponse);
                    return objectResult;
                }
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
    }
}

