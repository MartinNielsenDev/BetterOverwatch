using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Text;
using System.Xml;

namespace OverwatchTracker
{
    class Server
    {
        public static Stopwatch autoUpdaterTimer = new Stopwatch();
        public static void autoUpdater()
        {
            if (autoUpdaterTimer.ElapsedMilliseconds >= 600000)
            {
                newestVersion(false);
                autoUpdaterTimer.Restart();
            }
        }
        public static void openUpdate()
        {
            string argument = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\" & Start \"\" /D \"{3}\" \"{4}\" {5}";
            string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"overwatchtracker\overwatchtracker.exe");
            string currentPath = Application.ExecutablePath;

            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = String.Format(argument, currentPath, tempPath, currentPath, Path.GetDirectoryName(currentPath), Path.GetFileName(currentPath), "");
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }
        public static bool downloadUpdate(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(url, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"overwatchtracker\overwatchtracker.exe"));
                    return true;
                }
                catch { }
            }
            return false;
        }
        public static bool newestVersion(bool firstRun = true)
        {
            Debug.WriteLine("checking newest version");
            try
            {
                string serverResponse;
                using (var client = new WebClient())
                {
                    serverResponse = client.DownloadString(Vars.host + "/api/version.xml");
                }
                XmlDocument xmlTest = new XmlDocument();
                xmlTest.LoadXml(serverResponse);
                XmlNodeList xmlNodeList = xmlTest.SelectNodes("update");

                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    string serverVersion = xmlNode.SelectSingleNode("version").InnerText;

                    if (serverVersion.Length >= 6)
                    {
                        DateTime serverDate = DateTime.ParseExact(serverVersion, "yy.MMdd", null);
                        DateTime thisDate = DateTime.ParseExact(Vars.version, "yy.MMdd", null);

                        if ((thisDate - serverDate).TotalSeconds < 0)
                        {
                            Functions.DebugMessage("New update required: v" + serverVersion);
                            UpdateNoficationForm updateForm = new UpdateNoficationForm();
                            updateForm.label2.Text = "Current Version: " + Vars.version;
                            updateForm.label3.Text = "Newest Version: " + serverVersion;
                            updateForm.textBox1.Text = xmlNode.SelectSingleNode("changelog").InnerText;
                            updateForm.urlToDownload = xmlNode.SelectSingleNode("url").InnerText;
                            Application.Run(updateForm);
                            return false;
                        }
                        try
                        {
                            Vars.blizzardAppOffset = Convert.ToInt32(xmlNode.SelectSingleNode("blizzardapp").InnerText, 16);
                        }
                        catch { }
                    }
                }
                return true;
            }
            catch
            {
                if (firstRun)
                {
                    Functions.DebugMessage("error while validating version");
                    MessageBox.Show("Error while validating version", "Overwatch Tracker v" + Vars.version, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }
        public static void uploadGame(string gameData, Bitmap image)
        {
            
            Program.uploaderThread = new Thread(() => uploadGameData(gameData, image));
            Program.uploaderThread.Start();
        }
        private static void uploadGameData(string gameData, Bitmap image)
        {
            Functions.DebugMessage("Uploading gamedata...");

            for (var i = 1; i <= 10; i++)
            {
                string uploadResult = gameUploader(gameData);

                if (uploadResult.Contains("success"))
                {
                    Functions.DebugMessage("Successfully uploaded game");
                    if (Vars.settings.uploadScreenshot && image != null)
                    {
                        for (int e = 1; e <= 10; e++)
                        {
                            Functions.DebugMessage("Uploading playerlist image...");
                            if (uploadPlayerListImage(image, uploadResult.Replace("success", String.Empty)) == "success")
                            {
                                Functions.DebugMessage("Successfully uploaded playerlist image");
                                break;
                            }
                            Thread.Sleep(500);
                        }
                    }
                    break;
                }
                else
                {
                    Functions.DebugMessage("Game upload failed");
                }
                Thread.Sleep(500);
            }
        }
        public static string uploadPlayerListImage(Bitmap image, string entryId)
        {
            try
            {
                byte[] bytes = Functions.imageToBytes(Functions.reduceImageSize(image, 70));
                string base64image = Convert.ToBase64String(bytes);

                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues(Vars.host + "/api/upload-image/", new NameValueCollection()
                    {
                        { "image", base64image },
                        {"entryId", entryId }
                    });

                    return Encoding.Default.GetString(response);
                }
            }
            catch { }
            return String.Empty;
        }
        public static string gameUploader(string gameData)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues(Vars.host + "/api/upload-game/", new NameValueCollection()
                        {
                            { "entry", gameData }
                        });

                    return Encoding.Default.GetString(response);
                }
            }
            catch { }
            return String.Empty;
        }
        public static string getToken(bool createUserOnFail = false)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues(Vars.host + "/api/get-token/", new NameValueCollection()
                {
                    { "privateToken", Vars.settings.privateToken },
                    { "publicToken", Vars.gameData.battletag },
                    { "createUserOnFail", createUserOnFail.ToString() }
                });
                    return Encoding.Default.GetString(response);
                }
            }
            catch { }
            return String.Empty;
        }
        public static string encryptRJ256(string prm_key, string prm_iv, string prm_text_to_encrypt)
        {
            var sToEncrypt = prm_text_to_encrypt;

            var myRijndael = new RijndaelManaged()
            {
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 256
            };

            var key = Encoding.ASCII.GetBytes(prm_key);
            var IV = Encoding.ASCII.GetBytes(prm_iv);

            var encryptor = myRijndael.CreateEncryptor(key, IV);

            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            var toEncrypt = Encoding.ASCII.GetBytes(sToEncrypt);

            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();

            var encrypted = msEncrypt.ToArray();

            return (Convert.ToBase64String(encrypted));
        }
        public static string computeHash(string plainText, string hashAlgorithm, byte[] saltBytes = null)
        {
            if (saltBytes == null)
            {
                int minSaltSize = 4;
                int maxSaltSize = 8;

                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);

                saltBytes = new byte[saltSize];

                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                rng.GetNonZeroBytes(saltBytes);
            }

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

            HashAlgorithm hash;

            if (hashAlgorithm == null)
                hashAlgorithm = "";

            switch (hashAlgorithm.ToUpper())
            {
                case "SHA1":
                    hash = new SHA1Managed();
                    break;

                case "SHA256":
                    hash = new SHA256Managed();
                    break;

                case "SHA384":
                    hash = new SHA384Managed();
                    break;

                case "SHA512":
                    hash = new SHA512Managed();
                    break;

                default:
                    hash = new MD5CryptoServiceProvider();
                    break;
            }

            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            string hashValue = Convert.ToBase64String(hashWithSaltBytes);

            return hashValue;
        }
        public static string md5Hash(string plainText)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(plainText);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }
    }
}