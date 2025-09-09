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

            // Pair: namespace → local schema path (relative to repo root)
            new XAttribute(xsi + "schemaLocation",
                string.Join(' ',
                    "http://www.isotc211.org/2005/gmd", "schema/iso19139/gmd/gmd.xsd",
                    "http://www.isotc211.org/2005/gco", "schema/iso19139/gco/gco.xsd",
                    "http://www.isotc211.org/2005/gmi", "schema/iso19139/gmi/gmi.xsd",
                    "http://www.opengis.net/gml/3.2", "schema/iso19139/gml/gml.xsd",
                    "https://www.seadatanet.org/urnschema", "schema/sdn-csr/SDN_CSR_ISO19139_5.2.0.xsd"
                ))
        );

        // TODO: Add sections in spec order:
        // fileIdentifier, language, characterSet, hierarchyLevel, contact, dateStamp, metadataStandard*, identificationInfo, distributionInfo, dataQualityInfo, acquisitionInformation, etc.
        // Use AddIfNotNull(root, BuildIdentification(bundle)); and so on.

        return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
    }
}

// Stub; wire your real model later
public sealed class CsrExportBundle { }
