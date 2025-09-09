# Build a C# library that generates a SeaDataNet CSR (ISO 19139 profile) XML from our JSON

## Goal

Create a small C# class library that reads **`samples/example-input.json`** (the contract) and produces a **CSR XML** that is valid against the **local XSDs** under `schema/**`. No network fetching. Keep the code modular and omit empty/unknown data.

---

## Inputs (repo paths are fixed)

* **JSON contract:** `./samples/example-input.json`
* **Schemas:**

  * CSR profile: `./schema/sdn-csr/SDN_CSR_ISO19139_5.2.0.xsd`
  * ISO 19139 deps (imported by CSR): `./schema/iso19139/gmd/*.xsd`, `gco/*.xsd`, `gmi/*.xsd`, `gml/*.xsd`
* **Write code into:** `./src/Csr.XmlBuilder/`

  * `CsrXmlBuilder.cs` (entrypoint + section builders)
  * `Ns.cs` (namespace constants — already present)
  * `XmlUtil.cs` (small helpers — already present)
  * `JsonLoader.cs` (read & map JSON → model — already present; extend if needed)

> **Do not** rename folders/files. **Do not** pull schemas from the web.

---

## Output

* Public API:

  ```csharp
  namespace Csr.XmlBuilder {
    public static class CsrXmlBuilder {
      public static XDocument BuildCsr(CsrExportBundle bundle);
    }
  }
  ```
* JSON helper (extend `JsonLoader`):

  ```csharp
  public static CsrExportBundle FromJson(string json);
  ```
* Library returns an `XDocument`; caller handles saving.

---

## Namespaces (use these exact URIs & prefixes)

```
gmd="http://www.isotc211.org/2005/gmd"
gco="http://www.isotc211.org/2005/gco"
gmi="http://www.isotc211.org/2005/gmi"
gml="http://www.opengis.net/gml/3.2"
gmx="http://www.isotc211.org/2005/gmx"
gts="http://www.isotc211.org/2005/gts"
xlink="http://www.w3.org/1999/xlink"
xsi="http://www.w3.org/2001/XMLSchema-instance"
sdn="https://www.seadatanet.org/urnschema"
```

---

## Root element & `schemaLocation`

* Root **must be** `gmd:MD_Metadata`.
* Declare namespaces **once** at the root.
* Provide **local** `xsi:schemaLocation` pairs (space-separated namespace/Path pairs):

```
http://www.isotc211.org/2005/gmd schema/iso19139/gmd/gmd.xsd
http://www.isotc211.org/2005/gco schema/iso19139/gco/gco.xsd
http://www.isotc211.org/2005/gmi schema/iso19139/gmi/gmi.xsd
http://www.opengis.net/gml/3.2   schema/iso19139/gml/gml.xsd
https://www.seadatanet.org/urnschema schema/sdn-csr/SDN_CSR_ISO19139_5.2.0.xsd
```

---

## Rules & conventions

* **Contract is the JSON.** Infer fields from it; do not invent data.
* **Omit** optional/empty values. Never emit empty `gco:CharacterString`.
* **Dates vs DateTimes**

  * If the schema slot is **date-only**, use `<gco:Date>yyyy-MM-dd</gco:Date>`.
  * Otherwise use **UTC** `<gco:DateTime>yyyy-MM-ddTHH:mm:ssZ</gco:DateTime>`.
* **Temporal extent** uses `gml:TimePeriod/gml:beginPosition` & `gml:endPosition` (ISO-8601).
* **Numbers** (`posList`, coords, depths) use `InvariantCulture` and required precision.
* **Element order** follows ISO 19139 ordering inside each complex type.
* **Keywords** as `gmd:MD_Keywords` (include `gmd:type/gmd:MD_KeywordTypeCode` and thesaurus citation when present in JSON).
* **Acquisition**: map moorings/operations under `gmi:MI_AcquisitionInformation`.
* **URNs**: if JSON holds local IDs, format URNs consistently as `urn:SDN:CSR:LOCAL:{LocalId}:...`. If no data, omit.

---

## Sections to implement (keep functions small)

Implement these **private** helpers in `CsrXmlBuilder.cs` and call them in spec order:

1. `BuildFileIdentifier(CsrExportBundle b)` → `gmd:fileIdentifier/gco:CharacterString`
2. `BuildLanguageAndCharset(CsrExportBundle b)` → `gmd:language`, `gmd:characterSet`
3. `BuildHierarchyLevel(CsrExportBundle b)` (usually `dataset`)
4. `BuildContact(CsrExportBundle b)` (if present)
5. `BuildDateStamp(CsrExportBundle b)` → `gmd:dateStamp`
6. `BuildMetadataStandardInfo()` → `gmd:metadataStandardName` / `gmd:metadataStandardVersion`
7. `BuildIdentification(CsrExportBundle b)` → `gmd:identificationInfo/gmd:MD_DataIdentification`

   * `citation/title/date`
   * `abstract`
   * `descriptiveKeywords` (multiple packs)
   * `extent` (calls temporal + spatial)
8. `BuildDistribution(CsrExportBundle b)` (optional)
9. `BuildDataQuality(CsrExportBundle b)` (minimal, if required by profile)
10. `BuildAcquisitionInformation(CsrExportBundle b)`

    * `gmi:MI_AcquisitionInformation/gmi:operation/gmi:MI_Operation`
    * `gmi:objective/gmi:MI_Objective`
    * `gmi:event/gmi:MI_Event`
    * Include only rows that pass your validity rules.

Utility to use:

```csharp
static void AddIfNotNull(XElement parent, XElement? child)
```

Add blocks **only** when data exists.

---

## Minimal root bootstrap (target shape)

```csharp
var root = new XElement(Ns.gmd + "MD_Metadata",
  new XAttribute(XNamespace.Xmlns + "gmd", Ns.gmd),
  new XAttribute(XNamespace.Xmlns + "gco", Ns.gco),
  new XAttribute(XNamespace.Xmlns + "gmi", Ns.gmi),
  new XAttribute(XNamespace.Xmlns + "gml", Ns.gml),
  new XAttribute(XNamespace.Xmlns + "gmx", Ns.gmx),
  new XAttribute(XNamespace.Xmlns + "gts", Ns.gts),
  new XAttribute(XNamespace.Xmlns + "xlink", Ns.xlink),
  new XAttribute(XNamespace.Xmlns + "xsi", Ns.xsi),
  new XAttribute(XNamespace.Xmlns + "sdn", Ns.sdn),
  new XAttribute(Ns.xsi + "schemaLocation",
    string.Join(' ',
      "http://www.isotc211.org/2005/gmd",   "schema/iso19139/gmd/gmd.xsd",
      "http://www.isotc211.org/2005/gco",   "schema/iso19139/gco/gco.xsd",
      "http://www.isotc211.org/2005/gmi",   "schema/iso19139/gmi/gmi.xsd",
      "http://www.opengis.net/gml/3.2",     "schema/iso19139/gml/gml.xsd",
      "https://www.seadatanet.org/urnschema","schema/sdn-csr/SDN_CSR_ISO19139_5.2.0.xsd"
)));
```

---

## Mapping guidance (derive from JSON)

* Inspect `samples/example-input.json`. For each non-null field, choose the correct ISO slot and emit it.
* Prefer **one-purpose** mappers, e.g.:

  * `BuildCitationIdentifier(b)` → additional `gmd:identifier` blocks
  * `BuildTemporalExtent(b)` → `gmd:extent/gmd:EX_Extent/gmd:temporalElement/.../gml:TimePeriod`
  * `BuildSpatialExtent(b)` → `gmd:geographicElement` + `gmd:EX_BoundingPolygon` or `EX_GeographicBoundingBox`
* If the JSON doesn’t include a concept, **omit the whole block**.

---

## Acceptance criteria

1. The produced XML uses **exact** namespace URIs above, with a `gmd:MD_Metadata` root.
2. `xsi:schemaLocation` points to the **local** files under `schema/**`.
3. **No empty nodes**; optional data is **omitted**.
4. Dates vs dateTimes are emitted correctly (see “Rules & conventions”).
5. Temporal extent uses **GML 3.2 TimePeriod** with `beginPosition`/`endPosition`.
6. Code compiles on **.NET 8+**.

---

## Non-goals (for now)

* Schematron validation
* Byte-for-byte parity with any SDN sample
* Unit tests

---

## Style

* Small, pure functions.
* No external dependencies beyond BCL.
* Use `CultureInfo.InvariantCulture` when converting numbers/DateTimes to text.
* Keep ordering per ISO types; do not sort elements alphabetically.

---

## Deliverables

* Implement `BuildCsr` in `src/Csr.XmlBuilder/CsrXmlBuilder.cs` and the helper builders listed above.
* Extend `JsonLoader` to parse the JSON into `CsrExportBundle` and children.
* Ensure a simple runner (console/app) can do:

  1. `var b = JsonLoader.FromFile("samples/example-input.json");`
  2. `var x = CsrXmlBuilder.BuildCsr(b);`
  3. `x.Save("out-csr.xml");`

> **Important:** If any mapping is ambiguous from the JSON, prefer **omission** over guessing.
