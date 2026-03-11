using System.Reflection;
using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.ResourcesWpfLib.Commands;

namespace tulo.eInvoice.eInvoiceViewer.ViewModels;
public class AboutViewModel : BaseViewModel
{
    public string Version { get; } = GetProgramVersion();

    public string DisclaimerText { get; } = @"
## ⚖️ Legal Notice & Disclaimer / Rechtliche Hinweise & Haftungsausschluss / Aviso legal y exención de responsabilidad

**Important: Please read these notes carefully before using the software.**  
**WICHTIG: Bitte lesen Sie diese Hinweise sorgfältig durch, bevor Sie die Software verwenden.**  
**Importante: Lea atentamente estas indicaciones antes de utilizar el software.**  

### ❤️ Support / Unterstützung / Apoyo
This tool is a private project. If it helps you, I would appreciate your support.  
Dieses Tool ist ein privates Projekt. Wenn es Ihnen hilft, freue ich mich über eine Anerkennung.  
Esta herramienta es un proyecto privado. Si le resulta útil, agradeceré su apoyo.  

* ☕ [PayPal](https://www.paypal.me)
* ⭐ [GitHub](https://github.com)

---

### 1. No Substitute for Tax Advice
This software is a technical tool for displaying a CII XML invoice. The provision of this software does **not** constitute tax advice or legal advice. The developer does not verify the tax-related accuracy of your data.

The display is intended solely for informational purposes and to improve the readability of the loaded data. It does not constitute proof of factual, technical, or legal correctness.

### 2. Disclaimer (Software)
The software is provided ""as is"" (AS IS) without any express or implied warranty.  
* **No warranty:** The developer does not guarantee that the displayed content complies with the requirements of tax authorities or the specific ERP systems of recipients.
* **Liability for damages:** Under no circumstances shall the developer be liable for any damages (including, but not limited to, loss of profit, business interruption, or loss of business information) arising from the use of or inability to use this software.

### 3. User Responsibility
As the issuer of an invoice, you are solely responsible for its content.
* You are obliged to review each invoice technically and substantively before sending it.
* We strongly recommend validation using official tools such as the **[Kosit Validator](https://github.com/itplr-kosit/validator)**.

### 4. Open Source & Licenses
This program is freeware and published under the **MIT License**. It uses third-party libraries (including Serilog, PdfSharp, QRCoder, Svg, and Markdig.Wpf), whose licenses must be observed.

*As of: March 2026*

---

### 1. Kein Steuerberatungsersatz
Diese Software ist ein technisches Hilfsmittel zur Darstellung einer CII-XML-Rechnung. Die Bereitstellung dieser Software stellt **keine Steuerberatung** und keine Rechtsberatung dar. Der Entwickler übernimmt keine Prüfung der steuerrechtlichen Richtigkeit Ihrer Angaben.

Die Anzeige dient ausschließlich der Information und der besseren Lesbarkeit der geladenen Daten. Sie stellt keinen Nachweis der fachlichen, technischen oder rechtlichen Richtigkeit dar.

### 2. Haftungsausschluss (Software)
Die Software wird ""wie besehen"" (AS IS) und ohne jegliche ausdrückliche oder stillschweigende Gewährleistung zur Verfügung gestellt.  
* **Keine Garantie:** Der Entwickler garantiert nicht, dass die angezeigten Inhalte den Anforderungen der Finanzbehörden oder spezifischen ERP-Systeme der Empfänger entsprechen.
* **Schadenersatz:** In keinem Fall haftet der Entwickler für Schäden (einschließlich, aber nicht beschränkt auf entgangenen Gewinn, Betriebsunterbrechung oder Verlust von geschäftlichen Informationen), die aus der Nutzung oder der Unfähigkeit zur Nutzung dieser Software entstehen.

### 3. Eigenverantwortung des Nutzers
Als Aussteller einer Rechnung sind Sie allein für deren Inhalt verantwortlich. 
* Sie sind verpflichtet, jede Rechnung vor dem Versand technisch und inhaltlich zu prüfen.
* Wir empfehlen dringend die Validierung über offizielle Tools wie den **[Kosit-Validator](https://github.com/itplr-kosit/validator)**.

### 4. Open Source & Lizenzen
Dieses Programm ist Freeware und unter der **MIT-Lizenz** veröffentlicht. Es verwendet Drittanbieter-Bibliotheken (u. a. Serilog, PdfSharp, QRCoder, Svg und Markdig.Wpf), deren Lizenzen beachtet werden müssen.

*Stand: März 2026*

---

### 1. No sustituye el asesoramiento fiscal
Este software es una herramienta técnica para mostrar una factura XML CII. La puesta a disposición de este software **no** constituye asesoramiento fiscal ni asesoramiento jurídico. El desarrollador no verifica la exactitud fiscal de los datos proporcionados.

La visualización tiene únicamente fines informativos y para mejorar la legibilidad de los datos cargados. No constituye prueba de la exactitud fáctica, técnica o jurídica.

### 2. Exención de responsabilidad (software)
El software se proporciona ""tal cual"" (AS IS), sin ninguna garantía expresa ni implícita.  
* **Sin garantía:** El desarrollador no garantiza que el contenido mostrado cumpla con los requisitos de las autoridades fiscales o de los sistemas ERP específicos de los destinatarios.
* **Responsabilidad por daños:** En ningún caso el desarrollador será responsable de daños (incluidos, entre otros, la pérdida de beneficios, la interrupción de la actividad comercial o la pérdida de información empresarial) derivados del uso o de la imposibilidad de uso de este software.

### 3. Responsabilidad del usuario
Como emisor de una factura, usted es el único responsable de verificar la exactitud e integridad de su contenido.  
* Está obligado a revisar cada factura, tanto técnica como materialmente, antes de enviarla.
* Recomendamos encarecidamente la validación mediante herramientas oficiales como el **[Kosit Validator](https://github.com/itplr-kosit/validator)**.

### 4. Código abierto y licencias
Este programa es freeware y se publica bajo la **licencia MIT**. Utiliza bibliotecas de terceros (entre ellas Serilog, PdfSharp, QRCoder, Svg y Markdig.Wpf), cuyas licencias deben respetarse.

*Estado: marzo de 2026*
";

    public ICommand OpenHyperlinkCommand { get; }

    private readonly ICollectorCollection _collectorCollection;
    public AboutViewModel(ICollectorCollection collectorCollection)
    {
        _collectorCollection = collectorCollection;
        OpenHyperlinkCommand = new OpenHyperlinkCommand();
    }
    
    public static string GetProgramVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"Version: v{version.Major}.{version.Minor}.{version.Build}" : "Unknown Version";
    }
}
