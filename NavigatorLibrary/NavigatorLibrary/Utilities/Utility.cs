using NavigatorLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NavigatorLibrary.Utilities
{
    public static class Utility
    {
        public static IBeaconScan _ibeaconScan;
        public static LBeaconScan _lbeaconScan;
        public static ITextToSpeech _textToSpeech;

        private static bool ValidateServerCertificate(Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Download navigation graph from specified server
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="navigraphName"></param>
        /// <returns></returns>
        public static bool DownloadNavigraph(string URL, string navigraphName)
        {
            string filePath = Path.Combine(NavigraphStorage._navigraphFolder,
                                            navigraphName);
            try
            {
                if (!Directory.Exists(NavigraphStorage._navigraphFolder))
                    Directory.CreateDirectory(
                        NavigraphStorage._navigraphFolder);

                using (WebClient webClient = new WebClient())
                    webClient.DownloadFileAsync(new Uri(URL), filePath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public static bool DownloadFirstDirectionFile(string URL,
                                                      string fileName)
        {
            string filePath =
                Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder,
                             fileName);
            try
            {
                if (!Directory.Exists(NavigraphStorage
                    ._firstDirectionInstuctionFolder))

                    Directory.CreateDirectory(
                        NavigraphStorage._firstDirectionInstuctionFolder);

                using (WebClient webClient = new WebClient())
                    webClient.DownloadFileAsync(new Uri(URL), filePath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        public static bool DownloadInformationFile(string URL, string fileName)
        {
            string filePath =
                Path.Combine(NavigraphStorage._informationFolder, fileName);
            try
            {
                if (!Directory.Exists(NavigraphStorage._informationFolder))
                    Directory.CreateDirectory(
                        NavigraphStorage._informationFolder);

                using (WebClient webClient = new WebClient())
                    webClient.DownloadFileAsync(new Uri(URL), filePath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
