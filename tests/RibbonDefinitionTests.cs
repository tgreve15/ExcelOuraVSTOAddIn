using ExcelOuraVSTOAddIn;
using OuraAPIInterface;
using System;
using System.IO;
using System.Linq;
using Xunit;

public sealed class RibbonDefinitionTests
{
    [Fact]
    public void LayoutPreservesEveryExportExactlyOnceAndDoesNotDisableConnection()
    {
        var buttons = OuraRibbonDefinition.Groups.SelectMany(group => group.Buttons).ToArray();
        Assert.Equal(buttons.Length, buttons.Select(button => button.Name).Distinct(StringComparer.Ordinal).Count());
        var actual = buttons.Where(button => button.Command == OuraRibbonCommand.Collection).Select(button => button.Endpoint).OrderBy(endpoint => endpoint);
        Assert.Equal(OuraCollections.All.Select(collection => collection.Endpoint).OrderBy(endpoint => endpoint), actual);
        foreach (var command in new[] { OuraRibbonCommand.DailyData, OuraRibbonCommand.SpO2, OuraRibbonCommand.Workouts, OuraRibbonCommand.Tags, OuraRibbonCommand.Connect })
            Assert.Single(buttons, button => button.Command == command);
        Assert.All(buttons, button => Assert.Equal(button.Command != OuraRibbonCommand.Connect, button.RequiresConnection));
    }

    [Fact]
    public void EveryConfiguredIconHasBothEmbeddedPngSizes()
    {
        var assembly = typeof(RibbonDefinitionTests).Assembly;
        foreach (var button in OuraRibbonDefinition.Groups.SelectMany(group => group.Buttons))
        {
            foreach (int size in new[] { 16, 32 })
            {
                using var stream = assembly.GetManifestResourceStream("ExcelOuraVSTOAddIn.RibbonIcons." + button.Icon + "-" + size + ".png");
                Assert.NotNull(stream);
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                byte[] bytes = memory.ToArray();
                Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes.Take(8));
                Assert.Equal(size, ReadBigEndianInt32(bytes, 16));
                Assert.Equal(size, ReadBigEndianInt32(bytes, 20));
                Assert.Equal(6, bytes[25]); // RGBA: preserve transparent backgrounds.
            }
        }
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
}
