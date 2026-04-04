# tulo.eInvoiceApp

A WPF desktop application for creating, previewing, and archiving compliant electronic invoices
in **ZUGFeRD / Factur-X** format — including PDF/A-3 generation and optional digital signing.

---

## What this application does

`tulo.eInvoiceApp` allows users to create structured electronic invoices based on the
**CII (Cross Industry Invoice)** standard, generate a fully compliant **PDF/A-3** document
with the XML embedded, and optionally sign the result with a digital certificate.

The application is intended for businesses and developers who need to produce, preview,
and archive legally structured electronic invoices in a practical desktop environment.

---

## Important disclaimer

Please read the disclaimer information available inside the application.

You can find it in:

**View → About**

This information is important and should be read before using the application in productive,
legal, business, or compliance-related scenarios.

The disclaimer shown in the application is the relevant notice for usage, limitations,
and responsibility.

---

## Core pipeline

Every invoice goes through the following processing steps:

```
Invoice Data (ViewModel)
        │
        ▼
  1. Build Invoice Model          (IInvoiceBuilderService)
        │
        ▼
  2. Map to CII structure         (ICiiMapper)
        │
        ▼
  3. Export CII to XML            (IXmlCiiExporter)
        │
        ▼
  4. Generate PDF stream          (IPdfGeneratorFromInvoice)
        │
        ▼
  5. Convert PDF → PDF/A          (IToPdfAConverterService)
        │
        ▼
  6. Upgrade PDF/A → PDF/A-3      (IToPdfA3UpgradeService)
     + embed CII XML attachment
        │
        ▼
  7. Sign PDF/A-3 (optional)      (tulo.SigningPdfA3.exe)
        │
        ▼
  8. Open with default viewer     (if configured)
```

---

## Features

- Create structured electronic invoices based on the CII / EN16931 standard
- Generate PDF from invoice data with full layout rendering
- Convert PDF to **PDF/A** (archival format)
- Upgrade PDF/A to **PDF/A-3** with embedded CII XML (ZUGFeRD / Factur-X compliant)
- **Optional digital signing** of the final PDF/A-3 via external signing tool
- **Preview mode** with watermark before final file creation
- **Archive** output files to a configured directory
- Open the final invoice automatically with the default PDF viewer
- Full **localization support** for UI messages via translation provider
- Structured **Serilog** logging throughout the entire pipeline
- Configurable via `appsettings.json`

---

## Preview mode

Before creating the final files, the user can request a preview.

In preview mode:
- The invoice is rendered as PDF in memory
- A **PREVIEW** watermark is applied to the document
- The result is displayed inside the application
- No files are written to disk

This allows the user to verify the invoice content visually before committing to file creation.

---

## Archive and output

When file creation is triggered, the following files are written to the configured output path:

| File | Description |
|---|---|
| `{InvoiceNumber}.pdf` | Raw generated PDF |
| `{InvoiceNumber}.xml` | CII XML export |
| `{InvoiceNumber}_PdfA.pdf` | PDF/A intermediate |
| `{InvoiceNumber}_PdfA3.pdf` | Final PDF/A-3 with embedded XML |
| `{InvoiceNumber}_SignedPdfA3.pdf` | Digitally signed PDF/A-3 (if signing is configured) |

The output path is configured in `appsettings.json`:

```json
"Archive": {
  "OutputPath": "C:\\Invoices\\Output",
  "CanOpenPdfWithDefaultApp": true
}
```

If no valid path is configured, the system temp directory is used as fallback.

---

## Digital signing (optional)

The application supports optional PDF/A-3 signing via the external CLI tool `tulo.SigningPdfA3.exe`.

Signing is **skipped silently** if any of the following is missing or not found:
- The signing executable (`SignedExepath`)
- The certificate file (`SignaturePath`)
- The certificate password (`PublicKey`)

Configure signing in `appsettings.json`:

```json
"Signature": {
  "SignedExepath": "C:\\Tools\\tulo.SigningPdfA3.exe",
  "SignaturePath": "C:\\Certificates\\invoice.pfx",
  "PublicKey": "your-certificate-password",
  "Reason": "Invoice approval",
  "Location": "Germany",
  "ContactInfo": "contact@example.com"
}
```

If all values are present and valid, the signed PDF is created automatically after Step 6
and is preferred over the unsigned PDF when opening with the default viewer.

---

## Configuration overview

All application behaviour is controlled via `appsettings.json`:

```json
{
  "Archive": {
    "OutputPath": "C:\\Invoices\\Output",
    "CanOpenPdfWithDefaultApp": true
  },
  "Signature": {
    "SignedExepath": "C:\\Tools\\tulo.SigningPdfA3.exe",
    "SignaturePath": "C:\\Certificates\\invoice.pfx",
    "PublicKey": "your-password",
    "Reason": "Invoice approval",
    "Location": "Germany",
    "ContactInfo": "contact@example.com"
  }
}
```

---

## Supported invoice standards

This application is designed for structured electronic invoices based on:

- **CII** – Cross Industry Invoice (UN/CEFACT)
- **EN 16931** – European standard for electronic invoicing
- **ZUGFeRD** – German eInvoice profile (all conformance levels)
- **Factur-X** – French/European equivalent of ZUGFeRD

Only invoice data that maps to these supported structures can be processed correctly.

---

## Open Source

This project is open source and can be used, modified, and improved by the community.

The source code is available at:
https://github.com/TuloSharp/tulo.eInvoiceApp.git

---

## Third-Party Libraries

This project uses the following third-party NuGet packages:

- PDFsharp-extended
- Serilog
- tulo.CommonMVVM.WPF
- tulo.CoreLib
- tulo.SerilogLib
- tulo.XMLeInvoiceToPdf
- tulo.ResourcesWpfLib
- tulo.LoadingSpinnerControl

All credits for these libraries go to their respective authors and maintainers.

---

## UI Icons

This project uses **Google Material Icons** in the user interface.

All credits go to their respective authors and maintainers.

---

## Logging

The application uses **Serilog** for structured logging throughout the entire pipeline.

Log entries are written for every processing step, including:
- Pipeline start and completion
- File write operations with byte/character counts
- Step-by-step PDF/A conversion and upgrade results
- Signing process output (stdout / stderr) and exit code
- All errors with full context

Logs help identify configuration issues, file access problems, and pipeline failures
during development and production use.

---

## Build and development notes

If example invoice XML files are included in the project, configure them to be copied
to the output directory automatically:

```xml
<ItemGroup>
  <None Update="Examples\**\*.xml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

---

## License

This project is open source.

- **Apache License**
- Version 2.0, January 2004
