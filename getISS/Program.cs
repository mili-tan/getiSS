﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using Newtonsoft.Json.Linq;
using Copernicus.SSURL;
using System.Diagnostics;

namespace getISS
{
    class Program
    {
        static void Main(string[] args)
        {
            JArray clientArray = new JArray();
            if (!Directory.Exists("./qrcode"))
            {
                Directory.CreateDirectory("./qrcode");
            }
            string[] qrCodeURLs = File.ReadAllLines("./list-ssqrcode.text");

            foreach (string item in qrCodeURLs)
            {
                string myUrl = item;
                string[] fileShortFile = myUrl.Split('/');
                string fileName = string.Format(@".\qrcode\{0}", fileShortFile[fileShortFile.Count() - 1]);

                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(myUrl, fileName);

                if (File.Exists(fileName))
                {
                    QRCodeDecoder myDecoder = new QRCodeDecoder();
                    string mySSURL = myDecoder.decode(new QRCodeBitmapImage(new Bitmap(Image.FromFile(fileName))));
                    string linkNameMark = fileShortFile[fileShortFile.Count() - 1].Replace(".png", "").Replace(".jpg", "").Replace("xxoo", "");
                    string[] linkInfo = SSURL.Parse(mySSURL);

                    SSClientInfo client = new SSClientInfo();
                    client.remarks = linkNameMark;
                    client.method = linkInfo[0];
                    client.password = linkInfo[1];
                    client.server = linkInfo[2];
                    client.server_port = Convert.ToInt32(linkInfo[3]);
                    client.timeout = 5;
                    clientArray.Add(JObject.FromObject(client));
                }
            }

            JObject configs = new JObject();
            configs["configs"] = clientArray;
            configs["enabled"] = true;
            File.WriteAllText("gui-config.json", configs.ToString());

            if (File.Exists("./Shadowsocks.exe"))
            {
                Process.Start("Shadowsocks.exe");
            }

            Process.Start("explorer.exe", "https://www.google.com/");
        }

        class SSClientInfo
        {
            public string server { get; set; }
            public string password { get; set; }
            public string method { get; set; }
            public string remarks { get; set; }
            public int server_port { get; set; }
            public int timeout { get; set; }
        }
    }
}
