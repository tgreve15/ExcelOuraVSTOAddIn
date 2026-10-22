using Microsoft.Office.Tools.Ribbon;
using OuraAPIInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Office = Microsoft.Office.Core;

namespace ExcelOuraVSTOAddIn
{
    public partial class OuraRibbonBar
    {
        private readonly List<RibbonButton> exportButtons = new List<RibbonButton>();
        private readonly Dictionary<string, Image> ribbonImages = new Dictionary<string, Image>(StringComparer.Ordinal);

        public OuraRibbonBar() : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
            // VSTO freezes tab/group collections when loading starts. Build every control here.
            BuildRibbon();
        }

        private void BuildRibbon()
        {
            var tab = Factory.CreateRibbonTab();
            tab.Name = OuraRibbonDefinition.TabName;
            tab.Label = OuraRibbonDefinition.TabLabel;
            // A custom ID keeps Oura separate from other add-ins' shared built-in tabs.
            tab.ControlId.ControlIdType = RibbonControlIdType.Custom;
            foreach (var groupDefinition in OuraRibbonDefinition.Groups)
            {
                var group = Factory.CreateRibbonGroup();
                group.Name = groupDefinition.Name;
                group.Label = groupDefinition.Label;
                foreach (var definition in groupDefinition.Buttons)
                {
                    var button = Factory.CreateRibbonButton();
                    button.Name = definition.Name;
                    button.Label = definition.Label;
                    button.ScreenTip = definition.ScreenTip;
                    button.SuperTip = definition.SuperTip;
                    button.ControlSize = definition.Large ? Office.RibbonControlSize.RibbonControlSizeLarge : Office.RibbonControlSize.RibbonControlSizeRegular;
                    button.Image = LoadRibbonImage(definition.Icon, definition.Large);
                    button.ShowImage = true;
                    button.ShowLabel = true;
                    button.Enabled = !definition.RequiresConnection;
                    button.Click += (sender, args) => ExecuteRibbonCommand(definition.Command, definition.Endpoint, sender, args);
                    group.Items.Add(button);
                    if (definition.RequiresConnection) exportButtons.Add(button);
                }
                tab.Groups.Add(group);
            }
            Tabs.Add(tab);
        }

        private void ExecuteRibbonCommand(OuraRibbonCommand command, string endpoint, object sender, RibbonControlEventArgs args)
        {
            switch (command)
            {
                case OuraRibbonCommand.DailyData: btnGetOuraData_Click(sender, args); break;
                case OuraRibbonCommand.SpO2: btnDailyspo2_Click(sender, args); break;
                case OuraRibbonCommand.Workouts: btnWorkouts_Click(sender, args); break;
                case OuraRibbonCommand.Tags: btnTags_Click(sender, args); break;
                case OuraRibbonCommand.Collection: ExportCollection(OuraCollections.Get(endpoint)); break;
                case OuraRibbonCommand.Connect: btnConfigureOuraAPIKey_Click(sender, args); break;
                default: throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        private Image LoadRibbonImage(string icon, bool large)
        {
            string key = icon + (large ? "-32" : "-16");
            Image image;
            if (ribbonImages.TryGetValue(key, out image)) return image;
            using (var stream = typeof(OuraRibbonBar).Assembly.GetManifestResourceStream("ExcelOuraVSTOAddIn.RibbonIcons." + key + ".png"))
            {
                if (stream == null) throw new InvalidOperationException("Missing embedded ribbon image: " + key);
                using (var loaded = Image.FromStream(stream)) image = new Bitmap(loaded);
            }
            ribbonImages.Add(key, image);
            return image;
        }

        protected override void Dispose(bool disposing)
        {
            try { base.Dispose(disposing); }
            finally
            {
                if (disposing)
                {
                    foreach (var image in ribbonImages.Values) image.Dispose();
                    ribbonImages.Clear();
                }
            }
        }
    }
}
