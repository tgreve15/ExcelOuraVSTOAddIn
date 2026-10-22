using Newtonsoft.Json;
using OuraAPIInterface;
using System;
using System.Collections.Generic;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraRibbonBar
    {
        private static Dictionary<string, string> LoadAdditionalFieldSelections()
        {
            try
            {
                string json = OuraExcelSettings.Default.AdditionalExportFields;
                if (String.IsNullOrWhiteSpace(json) || json.Length > 1024 * 1024) return new Dictionary<string, string>();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json,
                    new JsonSerializerSettings { MaxDepth = 16 }) ?? new Dictionary<string, string>();
            }
            catch (JsonException) { return new Dictionary<string, string>(); }
            catch (System.Configuration.ConfigurationErrorsException) { return new Dictionary<string, string>(); }
        }
        private void ExportCollection(OuraCollection definition)
        {
            ExecuteSafely(() =>
            {
                var saved = LoadAdditionalFieldSelections();
                string selection;
                saved.TryGetValue(definition.Endpoint, out selection);
                var timezone = TimeZoneInfo.Local;
                RunExport(definition.Fields, selection, json =>
                {
                    saved[definition.Endpoint] = json;
                    OuraExcelSettings.Default.AdditionalExportFields = JsonConvert.SerializeObject(saved);
                }, definition.Label,
                    (start, end, token, progress, options) => OuraCollectionClient.FetchAsync(definition, start, end, token, progress, timezone, options),
                    definition.DateMode != OuraDateMode.All && definition.DateMode != OuraDateMode.Single);
            });
        }
    }
}
