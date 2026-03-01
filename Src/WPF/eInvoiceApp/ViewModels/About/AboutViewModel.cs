using System.Windows.Input;
using tulo.CommonMVVM.Collector;
using tulo.CommonMVVM.ViewModels;
using tulo.ResourcesWpfLib.Commands;

namespace tulo.eInvoice.eInvoiceApp.ViewModels.About;
public class AboutViewModel : BaseViewModel
{
    public string Version { get; } = $"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

    public string DisclaimerText { get; } = @"
# ⚖️ Rechtliche Hinweise & Haftungsausschluss

**WICHTIG: Bitte lesen Sie diese Hinweise sorgfältig durch, bevor Sie die Software verwenden.**

### 1. Kein Steuerberatungsersatz
Diese Software ist ein technisches Hilfsmittel zur Erzeugung von E-Rechnungsformaten (ZUGFeRD/CII/UBL). Die Bereitstellung dieser Software stellt **keine Steuerberatung** und keine Rechtsberatung dar. Der Entwickler übernimmt keine Prüfung der steuerrechtlichen Richtigkeit Ihrer Angaben.

### 2. Haftungsausschluss (Software)
Die Software wird ""wie besehen"" (AS IS) und ohne jegliche ausdrückliche oder stillschweigende Gewährleistung zur Verfügung gestellt. 
* **Keine Garantie:** Der Entwickler garantiert nicht, dass die erzeugten Dateien den Anforderungen der Finanzbehörden oder spezifischen ERP-Systemen der Empfänger entsprechen.
* **Schadenersatz:** In keinem Fall haftet der Entwickler für Schäden (einschließlich, aber nicht beschränkt auf, entgangenen Gewinn, Betriebsunterbrechung oder Verlust von geschäftlichen Informationen), die aus der Nutzung oder der Unfähigkeit zur Nutzung dieser Software entstehen.

### 3. Eigenverantwortung des Nutzers
Als Aussteller einer Rechnung sind Sie allein für deren Inhalt gemäß **§ 14 UStG** verantwortlich. 
* Sie sind verpflichtet, jede erzeugte Rechnung vor dem Versand technisch und inhaltlich zu prüfen.
* Wir empfehlen dringend die Validierung über offizielle Tools wie den **[Kosit-Validator](https://github.com/itplr-kosit/validator)**.

### 4. Open Source & Lizenzen
Dieses Programm ist Freeware und unter der **MIT-Lizenz** veröffentlicht. Es verwendet Drittanbieter-Bibliotheken (u.a. Markdig.Wpf), deren Lizenzen respektiert werden müssen.

---
### ❤️ Unterstützung
Dieses Tool ist ein privates Projekt. Wenn es Ihnen hilft, freue ich mich über eine Anerkennung:
* ☕ [Kaffee spendieren (PayPal)](https://www.paypal.me)
* ⭐ [Projekt auf GitHub folgen](https://github.com)
* ⭐ [Online ZUGFeRD Validator](https://www.portinvoice.com/en/)

*Stand: Februar 2026*
";

    public ICommand OpenHyperlinkCommand { get; }

    private readonly ICollectorCollection _collectorCollection;
    public AboutViewModel(ICollectorCollection collectorCollection)
    {
        _collectorCollection = collectorCollection;
        OpenHyperlinkCommand = new OpenHyperlinkCommand();
    }
}
