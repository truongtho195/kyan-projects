using System;
using System.Linq;
using System.IO;
using System.Net;

namespace FlashCard.Helper
{
    public static class GetSoundFromGoogleTranslate
    {


        /// <summary>
        /// Check & Create Directory , get sound with keyword & store in pathFile
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pathFile"></param>
        public static Stream GetSoundGoogle(string keyword, string pathFile)
        {
            if (!string.IsNullOrWhiteSpace(pathFile))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(pathFile);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
            }
            return GetSound(keyword, pathFile);
        }




        private static Stream GetSound(string keyword, string pathFile)
        {
            try
            {
                string strUrl = string.Format("{0}{1}&tl=en", "http://translate.google.com/translate_tts?q=", keyword);

                if (keyword.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
                    keyword = CleanFileName(keyword);

                var ur = new Uri(strUrl, UriKind.RelativeOrAbsolute);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ur);
                WebResponse response = request.GetResponse();
                Stream strm = response.GetResponseStream();
                //Is User Add File Path => Save To file else return stream
                if (!string.IsNullOrWhiteSpace(pathFile))
                {
                    string fullPathFile = string.Format("{0}/{1}.mp3", pathFile, keyword);
                    if (!File.Exists(fullPathFile))
                    {
                        if (strm.CanRead & !File.Exists(fullPathFile))
                        {
                            SaveStreamToFile(strm, fullPathFile);
                        }
                    }
                }
                return strm;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        private static void SaveStreamToFile(Stream stream, string filename)
        {
            using (Stream destination = File.Create(filename))
                Write(stream, destination);
        }


        //Typically I implement this Write method as a Stream extension method. 
        //The framework handles buffering.
        private static void Write(Stream from, Stream to)
        {
            for (int a = from.ReadByte(); a != -1; a = from.ReadByte())
                to.WriteByte((byte)a);
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }


    }

}
