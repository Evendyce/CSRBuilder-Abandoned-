using System.Globalization;
using System.Xml.Linq;
using static Csr.XmlBuilder.Ns;

namespace Csr.XmlBuilder;

public static class CsrXmlBuilder
{
    public static XDocument BuildCsr(CsrExportBundle bundle)
    {
        var root = new XElement(gmd + "MD_Metadata",
            new XAttribute(XNamespace.Xmlns + "gmd", gmd),
            new XAttribute(XNamespace.Xmlns + "gco", gco),
            new XAttribute(XNamespace.Xmlns + "gmi", gmi),
            new XAttribute(XNamespace.Xmlns + "gml", gml),
            new XAttribute(XNamespace.Xmlns + "gmx", gmx),
            new XAttribute(XNamespace.Xmlns + "gts", gts),
            new XAttribute(XNamespace.Xmlns + "xlink", xlink),
            new XAttribute(XNamespace.Xmlns + "xsi", xsi),
            new XAttribute(XNamespace.Xmlns + "sdn", sdn),
            new XAttribute(xsi + "schemaLocation",
                string.Join(' ',
                    "http://www.isotc211.org/2005/gmd", "schema/iso19139/gmd/gmd.xsd",
                    "http://www.isotc211.org/2005/gco", "schema/iso19139/gco/gco.xsd",
                    "http://www.isotc211.org/2005/gmi", "schema/iso19139/gmi/gmi.xsd",
                    "http://www.opengis.net/gml/3.2", "schema/iso19139/gml/gml.xsd",
                    "https://www.seadatanet.org/urnschema", "schema/sdn-csr/SDN_CSR_ISO19139_5.2.0.xsd"))
        );

        XmlUtil.AddIfNotNull(root, BuildFileIdentifier(bundle));
        foreach (var e in BuildLanguageAndCharset(bundle)) root.Add(e);
        XmlUtil.AddIfNotNull(root, BuildHierarchyLevel(bundle));
        XmlUtil.AddIfNotNull(root, BuildContact(bundle));
        XmlUtil.AddIfNotNull(root, BuildDateStamp(bundle));
        foreach (var e in BuildMetadataStandardInfo()) root.Add(e);
        XmlUtil.AddIfNotNull(root, BuildIdentification(bundle));
        XmlUtil.AddIfNotNull(root, BuildDistribution(bundle));
        XmlUtil.AddIfNotNull(root, BuildDataQuality(bundle));
        XmlUtil.AddIfNotNull(root, BuildAcquisitionInformation(bundle));

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
    }

    static XElement? BuildFileIdentifier(CsrExportBundle b)
    {
        if (string.IsNullOrWhiteSpace(b.CsrLocalId)) return null;
        var id = $"urn:SDN:CSR:LOCAL:{b.CsrLocalId}";
        return new XElement(gmd + "fileIdentifier",
            new XElement(gco + "CharacterString", id));
    }

    static IEnumerable<XElement> BuildLanguageAndCharset(CsrExportBundle b)
    {
        yield return new XElement(gmd + "language",
            new XElement(gmd + "LanguageCode",
                new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#LanguageCode"),
                new XAttribute("codeListValue", "eng"),
                "eng"));
        yield return new XElement(gmd + "characterSet",
            new XElement(gmd + "MD_CharacterSetCode",
                new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#MD_CharacterSetCode"),
                new XAttribute("codeListValue", "utf8"),
                "utf8"));
    }

    static XElement? BuildHierarchyLevel(CsrExportBundle b)
    {
        return new XElement(gmd + "hierarchyLevel",
            new XElement(gmd + "MD_ScopeCode",
                new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#MD_ScopeCode"),
                new XAttribute("codeListValue", "dataset"),
                "dataset"));
    }

    static XElement? BuildContact(CsrExportBundle b)
    {
        var org = b.ResponsibleLab;
        if (org == null) return null;
        var rp = new XElement(gmd + "CI_ResponsibleParty");
        XmlUtil.AddIfNotNull(rp, new XElement(gmd + "organisationName",
            new XElement(gco + "CharacterString", org.Name)));
        XmlUtil.AddIfNotNull(rp, new XElement(gmd + "role",
            new XElement(gmd + "CI_RoleCode",
                new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#CI_RoleCode"),
                new XAttribute("codeListValue", "pointOfContact"),
                "pointOfContact")));
        return new XElement(gmd + "contact", rp);
    }

    static XElement? BuildDateStamp(CsrExportBundle b)
    {
        var date = b.RevisionDateUtc ?? DateTime.UtcNow;
        return new XElement(gmd + "dateStamp",
            new XElement(gco + "DateTime", date.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture)));
    }

    static IEnumerable<XElement> BuildMetadataStandardInfo()
    {
        yield return new XElement(gmd + "metadataStandardName",
            new XElement(gco + "CharacterString", "SeaDataNet CSR"));
        yield return new XElement(gmd + "metadataStandardVersion",
            new XElement(gco + "CharacterString", "5.2.0"));
    }

    static XElement? BuildIdentification(CsrExportBundle b)
    {
        var citation = new XElement(gmd + "CI_Citation",
            new XElement(gmd + "title", new XElement(gco + "CharacterString", b.CruiseName)));
        if (b.BeginUtc.HasValue)
        {
            citation.Add(new XElement(gmd + "date",
                new XElement(gmd + "CI_Date",
                    new XElement(gmd + "date",
                        new XElement(gco + "Date", b.BeginUtc.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))),
                    new XElement(gmd + "dateType",
                        new XElement(gmd + "CI_DateTypeCode",
                            new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#CI_DateTypeCode"),
                            new XAttribute("codeListValue", "creation"),
                            "creation")))));
        }

        var id = new XElement(gmd + "MD_DataIdentification",
            new XElement(gmd + "citation", citation));

        if (!string.IsNullOrWhiteSpace(b.Abstract))
            id.Add(new XElement(gmd + "abstract", new XElement(gco + "CharacterString", b.Abstract)));

        if (b.SeaAreas.Count > 0)
        {
            foreach (var sa in b.SeaAreas)
            {
                var mdKeywords = new XElement(gmd + "MD_Keywords",
                    new XElement(gmd + "keyword", new XElement(gco + "CharacterString", sa.Label ?? sa.Code)),
                    new XElement(gmd + "type", new XElement(gmd + "MD_KeywordTypeCode",
                        new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#MD_KeywordTypeCode"),
                        new XAttribute("codeListValue", "place"),
                        "place"))
                );
                id.Add(new XElement(gmd + "descriptiveKeywords", mdKeywords));
            }
        }

        foreach (var rc in BuildResourceConstraints(b))
            id.Add(rc);
      
        var extent = new XElement(gmd + "EX_Extent");
        XmlUtil.AddIfNotNull(extent, BuildTemporalExtent(b));
        XmlUtil.AddIfNotNull(extent, BuildSpatialExtent(b));
        if (extent.HasElements)
            id.Add(new XElement(gmd + "extent", extent));

        return new XElement(gmd + "identificationInfo", id);
    }

    static XElement? BuildTemporalExtent(CsrExportBundle b)
    {
        if (!b.BeginUtc.HasValue && !b.EndUtc.HasValue) return null;
        var tp = new XElement(gml + "TimePeriod",
            new XElement(gml + "beginPosition", b.BeginUtc?.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture)),
            new XElement(gml + "endPosition", b.EndUtc?.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture))
        );
        return new XElement(gmd + "temporalElement",
            new XElement(gmd + "EX_TemporalExtent",
                new XElement(gmd + "extent", tp)));
    }

    static XElement? BuildSpatialExtent(CsrExportBundle b)
    {
        if (!b.West.HasValue || !b.East.HasValue || !b.South.HasValue || !b.North.HasValue) return null;
        var bbox = new XElement(gmd + "EX_GeographicBoundingBox",
            new XElement(gmd + "westBoundLongitude", new XElement(gco + "Decimal", b.West.Value.ToString(CultureInfo.InvariantCulture))),
            new XElement(gmd + "eastBoundLongitude", new XElement(gco + "Decimal", b.East.Value.ToString(CultureInfo.InvariantCulture))),
            new XElement(gmd + "southBoundLatitude", new XElement(gco + "Decimal", b.South.Value.ToString(CultureInfo.InvariantCulture))),
            new XElement(gmd + "northBoundLatitude", new XElement(gco + "Decimal", b.North.Value.ToString(CultureInfo.InvariantCulture))));
        return new XElement(gmd + "geographicElement", bbox);
    }

    static IEnumerable<XElement> BuildResourceConstraints(CsrExportBundle b)
    {
        if (b.Distribution == null) yield break;
        var dist = b.Distribution;
        var legal = new XElement(gmd + "MD_LegalConstraints");
        if (!string.IsNullOrWhiteSpace(dist.UseLimitation))
            legal.Add(new XElement(gmd + "useLimitation", new XElement(gco + "CharacterString", dist.UseLimitation)));
        if (!string.IsNullOrWhiteSpace(dist.AccessConstraints))
            legal.Add(new XElement(gmd + "accessConstraints",
                new XElement(gmd + "MD_RestrictionCode",
                    new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#MD_RestrictionCode"),
                    new XAttribute("codeListValue", dist.AccessConstraints),
                    dist.AccessConstraints)));
        if (legal.HasElements)
            yield return new XElement(gmd + "resourceConstraints", legal);
    }

    static XElement? BuildDistribution(CsrExportBundle b)
    {
        var d = b.Distribution;
        if (d == null) return null;
        var dist = new XElement(gmd + "MD_Distribution");
        foreach (var f in d.Formats)
        {
            var fmt = new XElement(gmd + "MD_Format",
                new XElement(gmd + "name", new XElement(gco + "CharacterString", f.Name)));
            if (!string.IsNullOrWhiteSpace(f.Version))
                fmt.Add(new XElement(gmd + "version", new XElement(gco + "CharacterString", f.Version)));
            dist.Add(new XElement(gmd + "distributionFormat", fmt));
        }
        if (d.OnlineResources.Count > 0)
        {
            var dto = new XElement(gmd + "MD_DigitalTransferOptions");
            foreach (var r in d.OnlineResources)
            {
                var or = new XElement(gmd + "CI_OnlineResource",
                    new XElement(gmd + "linkage", new XElement(gmd + "URL", r.Url)));
                if (!string.IsNullOrWhiteSpace(r.Description))
                    or.Add(new XElement(gmd + "description", new XElement(gco + "CharacterString", r.Description)));
                if (!string.IsNullOrWhiteSpace(r.Protocol))
                    or.Add(new XElement(gmd + "protocol", new XElement(gco + "CharacterString", r.Protocol)));
                dto.Add(new XElement(gmd + "onLine", or));
            }
            dist.Add(new XElement(gmd + "transferOptions", dto));
        }
        return dist.HasElements ? new XElement(gmd + "distributionInfo", dist) : null;
    }

    static XElement? BuildDataQuality(CsrExportBundle b)
    {
        var dq = b.DataQuality;
        if (dq == null) return null;
        var dqEl = new XElement(gmd + "DQ_DataQuality",
            new XElement(gmd + "scope",
                new XElement(gmd + "DQ_Scope",
                    new XElement(gmd + "level",
                        new XElement(gmd + "MD_ScopeCode",
                            new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#MD_ScopeCode"),
                            new XAttribute("codeListValue", "dataset"),
                            "dataset")))),
            new XElement(gmd + "report",
                new XElement(gmd + "DQ_DomainConsistency",
                    new XElement(gmd + "result",
                        new XElement(gmd + "DQ_ConformanceResult",
                            new XElement(gmd + "pass", new XElement(gco + "Boolean", "true")))))));

        var lineage = new XElement(gmd + "LI_Lineage");
        if (!string.IsNullOrWhiteSpace(dq.Lineage))
            lineage.Add(new XElement(gmd + "statement", new XElement(gco + "CharacterString", dq.Lineage)));
        foreach (var ps in dq.ProcessSteps)
        {
            var step = new XElement(gmd + "LI_ProcessStep");
            if (!string.IsNullOrWhiteSpace(ps.Description))
                step.Add(new XElement(gmd + "description", new XElement(gco + "CharacterString", ps.Description)));
            if (ps.Date.HasValue)
                step.Add(new XElement(gmd + "dateTime", new XElement(gco + "DateTime", ps.Date.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture))));
            if (ps.Processor != null)
            {
                var proc = new XElement(gmd + "CI_ResponsibleParty",
                    new XElement(gmd + "organisationName", new XElement(gco + "CharacterString", ps.Processor.Name)),
                    new XElement(gmd + "role",
                        new XElement(gmd + "CI_RoleCode",
                            new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#CI_RoleCode"),
                            new XAttribute("codeListValue", "processor"),
                            "processor")));
                step.Add(new XElement(gmd + "processor", proc));
            }
            lineage.Add(new XElement(gmd + "processStep", step));
        }
        dqEl.Add(new XElement(gmd + "lineage", lineage));
        return new XElement(gmd + "dataQualityInfo", dqEl);
    }

    static XElement? BuildAcquisitionInformation(CsrExportBundle b)
    {
        if (b.Moorings == null || b.Moorings.Count == 0) return null;
        var mi = new XElement(gmi + "MI_AcquisitionInformation");
        foreach (var m in b.Moorings)
        {
            var op = new XElement(gmi + "MI_Operation");
            if (!string.IsNullOrWhiteSpace(m.Description))
                op.Add(new XElement(gmd + "description", new XElement(gco + "CharacterString", m.Description)));
            if (m.EventTimeUtc.HasValue)
                op.Add(new XElement(gmd + "citation",
                    new XElement(gmd + "CI_Citation",
                        new XElement(gmd + "date",
                            new XElement(gmd + "CI_Date",
                                new XElement(gmd + "date", new XElement(gco + "DateTime", m.EventTimeUtc.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture))),
                                new XElement(gmd + "dateType", new XElement(gmd + "CI_DateTypeCode",
                                    new XAttribute("codeList", "http://www.isotc211.org/2005/resources/codeList.xml#CI_DateTypeCode"),
                                    new XAttribute("codeListValue", "creation"),
                                    "creation")))))));
            mi.Add(new XElement(gmi + "operation", op));
        }
      
        return mi.HasElements ? new XElement(gmi + "acquisitionInformation", mi) : null;
    }
}

public sealed class CsrExportBundle
{
    public string? CsrLocalId { get; set; }
    public string CruiseName { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public DateTime? RevisionDateUtc { get; set; }
    public DateTime? BeginUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public double? West { get; set; }
    public double? East { get; set; }
    public double? South { get; set; }
    public double? North { get; set; }
    public Organization? ResponsibleLab { get; set; }
    public List<Keyword> SeaAreas { get; set; } = new();
    public List<Mooring> Moorings { get; set; } = new();
    public Distribution? Distribution { get; set; }
    public DataQuality? DataQuality { get; set; }
}

public sealed class Organization
{
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Country { get; set; }
}

public sealed class Keyword
{
    public string? Code { get; set; }
    public string? Label { get; set; }
}
public sealed class Mooring
{
    public string? DataCategoryCode { get; set; }
    public string? DataCategoryLabel { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? EventTimeUtc { get; set; }
    public string? PrincipalInvestigator { get; set; }
    public string? OrganizationName { get; set; }
    public string? Email { get; set; }
    public string? PlatformDescription { get; set; }
}
public sealed class Distribution
{
    public List<Format> Formats { get; set; } = new();
    public List<OnlineResource> OnlineResources { get; set; } = new();
    public string? UseLimitation { get; set; }
    public string? AccessConstraints { get; set; }
}

public sealed class Format
{
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
}

public sealed class OnlineResource
{
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Protocol { get; set; }
}

public sealed class DataQuality
{
    public string? Lineage { get; set; }
    public List<ProcessStep> ProcessSteps { get; set; } = new();
}

public sealed class ProcessStep
{
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public Organization? Processor { get; set; }
}
