using tulo.eInvoiceXmlGeneratorCii.Models;
using Zugferd24.Extended;

namespace tulo.eInvoiceXmlGeneratorCii.Mappers;
/// <summary>
/// Defines a contract for mapping application invoice models to the ZUGFeRD Extended model.
/// </summary>
/// <remarks>
/// Implementations are responsible for converting domain-specific <see cref="Invoice"/> instances into
/// <see cref="CrossIndustryInvoiceType"/> instances suitable for ZUGFeRD (extended profile) consumption.
/// Implementers should ensure that required ZUGFeRD fields are populated and perform any necessary
/// validation or transformation (for example: currency formatting, tax breakdowns, and party identification).
/// </remarks>
public interface ICiiMapper
{
    /// <summary>
    /// Maps a simplified application <see cref="Invoice"/> model to a ZUGFeRD Extended
    /// <see cref="CrossIndustryInvoiceType"/> representation.
    /// </summary>
    /// <param name="invoice">The source <see cref="Invoice"/> containing data to be mapped into ZUGFeRD.</param>
    /// <returns>
    /// A <see cref="CrossIndustryInvoiceType"/> instance that represents the mapped ZUGFeRD Extended invoice.
    /// </returns>
    /// <remarks>
    /// Implementations should produce a fully-populated <see cref="CrossIndustryInvoiceType"/> that adheres
    /// to the expected ZUGFeRD profile. Mapping should handle conversions such as monetary formatting,
    /// tax calculations, and mapping of parties and line items.
    /// </remarks>
    CrossIndustryInvoiceType Map(Invoice invoice);
}
