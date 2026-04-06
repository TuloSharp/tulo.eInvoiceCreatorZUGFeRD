using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.ResourcesWpfLib.Commands;

namespace tulo.eInvoiceCreatorZUGFeRD.ViewModels.About;
public class AboutViewModel : BaseViewModel
{
    public string Version { get; } = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

    public string DisclaimerText { get; } = @"
## ⚖️ Legal Notice & Disclaimer / Rechtliche Hinweise & Haftungsausschluss / Aviso legal y exención de responsabilidad

**Important: Please read these notes carefully before using the software.**  
**WICHTIG: Bitte lesen Sie diese Hinweise sorgfältig durch, bevor Sie die Software verwenden.**  
**Importante: Lea atentamente estas indicaciones antes de utilizar el software.**  

### ❤️ Support / Unterstützung / Apoyo
This tool is a private project. If it helps you, I would appreciate your support.  
Dieses Tool ist ein privates Projekt. Wenn es Ihnen hilft, freue ich mich über eine Anerkennung.  
Esta herramienta es un proyecto privado. Si le resulta útil, agradeceré su apoyo.  

* ☕ [PayPal](https://paypal.me/MarceloGuartanAndrad)
* ⭐ [GitHub](https://github.com/TuloSharp/tulo.eInvoiceCreatorZUGFeRD.git)

---

### 1. No Substitute for Tax or Legal Advice
This software is a technical tool for creating, converting, archiving, and digitally signing CII XML invoices in PDF/A-3 format. The provision of this software does **not** constitute tax advice or legal advice. The developer does not verify the tax-related or legal accuracy of your invoice data.

The generated documents are intended solely for informational and operational purposes. They do not constitute proof of factual, technical, or legal correctness or compliance with any applicable invoicing regulation.

### 2. Disclaimer (Software)
The software is provided ""as is"" (AS IS) without any express or implied warranty.  
* **No warranty:** The developer does not guarantee that the generated PDF/A-3 documents, embedded XML, or digital signatures comply with the requirements of tax authorities, auditors, or the specific ERP systems of recipients.
* **No guarantee of compliance:** The ZUGFeRD / Factur-X output produced by this software has not been certified by any official body. It is the user's responsibility to validate generated documents before use in productive or legal scenarios.
* **Liability for damages:** Under no circumstances shall the developer be liable for any damages (including, but not limited to, loss of profit, business interruption, loss of business information, or regulatory penalties) arising from the use of or inability to use this software.

### 3. User Responsibility
As the creator and issuer of an electronic invoice, you are solely responsible for its content, correctness, and legal validity.
* You are obliged to review each generated invoice technically and substantively before sending or archiving it.
* Digital signatures produced via this application depend on the certificates and keys provided by the user. The developer is not responsible for the validity, expiry, or trustworthiness of any certificate used.
* Validation using official tools such as the **[Kosit Validator](https://github.com/itplr-kosit/validator)** or **[Online ZUGFeRD Validator](https://www.portinvoice.com/en/)** is strongly recommended.

### 4. Open Source & Licenses
This program is freeware and published under the **Apache License Version 2.0**. It uses third-party libraries (including Serilog, PdfSharp-extended, Markdig.Wpf, and Google Material icons), whose licenses must be observed.

*As of: April 2026*

---

### 1. Kein Steuerberatungs- oder Rechtsberatungsersatz
Diese Software ist ein technisches Hilfsmittel zur Erstellung, Konvertierung, Archivierung und digitalen Signierung von CII-XML-Rechnungen im PDF/A-3-Format. Die Bereitstellung dieser Software stellt **keine Steuerberatung** und keine Rechtsberatung dar. Der Entwickler übernimmt keine Prüfung der steuerrechtlichen oder rechtlichen Richtigkeit Ihrer Rechnungsdaten.

Die erzeugten Dokumente dienen ausschließlich der Information und dem operativen Einsatz. Sie stellen keinen Nachweis der fachlichen, technischen oder rechtlichen Richtigkeit oder der Konformität mit geltenden Rechnungsstellungsvorschriften dar.

### 2. Haftungsausschluss (Software)
Die Software wird ""wie besehen"" (AS IS) und ohne jegliche ausdrückliche oder stillschweigende Gewährleistung zur Verfügung gestellt.  
* **Keine Garantie:** Der Entwickler garantiert nicht, dass die erzeugten PDF/A-3-Dokumente, eingebetteten XML-Daten oder digitalen Signaturen den Anforderungen von Finanzbehörden, Prüfern oder den spezifischen ERP-Systemen der Empfänger entsprechen.
* **Keine Konformitätsgarantie:** Die von dieser Software erzeugten ZUGFeRD- / Factur-X-Ausgaben wurden von keiner offiziellen Stelle zertifiziert. Es liegt in der Verantwortung des Nutzers, erzeugte Dokumente vor dem Einsatz in produktiven oder rechtlichen Szenarien zu validieren.
* **Schadenersatz:** In keinem Fall haftet der Entwickler für Schäden (einschließlich, aber nicht beschränkt auf entgangenen Gewinn, Betriebsunterbrechung, Verlust von Geschäftsinformationen oder behördliche Sanktionen), die aus der Nutzung oder der Unfähigkeit zur Nutzung dieser Software entstehen.

### 3. Eigenverantwortung des Nutzers
Als Ersteller und Aussteller einer elektronischen Rechnung sind Sie allein für deren Inhalt, Richtigkeit und rechtliche Gültigkeit verantwortlich.
* Sie sind verpflichtet, jede erzeugte Rechnung vor dem Versand oder der Archivierung technisch und inhaltlich zu prüfen.
* Digitale Signaturen, die über diese Anwendung erzeugt werden, hängen von den vom Nutzer bereitgestellten Zertifikaten und Schlüsseln ab. Der Entwickler übernimmt keine Verantwortung für die Gültigkeit, den Ablauf oder die Vertrauenswürdigkeit verwendeter Zertifikate.
* Die Validierung über offizielle Tools wie den **[Kosit-Validator](https://github.com/itplr-kosit/validator)** oder **[Online ZUGFeRD Validator](https://www.portinvoice.com/en/)** wird dringend empfohlen.

### 4. Open Source & Lizenzen
Dieses Programm ist Freeware und unter der **Apache License Version 2.0** veröffentlicht. Es verwendet Drittanbieter-Bibliotheken (u. a. Serilog, PdfSharp-extended, Markdig.Wpf und Google Material icons), deren Lizenzen beachtet werden müssen.

*Stand: April 2026*

---

### 1. No sustituye el asesoramiento fiscal ni jurídico
Este software es una herramienta técnica para crear, convertir, archivar y firmar digitalmente facturas XML CII en formato PDF/A-3. La puesta a disposición de este software **no** constituye asesoramiento fiscal ni jurídico. El desarrollador no verifica la exactitud fiscal o legal de los datos de factura proporcionados.

Los documentos generados tienen únicamente fines informativos y operativos. No constituyen prueba de la exactitud fáctica, técnica o jurídica ni del cumplimiento de ninguna normativa de facturación aplicable.

### 2. Exención de responsabilidad (software)
El software se proporciona ""tal cual"" (AS IS), sin ninguna garantía expresa ni implícita.  
* **Sin garantía:** El desarrollador no garantiza que los documentos PDF/A-3 generados, el XML integrado o las firmas digitales cumplan con los requisitos de las autoridades fiscales, auditores o los sistemas ERP específicos de los destinatarios.
* **Sin garantía de conformidad:** Los documentos ZUGFeRD / Factur-X producidos por este software no han sido certificados por ningún organismo oficial. Es responsabilidad del usuario validar los documentos generados antes de utilizarlos en escenarios productivos o legales.
* **Responsabilidad por daños:** En ningún caso el desarrollador será responsable de daños (incluidos, entre otros, la pérdida de beneficios, la interrupción de la actividad comercial, la pérdida de información empresarial o sanciones regulatorias) derivados del uso o de la imposibilidad de uso de este software.

### 3. Responsabilidad del usuario
Como creador y emisor de una factura electrónica, usted es el único responsable de su contenido, exactitud y validez legal.
* Está obligado a revisar cada factura generada, tanto técnica como materialmente, antes de enviarla o archivarla.
* Las firmas digitales producidas mediante esta aplicación dependen de los certificados y claves proporcionados por el usuario. El desarrollador no se hace responsable de la validez, caducidad o fiabilidad de ningún certificado utilizado.
* Se recomienda encarecidamente la validación mediante herramientas oficiales como el **[Kosit Validator](https://github.com/itplr-kosit/validator)** o el **[Online ZUGFeRD Validator](https://www.portinvoice.com/en/)**.

### 4. Código abierto y licencias
Este programa es freeware y se publica bajo la **Apache License Version 2.0**. Utiliza bibliotecas de terceros (entre ellas Serilog, PdfSharp-extended, Markdig.Wpf y Google Material icons), cuyas licencias deben respetarse.

*Estado: abril de 2026*
";


    public ICommand OpenHyperlinkCommand { get; }

    private readonly ICollectorCollection _collectorCollection;
    public AboutViewModel(ICollectorCollection collectorCollection)
    {
        _collectorCollection = collectorCollection;
        OpenHyperlinkCommand = new OpenHyperlinkCommand();
    }
}
