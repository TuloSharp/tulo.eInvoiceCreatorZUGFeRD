using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;
using System.Reflection;
using tulo.eInvoice.eInvoiceApp.Properties;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace tulo.eInvoice.eInvoiceApp.Utilities
{
    /// <summary>
    /// Has some functions to verify that settings will be remain properly after update.
    /// <br/><br/>
    /// 1. Updates user.config after software update (new assembly)<br/>
    /// 2. Copys user.config as backup to predefined or default backup directory
    /// </summary>
    public class SettingsPropertyUpdateUtility
    {
        /// <summary>
        /// Project name where the helper gets executed (whole namespace)
        /// </summary>
        private readonly string _projectName = Assembly.GetExecutingAssembly()?.GetName().Name!;

        /// <summary>
        /// Current version of EntryAssembly
        /// </summary>
        private readonly string _version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()!;

        /// <summary>
        /// Path to user.config backup defined in appsettings.json
        /// </summary>
        private string _userConfigBackupPath;

        /// <summary>
        /// Application name
        /// </summary>
        readonly string _applicationName = "";

        /// <summary>
        /// Unique identifier for application
        /// </summary>
        readonly string _hash;

        Configuration _configuration;

        public SettingsPropertyUpdateUtility(IConfiguration configurationContext)
        {
            _applicationName = _projectName.Split(".")[0];
            _projectName = _projectName.Substring(_projectName.LastIndexOf(".") + 1);  // extracts only the last name which is the project name

            string customBackupPath = configurationContext.GetValue<string>("UserConfigBackup:Custom")!;

            string environementVariableAppData = configurationContext.GetValue<string>("UserConfigBackup:Default:Appdata")!;

            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);
            _hash = ExtractHashFromPath(_configuration.FilePath);

            _userConfigBackupPath = GetDefaultPathIfCustomIsEmpty(customBackupPath, environementVariableAppData);

            RefreshCurrentSettings();

            CopyLocalUserConfig(_configuration);
        }

        /// <summary>
        /// Extracts hash from Path. 
        /// </summary>
        /// <param name="filePath">Filepath where hash should be extracted</param>
        /// <returns>hash as string</returns>
        public static string ExtractHashFromPath(string filePath)
        {
            // +4 because we also take the "Uri_" chars into account, otherwise the hash would be incomplete
            return filePath.Substring(filePath.IndexOf("_") + 1, 32 + 4);
        }

        /// <summary>
        /// Checks if the custom userconfig path is valid if not it will set its default path to appdata backupfolder since we consider the use case will always be "appdata".
        /// </summary>
        /// <param name="customBackupPath">the custom path from appsettings</param>
        /// <param name="environmentVariable">the environment variable </param>
        /// <returns>Valid path as string</returns>
        /// <exception cref="ArgumentNullException">If the environmentVariable would not be recognizable exception would be thrown</exception>
        private string GetDefaultPathIfCustomIsEmpty(string customBackupPath, string environmentVariable)
        {
            if (!string.IsNullOrWhiteSpace(customBackupPath))
                return customBackupPath;

            if (string.IsNullOrWhiteSpace(environmentVariable))
                environmentVariable = "%APPDATA%\\";   // or "%LOCALAPPDATA%\\"

            var basePath = Environment.ExpandEnvironmentVariables(environmentVariable);

            return Path.Combine(basePath, "Backup", _applicationName, _hash, _version) + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Loads settings after update, so it updates the user.config itself as soon as new assembly is recognized
        /// on startup it checks if the updatesetting property is true (which is the case in default) therefore
        /// the application nows it should make an upgrade, save and reload. 
        /// </summary>
        void RefreshCurrentSettings()
        {
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade(); // Intruct providers to update their settings 
                Settings.Default.UpdateSettings = false; // Set property to false no longer needed because its already a new version initialized 
                Settings.Default.Save();
                Settings.Default.Reload();
            }
        }

        /// <summary>
        /// Makes a copy of productive /Appdata/local/xxx/user.config to specific directory which is defined in appsettings.json <br/> Takes overwrites existing file if parameter is not false.
        /// </summary>
        /// <param name="configuration">Take the configuration</param>
        /// <param name="overwrite">Overwrites existing file if true otherwise if its false exception gets thrown.<b> default:</b> true</param>
        /// <returns>true if copy was successful otherwise false</returns>
        private bool CopyLocalUserConfig(Configuration configuration, bool overwrite = true)
        {
            if (false == configuration.HasFile) { return false; }
            string source = configuration.FilePath;
            string target = _userConfigBackupPath + DateTime.Now.ToString("dd'-'MM'-'y'_'HH'-'mm'-'ss") + "_" + "user.config";
            DirectoryInfo di = Directory.CreateDirectory(_userConfigBackupPath); // Creates directory only if not available
            RemoveFilesOlderThanTwoDays(di);
            File.Copy(source, target, overwrite);
            return true;
        }

        /// <summary>
        /// Removes backup files which are older then two days
        /// </summary>
        /// <param name="directoryInfo">Info of passed directory</param>
        private void RemoveFilesOlderThanTwoDays(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.GetFiles().Length < 9) { return; }

            FileSystemInfo[] infos = directoryInfo.GetFileSystemInfos();

            foreach (FileSystemInfo info in infos)
            {
                if ((DateTime.Now - info.LastAccessTime).TotalDays > 2)
                {
                    File.Delete(info.FullName);
                }
            }
        }
    }
}
