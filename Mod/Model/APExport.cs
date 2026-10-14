using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace com.seadoggie.TFWRArchipelago.Model;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class APExport
{
    public IEnumerable<APLocation> locations { get; set; }
    public IEnumerable<APRegion> regions { get; set; }
    public IEnumerable<APItem> items { get; set; }
    public IEnumerable<Crop> crops { get; set; }
}