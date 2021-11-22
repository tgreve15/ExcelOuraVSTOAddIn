using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OuraAPIInterface;
using System.Reflection;

namespace OuraAPITestLine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            UserInfoResponse response = OuraAPIWrapper.PerformAuthentication();
            Console.WriteLine("Oura Response: Age {0} Weight: {1} Height: {2} Gender: {3} Email: {4}", response.Age, response.Weight, response.Height, response.Gender, response.Email);

            SleepSummaryResponse sleepResponse = OuraAPIWrapper.PerformSleepSummaryRequest((DateTime.Today).AddDays(-2), DateTime.Today);
            
            Console.WriteLine("---=== Sleep ===---");
            //if (sleepResponse is null)
            //{
            //    Console.WriteLine("****************** sleep response failed **************");
            //}
            //else
            //{
            //    StreamWriter st = new StreamWriter("c:\\temp\\ouraout.txt");

            //    foreach (SleepResponse resp in sleepResponse.Sleep)
            //    {
            //        Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", resp.SummaryDate, resp.Deep, resp.REM, resp.Light, resp.Awake, resp.Total, "", resp.BedtimeStart, resp.BedtimeEnd, "", 0);
            //        st.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", resp.SummaryDate, resp.Deep, resp.REM, resp.Light, resp.Awake, resp.Total, "", resp.BedtimeStart, resp.BedtimeEnd, "", 0);
            //    }
            //    st.Close();
            //}
            
            ActivitySummaryResponse activityResponse = OuraAPIWrapper.PerformActivitySummaryRequest((DateTime.Today).AddDays(-2), DateTime.Today);
            Console.WriteLine("---=== Activity ===---");
            //if (activityResponse is null)
            //{
            //    Console.WriteLine("****************** activity response failed **************");
            //}
            //else
            //{
            //    foreach (ActivityResponse act in activityResponse.Activity)
            //    {
            //        Console.WriteLine("{0}", act.Steps);
            //    }
            //}
            
            ReadinessSummaryResponse readinessResponse = OuraAPIWrapper.PerformReadinessSummaryRequest((DateTime.Today).AddDays(-2), DateTime.Today);
            //if (readinessResponse is null)
            //{
            //    Console.WriteLine("****************** readiness response failed **************");
            //}
            //else
            //{
            //    foreach (ReadinessResponse read in readinessResponse.Readiness)
            //    {
            //        Console.WriteLine("Readiness: {0},{1}", read.SummaryDate, read.ScoreTemperature);
            //    }
            //}

            Console.WriteLine("Finished");

            List<OuraCombinedObject> ouraObjects = new List<OuraCombinedObject>();
            if (activityResponse != null && readinessResponse != null && sleepResponse != null)
            {
                for (int i = 0; i < sleepResponse.Sleep.Length; i++)
                {
                    SleepResponse sleep = sleepResponse.Sleep[i];
                    ActivityResponse activity = activityResponse.Activity[i];
                    ReadinessResponse readiness = readinessResponse.Readiness[i];
                    if (sleep.SummaryDate == activity.SummaryDate && sleep.SummaryDate == readiness.SummaryDate)
                    {
                        OuraCombinedObject oObj = new OuraCombinedObject();
                        oObj.UpdateFrom(sleep, readiness, activity);
                        ouraObjects.Add(oObj);
                    }
                    else
                    {
                        Console.WriteLine("Failed");
                    }
                }

                foreach (OuraCombinedObject obj in ouraObjects)
                {
                    Console.WriteLine("Dynamic Results 1: {0}", dynamicExecuteMethod(obj, "BedtimeStartFormatLocal", "DateTime"));
                    Console.WriteLine("Dynamic Results 2: {0}", dynamicExecuteProperty(obj, "SummaryDate", "DateTime"));

                }
            }

            //localFields.Add(new OuraFields("Bedtime End (Default)", "BedtimeEnd", "Sleep", "BedtimeEnd"));
            //localFields.Add(new OuraFields("Bedtime Start (Local)", "BedtimeStart", "Sleep", "BedtimeStartFormatLocal()"));
            //localFields.Add(new OuraFields("Bedtime End (Local)", "BedtimeEnd", "Sleep", "BedtimeEndFormatLocal()"));
            //localFields.Add(new OuraFields("Score", "Score", "Sleep", "Score"));
            // type.GetProperty("SummaryDate", BindingFlags.Public | BindingFlags.Instance);
            // type.GetMethod("BedtimeStartFormatLocal", BindingFlags.Public | BindingFlags.Instance);
        }

        static string dynamicExecuteProperty(OuraCombinedObject obj, string functionName, string returnType)
        {
            Type type = typeof(OuraCombinedObject);
            PropertyInfo property = type.GetProperty(functionName, BindingFlags.Public | BindingFlags.Instance);
            object result = property.GetValue(obj); // Static methods, with no parameters
            if (result == null)
                return string.Empty;
            return result.ToString();
            // Could also be return (int)result;, if it was an integer (boxed to an object), etc.
        }

        static string dynamicExecuteMethod(OuraCombinedObject obj, string functionName, string returnType)
        {
            Type type = typeof(OuraCombinedObject);
            MethodInfo method = type.GetMethod(functionName, BindingFlags.Public | BindingFlags.Instance);
            object result = method.Invoke(obj, null); // Static methods, with no parameters
            if (result == null)
                return string.Empty;
            return result.ToString();
            // Could also be return (int)result;, if it was an integer (boxed to an object), etc.
        }
    }
}

