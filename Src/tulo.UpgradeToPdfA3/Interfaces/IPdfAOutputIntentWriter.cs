using PdfSharp.Pdf;
using tulo.UpgradeToPdfA3.Options;
using tulo.UpgradeToPdfA3.ResultPattern;

namespace tulo.UpgradeToPdfA3.Interfaces;

/// <summary>
/// Provides functionality to write the output intent into a <see cref="PdfDocument"/>,
/// which defines the intended output device or colour profile for rendering.
/// An output intent is mandatory for PDF/A conformance as it ensures
/// consistent and predictable colour reproduction across different devices.
/// </summary>
public interface IPdfAOutputIntentWriter
{
    /// <summary>
    /// Writes the output intent entry into the provided <see cref="PdfDocument"/>
    /// using the colour profile and rendering settings defined in the upgrade options.
    /// </summary>
    /// <param name="pdfDocument">
    /// The target <see cref="PdfDocument"/> into which the output intent
    /// will be written. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="appOptions">
    /// The PDF/A-3 upgrade options providing the colour profile (e.g. ICC profile path),
    /// output condition identifier, and registry name required for the output intent entry.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult"/> indicating whether the output intent
    /// was written successfully. Contains specific error details on failure.
    /// </returns>
    OperationResult Write(PdfDocument pdfDocument, IUpgradeToPdfA3Options appOptions);
}

