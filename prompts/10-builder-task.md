## Goal
Generate a C# library that converts CsrExportBundle into a SeaDataNet CSR XML (ISO19139).

## Inputs
- Object model: [link to src/Csr.Models]
- Sample input: [link to samples/example-input.cs/json]
- Sample CSR (canonical): [link to samples/example-csr.xml]
- Schemas: [links to schema/*]
- Namespace map: [link to docs/Namespace-Map.md]
- Acceptance tests: below.

## Requirements
- Build helpers per block (identification, keywords, temporal/spatial extents, acquisitionInformation, parties, constraints).
- Use exact namespaces & URN formats from Namespace-Map.md.
- Omit optional nodes if source is null/empty.
- Dates: use Date **or** DateTime exactly as schema demands.
- Temporal extent via gml:TimePeriod with gml:id="cruisePeriod".
- Keywords: emit MD_Keywords with type + thesaurus citation when required.
- Return: `XDocument BuildCsr(CsrExportBundle bundle)`.

## Acceptance tests
1) Validates against all XSDs in /schema.
2) Generated output for `samples/example-input.*` matches `samples/example-csr.xml` modulo whitespace.
3) Missing optional fields are omitted (no empty elements).
4) PosList uses invariant culture and correct order.
5) Namespaces declared once at root; children are qualified.

## Deliverables
- `src/Csr.XmlBuilder/*` with small pure functions.
- `src/Csr.XmlBuilder.Tests/*` snapshot + schema-validation tests.
