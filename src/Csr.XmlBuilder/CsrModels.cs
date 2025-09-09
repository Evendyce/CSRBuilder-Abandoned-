using System;
using System.Collections.Generic;

namespace Csr.XmlBuilder;

/// <summary>
/// Root object representing all metadata required to construct a CSR XML document.
/// </summary>
public sealed class CsrExportBundle
{
    /// <summary>Unique identifier for the CSR record.</summary>
    public string? CsrLocalId { get; set; }

    /// <summary>Name or title of the cruise.</summary>
    public string CruiseName { get; set; } = string.Empty;

    /// <summary>Textual abstract describing the cruise.</summary>
    public string? Abstract { get; set; }

    /// <summary>Date when the metadata was last revised.</summary>
    public DateTime? RevisionDateUtc { get; set; }

    /// <summary>UTC timestamp when the cruise began.</summary>
    public DateTime? BeginUtc { get; set; }

    /// <summary>UTC timestamp when the cruise ended.</summary>
    public DateTime? EndUtc { get; set; }

    /// <summary>Westernmost longitude of the cruise track.</summary>
    public double? West { get; set; }

    /// <summary>Easternmost longitude of the cruise track.</summary>
    public double? East { get; set; }

    /// <summary>Southernmost latitude of the cruise track.</summary>
    public double? South { get; set; }

    /// <summary>Northernmost latitude of the cruise track.</summary>
    public double? North { get; set; }

    /// <summary>Organization responsible for the CSR metadata.</summary>
    public Organization? ResponsibleLab { get; set; }

    /// <summary>Sea-area keywords associated with the cruise.</summary>
    public List<Keyword> SeaAreas { get; set; } = new();

    /// <summary>Mooring or operation metadata associated with the cruise.</summary>
    public List<Mooring> Moorings { get; set; } = new();

    /// <summary>Distribution metadata describing formats and online resources.</summary>
    public Distribution? Distribution { get; set; }

    /// <summary>Data-quality lineage and processing information.</summary>
    public DataQuality? DataQuality { get; set; }
}

/// <summary>
/// Represents an organisation or laboratory involved in the cruise.
/// </summary>
public sealed class Organization
{
    public string Name { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Country { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>
/// Keyword entry, typically for sea-area descriptors.
/// </summary>
public sealed class Keyword
{
    public string? Code { get; set; }
    public string? Label { get; set; }
}

/// <summary>
/// Single mooring or operational event associated with the cruise.
/// </summary>
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

/// <summary>
/// Distribution metadata describing how cruise data can be accessed.
/// </summary>
public sealed class Distribution
{
    public List<Format> Formats { get; set; } = new();
    public List<OnlineResource> OnlineResources { get; set; } = new();
    public string? UseLimitation { get; set; }
    public string? AccessConstraints { get; set; }
}

/// <summary>
/// Description of a data format used for distributing data.
/// </summary>
public sealed class Format
{
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
}

/// <summary>
/// Pointer to an online resource related to the cruise data.
/// </summary>
public sealed class OnlineResource
{
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Protocol { get; set; }
}

/// <summary>
/// Data-quality metadata including lineage and processing steps.
/// </summary>
public sealed class DataQuality
{
    public string? Lineage { get; set; }
    public List<ProcessStep> ProcessSteps { get; set; } = new();
}

/// <summary>
/// A single processing step applied to the data.
/// </summary>
public sealed class ProcessStep
{
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public Organization? Processor { get; set; }
}

