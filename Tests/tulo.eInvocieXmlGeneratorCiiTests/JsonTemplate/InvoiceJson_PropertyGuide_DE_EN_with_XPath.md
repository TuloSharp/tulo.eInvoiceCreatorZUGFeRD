# Property Guide (DE/EN) + XML Node (XPath) – ZUGFeRD 2.4 EXTENDED (CII)

This document explains each JSON property (your C# model) and where it typically appears in the generated CrossIndustryInvoice (CII) XML.

**Namespaces used in XPath examples** (common in ZUGFeRD/Factur-X):
- `rsm`: `urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100`
- `ram`: `urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100`
- `udt`: `urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100`

> Note: Some CIUS/XRechnung-specific identifiers (e.g., Leitweg-ID) can be mapped in different places depending on the implementation. If you validate against a specific profile/CIUS, adapt those paths accordingly.

## `Buyer`
- **DE:** Käufer / Rechnungsempfänger.
- **EN:** Buyer / invoice recipient.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Currency`
- **DE:** Währungscode nach ISO 4217, z.B. EUR.
- **EN:** Currency code (ISO 4217), e.g. EUR.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeSettlement/ram:InvoiceCurrencyCode`

## `DocumentName`
- **DE:** Dokumentbezeichnung (frei), z.B. RECHNUNG.
- **EN:** Human-readable document name, e.g. INVOICE.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:Name`

## `DocumentTypeCode`
- **DE:** Dokumentart-Code (UN/CEFACT 1001), z.B. 380=Rechnung, 381=Gutschrift.
- **EN:** Document type code (UN/CEFACT 1001), e.g. 380=Invoice, 381=Credit note.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:TypeCode`

## `HeaderAllowanceTotalAmount`
- **DE:** Summe Abschläge/Rabatte auf Kopfebene (optional).
- **EN:** Total header allowances/discounts (optional).

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:AllowanceTotalAmount`

## `HeaderChargeTotalAmount`
- **DE:** Summe Zuschläge auf Kopfebene (optional).
- **EN:** Total header charges (optional).

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:ChargeTotalAmount`

## `HeaderDuePayableAmount`
- **DE:** Zahlbarer Gesamtbetrag (fällig).
- **EN:** Amount due/payable total.

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:DuePayableAmount`

## `HeaderTotalPrepaidAmount`
- **DE:** Summe bereits gezahlter Beträge (optional).
- **EN:** Total prepaid amount (optional).

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementHeaderMonetarySummation/ram:TotalPrepaidAmount`

## `InvoiceDate`
- **DE:** Rechnungsdatum (Ausstellungsdatum).
- **EN:** Invoice date (issue date).

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IssueDateTime/udt:DateTimeString`

## `InvoiceNumber`
- **DE:** Rechnungsnummer / eindeutige Dokument-ID.
- **EN:** Invoice number / unique document ID.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:ID`

## `Lines`
- **DE:** Rechnungspositionen.
- **EN:** Invoice line items.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Notes`
- **DE:** Kopf-/Dokument-Notizen (mehrere möglich).
- **EN:** Header/document notes (multiple allowed).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment`
- **DE:** Zahlungsinformationen.
- **EN:** Payment information.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Seller`
- **DE:** Verkäufer / Rechnungsaussteller.
- **EN:** Seller / invoice issuer.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Buyer.City`
- **DE:** Ort/Stadt.
- **EN:** City.

- **XML (XPath):** `/.../ram:BuyerTradeParty/ram:PostalTradeAddress/ram:CityName`

## `Buyer.ContactEmail`
- **DE:** E-Mail Käufer (optional).
- **EN:** Buyer contact email (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Buyer.ContactPersonName`
- **DE:** Ansprechpartner Käufer (optional).
- **EN:** Buyer contact person (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Buyer.ContactPhone`
- **DE:** Telefon Käufer (optional).
- **EN:** Buyer contact phone (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Buyer.CountryCode`
- **DE:** Ländercode ISO 3166-1 alpha-2.
- **EN:** Country code ISO 3166-1 alpha-2.

- **XML (XPath):** `/.../ram:BuyerTradeParty/ram:PostalTradeAddress/ram:CountryID`

## `Buyer.FiscalId`
- **DE:** Steuernummer des Käufers (optional).
- **EN:** Buyer fiscal ID / tax number (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Buyer.GeneralEmail`
- **DE:** Allgemeine E-Mail (optional).
- **EN:** General email (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Buyer.ID`
- **DE:** Interne/ERP-Kundennummer des Käufers.
- **EN:** Buyer internal/ERP identifier.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:BuyerTradeParty/ram:ID`

## `Buyer.LeitwegId`
- **DE:** Leitweg-ID für Behörden/öffentliche Auftraggeber (XRechnung Routing).
- **EN:** Leitweg-ID used for German public sector routing (XRechnung).

- **XML (XPath):** `/.../ram:BuyerTradeParty/ram:DefinedTradeContact/ram:ID[@schemeID='???']  (varies by CIUS; often under Buyer ID with XR schemes)`

## `Buyer.Name`
- **DE:** Firmenname des Käufers.
- **EN:** Buyer legal/trade name.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:BuyerTradeParty/ram:Name`

## `Buyer.Street`
- **DE:** Straße + Hausnummer.
- **EN:** Street and house number.

- **XML (XPath):** `/.../ram:BuyerTradeParty/ram:PostalTradeAddress/ram:LineOne`

## `Buyer.VatId`
- **DE:** USt-IdNr. des Käufers (optional je nach Fall).
- **EN:** Buyer VAT ID (optional depending on scenario).

- **XML (XPath):** `/.../ram:BuyerTradeParty/ram:SpecifiedTaxRegistration/ram:ID[@schemeID='VA']`

## `Buyer.Zip`
- **DE:** Postleitzahl.
- **EN:** Postal code.

- **XML (XPath):** `/.../ram:BuyerTradeParty/ram:PostalTradeAddress/ram:PostcodeCode`

## `Lines[].AdditionalReferencedDocumentId`
- **DE:** Zusätzliche Referenz-Dokumentnummer (optional).
- **EN:** Additional referenced document ID (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].AdditionalReferencedDocumentReferenceTypeCode`
- **DE:** Referenztyp (optional, z.B. VN).
- **EN:** Reference type code (optional, e.g., VN).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].AdditionalReferencedDocumentTypeCode`
- **DE:** Dokumenttyp-Code der Zusatzreferenz (optional).
- **EN:** Type code of additional referenced document (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].BillingPeriodEndDate`
- **DE:** Leistungs-/Abrechnungszeitraum Ende (optional).
- **EN:** Billing/service period end date (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].BuyerOrderDate`
- **DE:** Bestelldatum (optional).
- **EN:** Buyer order date (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].BuyerOrderReferencedId`
- **DE:** Bestellreferenz des Käufers (optional).
- **EN:** Buyer order reference (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].DeliveryNoteDate`
- **DE:** Lieferscheindatum (optional).
- **EN:** Delivery note date (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].DeliveryNoteLineId`
- **DE:** Lieferschein-Positionsnummer (optional).
- **EN:** Delivery note line ID (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].DeliveryNoteNumber`
- **DE:** Lieferscheinnummer (optional).
- **EN:** Delivery note number (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Lines[].Description`
- **DE:** Positionsbezeichnung / Artikelname.
- **EN:** Line description / item name.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:IncludedSupplyChainTradeLineItem[*]/ram:SpecifiedTradeProduct/ram:Name`

## `Lines[].GlobalId`
- **DE:** Globale Artikel-ID (z.B. GTIN) – optional.
- **EN:** Global item identifier (e.g., GTIN) – optional.

- **XML (XPath):** `/.../ram:SpecifiedTradeProduct/ram:GlobalID`

## `Lines[].GlobalIdSchemeId`
- **DE:** Schema der GlobalId (z.B. 0160 für GTIN).
- **EN:** Scheme for GlobalId (e.g., 0160 for GTIN).

- **XML (XPath):** `/.../ram:SpecifiedTradeProduct/ram:GlobalID/@schemeID`

## `Lines[].OriginCountryCode`
- **DE:** Ursprungsland des Artikels (optional).
- **EN:** Country of origin (optional).

- **XML (XPath):** `/.../ram:SpecifiedLineTradeDelivery/ram:ShipToTradeParty/ram:PostalTradeAddress/ram:CountryID  (or /ram:OriginTradeCountry/ram:ID depending on mapping)`

## `Lines[].ProductDescription`
- **DE:** Kategorie/zusätzliche Beschreibung (optional).
- **EN:** Additional product description/category (optional).

- **XML (XPath):** `/.../ram:SpecifiedTradeProduct/ram:Description`

## `Lines[].Quantity`
- **DE:** Menge der Position.
- **EN:** Quantity.

- **XML (XPath):** `/.../ram:SpecifiedLineTradeDelivery/ram:BilledQuantity`

## `Lines[].SellerAssignedId`
- **DE:** Artikelnummer des Verkäufers (optional).
- **EN:** Seller assigned item ID/SKU (optional).

- **XML (XPath):** `/.../ram:SpecifiedTradeProduct/ram:SellerAssignedID`

## `Lines[].TaxCategory`
- **DE:** Steuerkategorie, z.B. S=Standard, Z=Zero-rated.
- **EN:** Tax category code, e.g. S=Standard, Z=Zero-rated.

- **XML (XPath):** `/.../ram:SpecifiedLineTradeSettlement/ram:ApplicableTradeTax/ram:CategoryCode`

## `Lines[].TaxPercent`
- **DE:** Steuersatz in Prozent.
- **EN:** Tax rate percentage.

- **XML (XPath):** `/.../ram:SpecifiedLineTradeSettlement/ram:ApplicableTradeTax/ram:RateApplicablePercent`

## `Lines[].UnitCode`
- **DE:** Mengeneinheit (UN/ECE Rec 20), z.B. H87=Stück, C62=Einheit.
- **EN:** Unit code (UN/ECE Rec 20), e.g. H87=piece, C62=unit.

- **XML (XPath):** `/.../ram:SpecifiedLineTradeDelivery/ram:BilledQuantity/@unitCode`

## `Lines[].UnitPrice`
- **DE:** Preis pro Einheit (i.d.R. Netto) gemäß deinem Mapper.
- **EN:** Unit price (typically net) as used by your mapper.

- **XML (XPath):** `/.../ram:SpecifiedLineTradeAgreement/ram:NetPriceProductTradePrice/ram:ChargeAmount`

## `Notes[].Content`
- **DE:** Notiztext.
- **EN:** Note text.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IncludedNote[*]/ram:Content`

## `Notes[].ContentCode`
- **DE:** Optionaler Inhalts-Code (oft leer).
- **EN:** Optional content code (often empty).

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IncludedNote[*]/ram:ContentCode`

## `Notes[].SubjectCode`
- **DE:** Notiz-Typ/Code (z.B. REG, AAI, PMT).
- **EN:** Note subject/type code (e.g., REG, AAI, PMT).

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocument/ram:IncludedNote[*]/ram:SubjectCode`

## `Payment.AccountName`
- **DE:** Kontoinhabername (optional).
- **EN:** Account holder name (optional).

- **XML (XPath):** `/.../ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeePartyCreditorFinancialAccount/ram:AccountName`

## `Payment.Bic`
- **DE:** BIC/SWIFT des Zahlungsempfängers (optional, je nach Land/Bank).
- **EN:** Payee BIC/SWIFT (optional depending on bank/country).

- **XML (XPath):** `/.../ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeeSpecifiedCreditorFinancialInstitution/ram:BICID`

## `Payment.DirectDebitMandateID`
- **DE:** Mandatsreferenz für SEPA-Lastschrift (nur falls genutzt).
- **EN:** Mandate reference for SEPA direct debit (only if used).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.DueDate`
- **DE:** Fälligkeitsdatum der Zahlung (optional).
- **EN:** Payment due date (optional).

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradePaymentTerms/ram:DueDateDateTime/udt:DateTimeString`

## `Payment.Iban`
- **DE:** IBAN des Zahlungsempfängers (Überweisung).
- **EN:** Payee IBAN (credit transfer).

- **XML (XPath):** `/.../ram:SpecifiedTradeSettlementPaymentMeans/ram:PayeePartyCreditorFinancialAccount/ram:IBANID`

## `Payment.PaymentMeansInformation`
- **DE:** Zusätzliche Infos zur Zahlungsart (frei).
- **EN:** Additional information about payment means (free text).

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementPaymentMeans/ram:Information`

## `Payment.PaymentMeansTypeCode`
- **DE:** Zahlungsart-Code, z.B. 58=SEPA Credit Transfer.
- **EN:** Payment means type code, e.g. 58=SEPA credit transfer.

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradeSettlementPaymentMeans/ram:TypeCode`

## `Payment.PaymentReference`
- **DE:** Zahlungsreferenz/Verwendungszweck.
- **EN:** Payment reference/remittance information.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeSettlement/ram:PaymentReference`

## `Payment.PaymentTermsText`
- **DE:** Zahlungsbedingungen als Text (frei).
- **EN:** Payment terms as free text.

- **XML (XPath):** `/.../ram:ApplicableHeaderTradeSettlement/ram:SpecifiedTradePaymentTerms/ram:Description`

## `Payment.Terms`
- **DE:** Erweiterte Zahlungsbedingungen/Skonto-Stufen (optional).
- **EN:** Extended payment terms/discount tiers (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Seller.City`
- **DE:** Ort/Stadt.
- **EN:** City.

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:PostalTradeAddress/ram:CityName`

## `Seller.ContactEmail`
- **DE:** E-Mail Ansprechpartner (optional).
- **EN:** Contact email (optional).

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:DefinedTradeContact/ram:EmailURIUniversalCommunication/ram:URIID`

## `Seller.ContactPersonName`
- **DE:** Ansprechpartner (optional).
- **EN:** Contact person (optional).

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:DefinedTradeContact/ram:PersonName`

## `Seller.ContactPhone`
- **DE:** Telefon Ansprechpartner (optional).
- **EN:** Contact phone (optional).

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:DefinedTradeContact/ram:TelephoneUniversalCommunication/ram:CompleteNumber`

## `Seller.CountryCode`
- **DE:** Ländercode nach ISO 3166-1 alpha-2, z.B. DE.
- **EN:** Country code (ISO 3166-1 alpha-2), e.g. DE.

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:PostalTradeAddress/ram:CountryID`

## `Seller.FiscalId`
- **DE:** Steuernummer/Tax Number (optional).
- **EN:** Fiscal ID / tax number (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Seller.GeneralEmail`
- **DE:** Allgemeine E-Mail-Adresse (optional).
- **EN:** General email address (optional).

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:DefinedTradeContact/ram:EmailURIUniversalCommunication/ram:URIID`

## `Seller.ID`
- **DE:** Interne/ERP-Kundennummer des Verkäufers (optional).
- **EN:** Seller internal/ERP identifier (optional).

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:SellerTradeParty/ram:ID`

## `Seller.LeitwegId`
- **DE:** Leitweg-ID (XRechnung) – i.d.R. beim Verkäufer leer.
- **EN:** Leitweg-ID (German e-invoicing routing ID) – usually empty for seller.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Seller.Name`
- **DE:** Firmenname des Verkäufers.
- **EN:** Seller legal/trade name.

- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:SupplyChainTradeTransaction/ram:ApplicableHeaderTradeAgreement/ram:SellerTradeParty/ram:Name`

## `Seller.Street`
- **DE:** Straße + Hausnummer.
- **EN:** Street and house number.

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:PostalTradeAddress/ram:LineOne`

## `Seller.VatId`
- **DE:** USt-IdNr. des Verkäufers.
- **EN:** Seller VAT ID (VAT registration number).

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:SpecifiedTaxRegistration/ram:ID[@schemeID='VA']`

## `Seller.Zip`
- **DE:** Postleitzahl.
- **EN:** Postal code.

- **XML (XPath):** `/.../ram:SellerTradeParty/ram:PostalTradeAddress/ram:PostcodeCode`

## `Payment.Terms[].Description`
- **DE:** Beschreibung der Skonto-/Zahlungsstufe.
- **EN:** Description of the discount/payment tier.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DiscountTerms`
- **DE:** Skonto-Details (optional).
- **EN:** Cash discount terms (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DueDate`
- **DE:** Fälligkeit dieser Stufe (optional).
- **EN:** Due date for this tier (optional).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DiscountTerms.ActualDiscountAmount`
- **DE:** Konkreter Skonto-Betrag (falls vorab berechnet).
- **EN:** Actual discount amount (if pre-calculated).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DiscountTerms.BasisAmount`
- **DE:** Bemessungsgrundlage (Betrag), auf den Prozent angewendet wird.
- **EN:** Base amount to which the percent is applied.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DiscountTerms.BasisDate`
- **DE:** Bezugsdatum für Skonto-Berechnung (z.B. Rechnungsdatum).
- **EN:** Basis date for discount calculation (e.g., invoice date).

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DiscountTerms.BasisPeriodDays`
- **DE:** Tage bis Skonto-Frist.
- **EN:** Number of days for discount period.

- **XML (XPath):** *(not mapped / not used by current generator)*

## `Payment.Terms[].DiscountTerms.CalculationPercent`
- **DE:** Skonto-Prozentsatz.
- **EN:** Discount percentage.
- **XML (XPath):** *(not mapped / not used by current generator)*

## `GuidelineId (profile)`
- **DE:** Profil-/Guideline-URN, z.B. Factur-X/ZUGFeRD EXTENDED. Das sitzt im DocumentContext und wird oft vom Generator fest vorgegeben.
- **EN:** Profile/guideline URN (e.g., Factur-X/ZUGFeRD EXTENDED). Located in the document context and often hard-coded by the generator.
- **XML (XPath):** `/rsm:CrossIndustryInvoice/rsm:ExchangedDocumentContext/ram:GuidelineSpecifiedDocumentContextParameter/ram:ID`
