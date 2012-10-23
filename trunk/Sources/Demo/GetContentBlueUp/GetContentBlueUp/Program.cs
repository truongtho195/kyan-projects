using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace GetContentBlueUp
{
    class Program
    {
        static String Rstring;
        static void Main(string[] args)
        {

            listCard();
            //var lessonModel = GetContent();
            Console.ReadLine();

        }
        static LessonModel GetContent()
        {
            WebRequest myWebRequest;
            WebResponse myWebResponse;
            String URL = "http://www.blueup.vn/index2.php?option=com_word&task=getWordById&word_id=777";
            myWebRequest = WebRequest.Create(URL);
            myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource
            Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet
            //and save it in the stream
            StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
            Rstring = sreader.ReadToEnd();//reads it to the end
            var doc = new HtmlDocument();
            doc.LoadHtml(Rstring);

            //Create new LessonModel
            var lessonModel = new LessonModel();
            lessonModel.BackSideCollection = new List<BackSideModel>();
            var divMeaning = doc.DocumentNode.SelectNodes("//div").Where(x => x.Attributes.Count > 0 && x.Attributes.Count(y => y.Name == "class" && y.Value.Equals("meanings")) > 0).SingleOrDefault();
            var tableContent = divMeaning.SelectNodes("table").SingleOrDefault();
            var html = tableContent.InnerHtml;
            HtmlNodeCollection rows = tableContent.SelectNodes(".//tr");
            string lessonDescription = string.Empty;
            for (int i = 0; i < rows.Count; ++i)
            {
                HtmlNodeCollection cols = rows[i].SelectNodes(".//td");
                var col1 = cols[0];
                var col2 = cols[1];
                var classNamCol1 = cols[0].Attributes.Where(x => x.Name == "class").Select(x => x.Value).FirstOrDefault();
                var classNamCol2 = cols[1].Attributes.Where(x => x.Name == "class").Select(x => x.Value).FirstOrDefault();

                if ("first-col".Equals(classNamCol1) && string.IsNullOrWhiteSpace(col1.InnerText))
                {
                    if ("second-col word".Equals(classNamCol2))
                    {
                        //get Div word-type
                        //get InnerText

                        //<td class="second-col word">
                        //facilitate
                        //<div class="word-type">v.</div>
                        //<style type="text/css">
                        //<div class="phonetic">media/wordlist/facilitate_v.mp3</div>
                        //</td>
                        var idex = col2.InnerText.Trim().IndexOf(" ");

                        lessonModel.LessonName = col2.InnerText.Trim().Substring(0, idex + 1).Trim(); ;
                        var wordTypeDiv = col2.SelectNodes("//div").Where(x => x.Attributes.Count > 0 && x.Attributes.Count(y => y.Name == "class" && y.Value.Equals("word-type")) > 0).SingleOrDefault();

                        lessonDescription = wordTypeDiv.InnerText.Trim().Replace("&nbsp;&nbsp;", string.Empty);
                    }
                    else if ("second-col pronounce".Equals(classNamCol2))
                    {
                        //<td class="second-col pronounce">
                        //<div style="display: inline; ">/ fəˈsɪlɪteɪt /</div>
                        //<div class="voice"/>
                        //</td>
                        var pronun = col2.InnerText.Trim();
                        //col2.SelectNodes("//div").Where(x => x.Attributes.Count > 0 && x.Attributes.Count(y => string.IsNullOrWhiteSpace(y.Name)) > 0).SingleOrDefault();
                        lessonModel.Description = lessonDescription + " " + pronun;
                    }
                }
                else if ("first-col highlight".Equals(classNamCol1))
                {

                    if ("Vietnamese".Equals(col1.InnerText.Trim()))
                    {
                        //<td class="first-col highlight">Vietnamese</td>
                        //<td class="second-col">làm cho thuận tiện</td>
                        BackSideModel backSideModel = new BackSideModel();
                        backSideModel.Name = "Main Back Side";
                        backSideModel.Content = col2.InnerText;
                        lessonModel.BackSideCollection.Add(backSideModel);
                    }
                    else if ("English".Equals(col1.InnerText.Trim()))
                    {
                        //<td class="first-col highlight">English</td>
                        //<td class="second-col">to make an action or a process possible or easier</td>
                        BackSideModel backSideModel = new BackSideModel();
                        backSideModel.Name = "Description";
                        backSideModel.Content = col2.InnerText;
                        lessonModel.BackSideCollection.Add(backSideModel);
                    }
                    else if ("Example".Equals(col1.InnerText.Trim()))
                    {
                        BackSideModel backSideModel = new BackSideModel();
                        backSideModel.Name = "Example";
                        backSideModel.Content = col2.InnerText;
                        lessonModel.BackSideCollection.Add(backSideModel);
                        //<td class="first-col highlight">Example</td>
                        //<td class="second-col">The new trade agreement facilitates more rapid economic growth</td>
                    }
                }
            }

            //Family 
            var divfamilyword = doc.DocumentNode.SelectNodes("//div").Where(x => x.Attributes.Count > 0 && x.Attributes.Count(y => y.Name == "class" && y.Value.Equals("familyword")) > 0).SingleOrDefault();
            var tablefamilywordContent = divfamilyword.SelectNodes("table").SingleOrDefault();
            var rowsFamily = tablefamilywordContent.SelectNodes(".//tr").SingleOrDefault();
            HtmlNodeCollection colFamily = rowsFamily.SelectNodes(".//td");
            //synonym
            if (!string.IsNullOrWhiteSpace(colFamily[0].InnerText.Trim()))
            {
                BackSideModel backside = new BackSideModel();
                backside.Name = "synonym";
                backside.Content = colFamily[0].InnerText.Trim();
                lessonModel.BackSideCollection.Add(backside);
            }

            //noun(person)
            if (!string.IsNullOrWhiteSpace(colFamily[1].InnerText.Trim()))
            {
                BackSideModel backside = new BackSideModel();
                backside.Name = "noun(person)";
                backside.Content = colFamily[1].InnerText.Trim();
                lessonModel.BackSideCollection.Add(backside);
            }

            //noun(thing)
            if (!string.IsNullOrWhiteSpace(colFamily[2].InnerText.Trim()))
            {
                BackSideModel backside = new BackSideModel();
                backside.Name = "noun(thing)";
                backside.Content = colFamily[2].InnerText.Trim();
                lessonModel.BackSideCollection.Add(backside);
            }

            //verb
            if (!string.IsNullOrWhiteSpace(colFamily[3].InnerText.Trim()))
            {
                BackSideModel backside = new BackSideModel();
                backside.Name = "synonym";
                backside.Content = colFamily[3].InnerText.Trim();
                lessonModel.BackSideCollection.Add(backside);
            }

            //adjective
            if (!string.IsNullOrWhiteSpace(colFamily[4].InnerText.Trim()))
            {
                BackSideModel backside = new BackSideModel();
                backside.Name = "adjective";
                backside.Content = colFamily[4].InnerText.Trim();
                lessonModel.BackSideCollection.Add(backside);
            }

            //adverb
            if (!string.IsNullOrWhiteSpace(colFamily[5].InnerText.Trim()))
            {
                BackSideModel backside = new BackSideModel();
                backside.Name = "adverb";
                backside.Content = colFamily[5].InnerText.Trim();
                lessonModel.BackSideCollection.Add(backside);
            }
            streamResponse.Close();
            sreader.Close();
            myWebResponse.Close();
            return lessonModel;
        }

        static List<string> listCard()
        {
            List<string> list = new List<string>();

            //wordlist-list-container /ul =>div word_id
            //WebRequest myWebRequest;
            //WebResponse myWebResponse;
            string url = "Toiec2.html";
            FileInfo file = new FileInfo(url);
            var ex = file.Exists;
            //myWebRequest = WebRequest.Create(url);
            //myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource
            //Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet
            ////and save it in the stream
            //StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
            //Rstring = sreader.ReadToEnd();//reads it to the end

            TextReader reader = File.OpenText(file.FullName);
            var doc = new HtmlDocument();
            doc.LoadHtml(reader.ReadToEnd());
            var divContainer = doc.DocumentNode.SelectNodes("//ul").SingleOrDefault();
            //.Where(x => x.Attributes.Count > 0 && x.Attributes.Count(y => y.Name == "class" && y.Value.Equals("wordlist-list")) > 0).SingleOrDefault();
            var liTag = divContainer.SelectNodes("//li/div[@class='word_id']");
            foreach (var item in liTag)
            {

                Console.WriteLine(item.InnerText);
                //foreach (var tag in item.Where(x=>x.Attributes.Any(y=>y.Name=="class" && y.Value=="word_id")))
                //{
                //        Console.WriteLine(tag.InnerText);
                //}
            }

            var test = doc.DocumentNode.Attributes.Where(x => x.Name == "class" && x.Value.Equals("wordlist-list-mask")).SingleOrDefault();
            return list;

        }
    }

    class LessonModel
    {
        public int LessonID { get; set; }
        public string LessonName { get; set; }
        public string Description { get; set; }
        public List<BackSideModel> BackSideCollection { get; set; }

    }

    class BackSideModel
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
