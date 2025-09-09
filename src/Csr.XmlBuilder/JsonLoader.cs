using System.Globalization;
using System.Text.Json;

namespace Csr.XmlBuilder;

public static class JsonLoader
{
    public static CsrExportBundle FromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }

    public static CsrExportBundle FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var bundle = new CsrExportBundle
        {
            CruiseName = root.GetProperty("Core").GetProperty("CruiseName").GetString() ?? string.Empty,
            CsrLocalId = root.GetProperty("Core").GetProperty("CsrLocalId").GetString(),
            Abstract = root.GetProperty("Core").GetProperty("Abstract").GetString(),
            RevisionDateUtc = TryGetDateTime(root.GetProperty("Core"), "RevisionDateUtc"),
            BeginUtc = TryGetDateTime(root.GetProperty("Temporal"), "BeginUtc"),
            EndUtc = TryGetDateTime(root.GetProperty("Temporal"), "EndUtc"),
            West = TryGetDouble(root.GetProperty("Spatial"), "West"),
            East = TryGetDouble(root.GetProperty("Spatial"), "East"),
            South = TryGetDouble(root.GetProperty("Spatial"), "South"),
            North = TryGetDouble(root.GetProperty("Spatial"), "North"),
            ResponsibleLab = ParseOrganisation(root.GetProperty("Parties"), "ResponsibleLab"),
            SeaAreas = ParseSeaAreas(root.GetProperty("Spatial")),
            Moorings = ParseMoorings(root),
            Distribution = ParseDistribution(root),
            DataQuality = ParseDataQuality(root)
        };

        return bundle;
    }

    static DateTime? TryGetDateTime(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String)
        {
            if (DateTime.TryParse(el.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;
        }
        return null;
    }

    static double? TryGetDouble(JsonElement parent, string name)
    {
        if (parent.TryGetProperty(name, out var el) && el.TryGetDouble(out var d))
            return d;
        return null;
    }

    static Organization? ParseOrganisation(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var orgEl) || orgEl.ValueKind != JsonValueKind.Object)
            return null;

        var org = new Organization
        {
            Name = orgEl.GetProperty("Name").GetString() ?? string.Empty,
            Url = orgEl.TryGetProperty("Url", out var urlEl) && urlEl.ValueKind == JsonValueKind.String ? urlEl.GetString() : null,
            Country = orgEl.TryGetProperty("Country", out var cEl) && cEl.ValueKind == JsonValueKind.String ? cEl.GetString() : null
        };
        return org;
    }

    static List<Keyword> ParseSeaAreas(JsonElement spatial)
    {
        var list = new List<Keyword>();
        if (spatial.TryGetProperty("SeaAreas", out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in arr.EnumerateArray())
            {
                list.Add(new Keyword
                {
                    Code = el.TryGetProperty("Code", out var cEl) ? cEl.GetString() : null,
                    Label = el.TryGetProperty("Label", out var lEl) ? lEl.GetString() : null
                });
            }
        }
        return list;
    }

    static List<Mooring> ParseMoorings(JsonElement root)
{
        var list = new List<Mooring>();
        if (root.TryGetProperty("Moorings", out var mo) &&
            mo.TryGetProperty("Rows", out var rows) && rows.ValueKind == JsonValueKind.Array)
        {
            foreach (var r in rows.EnumerateArray())
            {
                list.Add(new Mooring
                {
                    DataCategoryCode = r.TryGetProperty("DataCategoryCode", out var dc) ? dc.GetString() : null,
                    DataCategoryLabel = r.TryGetProperty("DataCategoryLabel", out var dl) ? dl.GetString() : null,
                    Description = r.TryGetProperty("Description", out var desc) ? desc.GetString() : null,
                    Latitude = r.TryGetProperty("Latitude", out var lat) && lat.TryGetDouble(out var la) ? la : (double?)null,
                    Longitude = r.TryGetProperty("Longitude", out var lon) && lon.TryGetDouble(out var lo) ? lo : (double?)null,
                    EventTimeUtc = TryGetDateTime(r, "EventTimeUtc"),
                    PrincipalInvestigator = r.TryGetProperty("PrincipalInvestigator", out var pi) ? pi.GetString() : null,
                    OrganizationName = r.TryGetProperty("OrganisationName", out var on) ? on.GetString() : null,
                    Email = r.TryGetProperty("Email", out var em) ? em.GetString() : null,
                    PlatformDescription = r.TryGetProperty("PlatformDescription", out var pd) ? pd.GetString() : null
                });
            }
        }
        return list;
    }

    static Distribution? ParseDistribution(JsonElement root)
    {
        if (!root.TryGetProperty("Distribution", out var dEl) || dEl.ValueKind != JsonValueKind.Object)
            return null;
        var d = new Distribution();
        if (dEl.TryGetProperty("Formats", out var formats) && formats.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in formats.EnumerateArray())
            {
                d.Formats.Add(new Format
                {
                    Name = f.GetProperty("Name").GetString() ?? string.Empty,
                    Version = f.TryGetProperty("Version", out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null
                });
            }
        }
        if (dEl.TryGetProperty("OnlineResources", out var ors) && ors.ValueKind == JsonValueKind.Array)
        {
            foreach (var r in ors.EnumerateArray())
            {
                d.OnlineResources.Add(new OnlineResource
                {
                    Url = r.GetProperty("Url").GetString() ?? string.Empty,
                    Description = r.TryGetProperty("Description", out var desc) && desc.ValueKind == JsonValueKind.String ? desc.GetString() : null,
                    Protocol = r.TryGetProperty("Protocol", out var proto) && proto.ValueKind == JsonValueKind.String ? proto.GetString() : null
                });
            }
        }
        d.UseLimitation = dEl.TryGetProperty("UseLimitation", out var ul) && ul.ValueKind == JsonValueKind.String ? ul.GetString() : null;
        d.AccessConstraints = dEl.TryGetProperty("AccessConstraints", out var ac) && ac.ValueKind == JsonValueKind.String ? ac.GetString() : null;
        return d;
    }

    static DataQuality? ParseDataQuality(JsonElement root)
    {
        if (!root.TryGetProperty("DataQuality", out var dqEl) || dqEl.ValueKind != JsonValueKind.Object)
            return null;
        var dq = new DataQuality
        {
            Lineage = dqEl.TryGetProperty("Lineage", out var lin) && lin.ValueKind == JsonValueKind.String ? lin.GetString() : null
        };
        if (dqEl.TryGetProperty("ProcessSteps", out var steps) && steps.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in steps.EnumerateArray())
            {
                dq.ProcessSteps.Add(new ProcessStep
                {
                    Description = s.TryGetProperty("Description", out var desc) && desc.ValueKind == JsonValueKind.String ? desc.GetString() : null,
                    Date = TryGetDateTime(s, "Date"),
                    Processor = ParseOrganisation(s, "Processor")
                });
            }
        }
        return dq;
    }
}
