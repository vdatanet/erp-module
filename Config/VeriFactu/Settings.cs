using System;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace erp.Module.Config.VeriFactu
{
    /// <summary>
    /// Configuración de VeriFactu con mecanismo de semáforo para evitar colisiones entre tenants.
    /// </summary>
    [Serializable]
    [XmlRoot("Settings")]
    public class Settings
    {
        #region Variables Privadas Estáticas

        private static readonly char _PathSep = System.IO.Path.DirectorySeparatorChar;
        private static Settings _Current;
        private static readonly string _Path = RuntimeInformation.IsOSPlatform(OSPlatform.Create("OSX")) || 
                                              RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")) || 
                                              RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")) 
            ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeriFactu") + _PathSep
            : System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VeriFactu") + _PathSep;

        private string _BlockchainPath;
        
        /// <summary>
        /// Semáforo para controlar el acceso concurrente a la configuración global.
        /// </summary>
        private static readonly SemaphoreSlim _Semaphore = new SemaphoreSlim(1, 1);

        #endregion

        #region Propiedades Privadas Estáticas

        internal static NumberFormatInfo DefaultNumberFormatInfo = new NumberFormatInfo();
        internal static string DefaultNumberDecimalSeparator = ".";
        internal static string FileName = "Settings.xml";
        internal static bool BlockchainInitialized;

        #endregion

        #region Constructores Estáticos

        static Settings()
        {
            DefaultNumberFormatInfo.NumberDecimalSeparator = DefaultNumberDecimalSeparator;
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            
            // Inicialización síncrona inicial
            LoadSync();
        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Carga la configuración de forma síncrona (usado en el constructor estático).
        /// </summary>
        private static void LoadSync()
        {
            _Semaphore.Wait();
            try
            {
                _Current = new Settings();
                string FullPath = System.IO.Path.Combine(Path, FileName);

                if (File.Exists(FullPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (StreamReader r = new StreamReader(FullPath))
                    {
                        _Current = (Settings)serializer.Deserialize(r);
                    }
                }
                else
                {
                    _Current = GetDefault();
                }

                CheckDirectories();
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        internal static string GetLocalMacAddress()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in networkInterfaces)
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    return $"{nic.GetPhysicalAddress()}";
            }
            return null;
        }

        internal static Settings GetDefault()
        {
            var numeroInstalacion = "01";
            try
            {
                var mac = GetLocalMacAddress();
                if (!string.IsNullOrEmpty(mac))
                    numeroInstalacion = mac;
            }
            catch (Exception)
            {
                // Silenciamos error de red
            }

            return new Settings()
            {
                IDVersion = "1.0",
                InboxPath = System.IO.Path.Combine(Path, "Inbox") + _PathSep,
                OutboxPath = System.IO.Path.Combine(Path, "Outbox") + _PathSep,
                BlockchainPath = System.IO.Path.Combine(Path, "Blockchains") + _PathSep,
                InvoicePath = System.IO.Path.Combine(Path, "Invoices") + _PathSep,
                LogPath = System.IO.Path.Combine(Path, "Log") + _PathSep,
                VeriFactuHashAlgorithm = "Sha256",
                VeriFactuHashInputEncoding = "UTF-8",
                SistemaInformatico = new SistemaInformatico()
                {
                    NIF = "B12959755",
                    NombreRazon = "IRENE SOLUTIONS SL",
                    NombreSistemaInformatico = Assembly.GetExecutingAssembly().GetName().Name,
                    IdSistemaInformatico = "01",
                    Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    NumeroInstalacion = numeroInstalacion,
                    TipoUsoPosibleSoloVerifactu = "S",
                    TipoUsoPosibleMultiOT = "S",
                    IndicadorMultiplesOT = "S"
                },
                SkipNifAeatValidation = true,
                SkipViesVatNumberValidation = true,
                LoggingEnabled = false
            };
        }

        private static void CheckDirectories()
        {
            var dirs = new string[] { Path, _Current.InboxPath, _Current.OutboxPath,
                _Current.BlockchainPath, _Current.InvoicePath, _Current.LogPath };

            foreach (var dir in dirs)
            {
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        #endregion

        #region Propiedades Públicas Estáticas

        /// <summary>
        /// Obtiene la configuración actual. Para modificarla, se debe usar el método UpdateAsync.
        /// </summary>
        public static Settings Current => _Current;

        public static string Path => _Path;

        #endregion

        #region Propiedades Públicas de Instancia

        [XmlElement("IDVersion")]
        public string IDVersion { get; set; }

        [XmlElement("InboxPath")]
        public string InboxPath { get; set; }

        [XmlElement("OutboxPath")]
        public string OutboxPath { get; set; }

        [XmlElement("BlockchainPath")]
        public string BlockchainPath
        {
            get => _BlockchainPath;
            set
            {
                if (_Current?.BlockchainPath != null && _Current.BlockchainPath != value
                    && Directory.Exists(_Current.BlockchainPath) 
                    && Directory.GetDirectories(_Current.BlockchainPath).Length > 0)
                {
                    throw new InvalidOperationException("No se puede cambiar el valor de 'BlockchainPath' si la carpeta no está vacía.");
                }
                _BlockchainPath = value;
            }
        }

        [XmlElement("InvoicePath")]
        public string InvoicePath { get; set; }

        [XmlElement("LogPath")]
        public string LogPath { get; set; }

        [XmlElement("CertificateSerial")]
        public string CertificateSerial { get; set; }

        [XmlElement("CertificateThumbprint")]
        public string CertificateThumbprint { get; set; }

        [XmlElement("CertificatePath")]
        public string CertificatePath { get; set; }

        [XmlElement("CertificatePassword")]
        public string CertificatePassword { get; set; }

        [XmlElement("VeriFactuEndPointPrefix")]
        public string VeriFactuEndPointPrefix { get; set; }

        [XmlElement("VeriFactuEndPointValidatePrefix")]
        public string VeriFactuEndPointValidatePrefix { get; set; }

        [XmlElement("VeriFactuHashAlgorithm")]
        public string VeriFactuHashAlgorithm { get; set; }

        [XmlElement("VeriFactuHashInputEncoding")]
        public string VeriFactuHashInputEncoding { get; set; }

        [XmlElement("SistemaInformatico")]
        public SistemaInformatico SistemaInformatico { get; set; }

        [XmlElement("Api")]
        public Api Api { get; set; }

        [XmlElement("SkipNifAeatValidation")]
        public bool SkipNifAeatValidation { get; set; }

        [XmlElement("SkipViesVatNumberValidation")]
        public bool SkipViesVatNumberValidation { get; set; }

        [XmlElement("LoggingEnabled")]
        public bool LoggingEnabled { get; set; }

        [XmlElement("DisableBlockchainDelete")]
        public bool DisableBlockchainDelete { get; set; }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Guarda la configuración actual protegiendo el acceso con un semáforo.
        /// </summary>
        public static async Task SaveAsync()
        {
            await _Semaphore.WaitAsync();
            try
            {
                CheckDirectories();
                string FullPath = System.IO.Path.Combine(Path, FileName);

                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (StreamWriter w = new StreamWriter(FullPath))
                {
                    serializer.Serialize(w, _Current);
                }
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        /// <summary>
        /// Ejecuta una acción de actualización de la configuración de forma atómica y guarda los cambios.
        /// </summary>
        /// <param name="updateAction">Acción que modifica la configuración.</param>
        public static async Task UpdateAsync(Action<Settings> updateAction)
        {
            await _Semaphore.WaitAsync();
            try
            {
                updateAction(_Current);
                
                CheckDirectories();
                string FullPath = System.IO.Path.Combine(Path, FileName);
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (StreamWriter w = new StreamWriter(FullPath))
                {
                    serializer.Serialize(w, _Current);
                }
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        /// <summary>
        /// Establece el archivo de configuración con el cual trabajar.
        /// </summary>
        public static async Task SetConfigFileNameAsync(string fileName)
        {
            await _Semaphore.WaitAsync();
            try
            {
                FileName = fileName;
                // Recargamos configuración
                string FullPath = System.IO.Path.Combine(Path, fileName);
                if (File.Exists(FullPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (StreamReader r = new StreamReader(FullPath))
                    {
                        _Current = (Settings)serializer.Deserialize(r);
                    }
                }
                else
                {
                    _Current = GetDefault();
                }
                CheckDirectories();
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        #endregion
    }

    [Serializable]
    public class SistemaInformatico
    {
        public string NIF { get; set; }
        public string NombreRazon { get; set; }
        public string NombreSistemaInformatico { get; set; }
        public string IdSistemaInformatico { get; set; }
        public string Version { get; set; }
        public string NumeroInstalacion { get; set; }
        public string TipoUsoPosibleSoloVerifactu { get; set; }
        public string TipoUsoPosibleMultiOT { get; set; }
        public string IndicadorMultiplesOT { get; set; }
    }

    [Serializable]
    public class Api
    {
        public string EndPointCreate { get; set; }
        public string EndPointCancel { get; set; }
        public string EndPointGetQrCode { get; set; }
        public string EndPointGetSellers { get; set; }
        public string EndPointGetRecords { get; set; }
        public string EndPointValidateNIF { get; set; }
        public string EndPointGetAeatInvoices { get; set; }
        public string EndPointGetFilteredList { get; set; }
        public string EndPointCreateBatch { get; set; }
        public string EndPointValidateNIFs { get; set; }
        public string EndPointValidateViesVatNumber { get; set; }
        public string ServiceKey { get; set; }
    }
}
