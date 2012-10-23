using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using HtmlAgilityPack;
using FlashCard.Database.Repository;
using FlashCard.Database;
using System.Net;
using FlashCard.Helper;

namespace FlashCard.Views
{
    /// <summary>
    /// Interaction logic for ImportFromSite.xaml
    /// </summary>
    public partial class ImportFromSite : Window
    {
        public ImportFromSite()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            InsertLesson();
        }

        private void InsertLesson()
        {
            try
            {
                CardRepository cardRepository = new CardRepository();
                Card card = new Card();
                card.CardID = AutoGeneration.NewSeqGuid().ToString();
                card.CardName = "Toeic A6";
                card.Remark = "Toeic A6 Blue Up";
                foreach (var item in listCard())
                {
                    var model = GetContent(item);
                    card.Lessons.Add(model);
                    GetSoundFromGoogleTranslate.GetSoundGoogle(model.LessonName, "FlashCardSound");
                    Console.WriteLine("Add Lesson \"{0}\" to Card :\"{1}\"", model.LessonName, card.CardName);
                }
                cardRepository.Add(card);
                cardRepository.Commit();
                MessageBox.Show("done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private List<string> listCard()
        {
            List<string> list = null;
            string url = "Toeic6.html";
            FileInfo file = new FileInfo(url);
            if (file.Exists)
            {
                list = new List<string>();
                TextReader reader = File.OpenText(file.FullName);
                var doc = new HtmlDocument();
                doc.LoadHtml(reader.ReadToEnd());
                var divContainer = doc.DocumentNode.SelectNodes("//ul").SingleOrDefault();
                var liTag = divContainer.SelectNodes("//li/div[@class='word_id']");
                foreach (var item in liTag)
                {
                    list.Add(item.InnerText);
                    Console.WriteLine(item.InnerText);
                }
            }
            return list;
        }


        static Lesson GetContent(string id)
        {
            string Rstring = string.Empty;
            WebRequest myWebRequest;
            WebResponse myWebResponse;
            String URL = "http://www.blueup.vn/index2.php?option=com_word&task=getWordById&word_id=" + id;
            myWebRequest = WebRequest.Create(URL);
            myWebResponse = myWebRequest.GetResponse();//Returns a response from an Internet resource
            Stream streamResponse = myWebResponse.GetResponseStream();//return the data stream from the internet
            //and save it in the stream
            StreamReader sreader = new StreamReader(streamResponse);//reads the data stream
            Rstring = sreader.ReadToEnd();//reads it to the end
            var doc = new HtmlDocument();
            doc.LoadHtml(Rstring);

            //Create new LessonModel
            var lessonModel = new Lesson();
            lessonModel.LessonID = AutoGeneration.NewSeqGuid().ToString();
            lessonModel.CategoryID = "1";
            lessonModel.BackSides = new System.Data.Objects.DataClasses.EntityCollection<BackSide>();
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
                        string lessonText = col2.InnerText.Trim().Replace("&nbsp;&nbsp;", " ");
                        var idex = lessonText.IndexOf(" ");

                        lessonModel.LessonName = lessonText.Trim().Substring(0, idex + 1).Trim();
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
                        BackSide backSideModel = new BackSide();
                        backSideModel.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                        backSideModel.BackSideName = "Main Back Side";
                        backSideModel.Content = col2.InnerText;
                        lessonModel.BackSides.Add(backSideModel);
                    }
                    else if ("English".Equals(col1.InnerText.Trim()))
                    {
                        //<td class="first-col highlight">English</td>
                        //<td class="second-col">to make an action or a process possible or easier</td>
                        BackSide backSideModel = new BackSide();
                        backSideModel.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                        backSideModel.BackSideName = "Description";
                        backSideModel.Content = col2.InnerText;
                        lessonModel.BackSides.Add(backSideModel);
                    }
                    else if ("Example".Equals(col1.InnerText.Trim()))
                    {
                        BackSide backSideModel = new BackSide();
                        backSideModel.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                        backSideModel.BackSideName = "Example";
                        backSideModel.Content = col2.InnerText;
                        lessonModel.BackSides.Add(backSideModel);

                        //<td class="first-col highlight">Example</td>
                        //<td class="second-col">The new trade agreement facilitates more rapid economic growth</td>
                    }
                }
            }


            //Family Word
            //Family 
            var divfamilyword = doc.DocumentNode.SelectNodes("//div").Where(x => x.Attributes.Count > 0 && x.Attributes.Count(y => y.Name == "class" && y.Value.Equals("familyword")) > 0).SingleOrDefault();
            var tablefamilywordContent = divfamilyword.SelectNodes("table").SingleOrDefault();
            var rowsFamily = tablefamilywordContent.SelectNodes(".//tr").SingleOrDefault();
            HtmlNodeCollection colFamily = rowsFamily.SelectNodes(".//td");
            //synonym
            if (!string.IsNullOrWhiteSpace(colFamily[0].InnerText.Trim()))
            {
                BackSide backside = new BackSide();
                backside.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                backside.BackSideName = "synonym";
                backside.Content = colFamily[0].InnerText.Trim();
                lessonModel.BackSides.Add(backside);
            }

            //noun(person)
            if (!string.IsNullOrWhiteSpace(colFamily[1].InnerText.Trim()))
            {
                BackSide backside = new BackSide();
                backside.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                backside.BackSideName = "noun(person)";
                backside.Content = colFamily[1].InnerText.Trim();
                lessonModel.BackSides.Add(backside);
            }

            //noun(thing)
            if (!string.IsNullOrWhiteSpace(colFamily[2].InnerText.Trim()))
            {
                BackSide backside = new BackSide();
                backside.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                backside.BackSideName = "noun(thing)";
                backside.Content = colFamily[2].InnerText.Trim();
                lessonModel.BackSides.Add(backside);
            }

            //verb
            if (!string.IsNullOrWhiteSpace(colFamily[3].InnerText.Trim()))
            {
                BackSide backside = new BackSide();
                backside.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                backside.BackSideName = "synonym";
                backside.Content = colFamily[3].InnerText.Trim();
                lessonModel.BackSides.Add(backside);
            }

            //adjective
            if (!string.IsNullOrWhiteSpace(colFamily[4].InnerText.Trim()))
            {
                BackSide backside = new BackSide();
                backside.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                backside.BackSideName = "adjective";
                backside.Content = colFamily[4].InnerText.Trim();
                lessonModel.BackSides.Add(backside);
            }

            //adverb
            if (!string.IsNullOrWhiteSpace(colFamily[5].InnerText.Trim()))
            {
                BackSide backside = new BackSide();
                backside.BackSideID = AutoGeneration.NewSeqGuid().ToString();
                backside.BackSideName = "adverb";
                backside.Content = colFamily[5].InnerText.Trim();
                lessonModel.BackSides.Add(backside);
            }
            streamResponse.Close();
            sreader.Close();
            myWebResponse.Close();

            return lessonModel;
        }
    }
}
