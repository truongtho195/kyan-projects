using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using ConvertToText.Database;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using FlashCard.Helper;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ConvertToText
{
    class Program
    {
        static void Main(string[] args)
        {
            //GetSoundForLesson();
            Console.WriteLine("============================GetSoundForLesson Lesson Start ?=============================");
            Console.WriteLine("Press any key to ..");
            Console.ReadLine();
            Console.WriteLine("Starting.....");
            SetMainBackSide();
            Console.ReadLine();
        }

        #region ImportLesson
        private static void InsertLesson()
        {

        }

        private static void SetMainBackSide()
        {
            SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
            List<string> listItemError = new List<string>();
            var AllLesson = flashCardEntity.Lessons.ToList();

            foreach (var item in AllLesson)
            {
                var mainBackSide = item.BackSides.SingleOrDefault(x => x.BackSideName.Trim().Equals("Main Back Side"));
                if (mainBackSide != null)
                {
                    Console.Write("Current Item {0} - {1}", item.LessonName, mainBackSide.BackSideID);
                    mainBackSide.IsMain = 1;
                    try
                    {
                        flashCardEntity.SaveChanges();
                        Console.WriteLine("Done :{0}", mainBackSide.BackSideID);
                    }
                    catch (Exception ex)
                    {
                        listItemError.Add(mainBackSide.BackSideID);
                        Console.WriteLine(" [ Error ] Item Error : {0}", mainBackSide.BackSideID);
                    }
                }

            }


        }

        static List<string> listCard()
        {
            List<string> list = new List<string>();
            string url = "Toiec2.html";
            FileInfo file = new FileInfo(url);
            var ex = file.Exists;
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
            return list;
        }
        #endregion

        private static void GetSoundForLesson()
        {
            Console.WriteLine("============================GetSoundForLesson Lesson Start ?=============================");
            Console.WriteLine("Press any key to ..");
            Console.ReadLine();
            Console.WriteLine("Starting.....");
            try
            {
                int iCount = 0;
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                var AllLesson = flashCardEntity.Lessons.ToList();
                foreach (var item in AllLesson)
                {
                    iCount++;
                    Console.Write("Get Sound For Lesson {0}", item.LessonName);
                    var fileName = item.LessonName;
                    if (item.LessonName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
                    {
                        fileName = CleanFileName(item.LessonName);
                        Console.WriteLine("      InvalidFileNameChars");
                    }
                    GetSoundFromGoogleTranslate.GetSoundGoogle(fileName, "FlashCardSound");
                    Console.WriteLine("==> Done");
                }
                Console.WriteLine("|| Generation successful {0}/{1}!", iCount, AllLesson.Count);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n + !!!  Exception : \n {0}", ex.ToString());
            }

        }
        private static string CleanFileName(string fileName)
        {
            try
            {
                return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty)).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return fileName;

            }

        }
        private static void InserLesson()
        {
            //Console.WriteLine("============================Convert Lesson Start ?=============================");
            //Console.WriteLine("Press enter key to ..");
            //Console.ReadLine();
            //Console.WriteLine("Starting.....");

            //try
            //{
            //    SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
            //    LessonDataAccess lessonDA = new LessonDataAccess();
            //    var allLesson= lessonDA.GetAll();

            //    foreach (var item in allLesson)
            //    {

            //        Console.WriteLine("-  Lesson item : {0}", item.LessonID);
            //        TextRange textRange = new TextRange(item.Description.ContentStart, item.Description.ContentEnd);
            //        flashCardEntity.Lessons.AddObject(new Lesson() {LessonID=item.LessonID.ToString(), LessonName=item.LessonName,Description=textRange.Text,CategoryID=item.TypeID.ToString(),CardID=item.CategoryID.ToString()});
            //        flashCardEntity.SaveChanges();
            //        Console.WriteLine("=====> Done");
            //    }
            //    Console.WriteLine("Finished.....");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("\n + !!!  Exception : \n {0}", ex.ToString());
            //}
            //Console.ReadLine();
        }

        private static void UpdateLessonNrelation()
        {
            Console.WriteLine("============================Update Lesson Start ?=============================");
            Console.WriteLine("Press any key to ..");
            Console.ReadLine();
            Console.WriteLine("Starting.....");
            try
            {
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                var AllLesson = flashCardEntity.Lessons.ToList();
                foreach (var item in AllLesson)
                {
                    Console.WriteLine("-  Lesson item convert : {0}", item.LessonID);
                    item.Description = item.Description.Trim();
                    item.LessonName = item.LessonName.Trim();
                    foreach (var backSide in item.BackSides)
                    {
                        Console.WriteLine("- - Back side item update :{0}", backSide.BackSideID);
                        backSide.BackSideName = backSide.BackSideName.Trim();
                        backSide.Content = backSide.Content.Trim();
                    }
                    flashCardEntity.SaveChanges();
                    Console.WriteLine("=====> Done");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n + !!!  Exception : \n {0}", ex.ToString());
            }
            Console.ReadLine();
        }

        private static void ConvertLesson()
        {
            Console.WriteLine("============================Convert Lesson Start ?=============================");
            Console.WriteLine("Press any key to ..");
            Console.ReadLine();
            Console.WriteLine("Starting.....");
            try
            {
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                var AllLesson = flashCardEntity.Lessons.ToList();
                foreach (var item in AllLesson)
                {
                    Console.WriteLine("-  Lesson item convert : {0}", item.LessonID);
                    FlowDocument descriptionDocument = FlowDocumentConverter.ConvertXMLToFlowDocument(item.Description);
                    TextRange textRange = new TextRange(descriptionDocument.ContentStart, descriptionDocument.ContentEnd);
                    item.Description = textRange.Text;
                    flashCardEntity.SaveChanges();
                    //foreach (var backSide in item.BackSides)
                    //{
                    //    Console.WriteLine("      Back Side item convert : {0}", backSide.BackSideID);
                    //    FlowDocument backSideDocument = FlowDocumentConverter.ConvertXMLToFlowDocument(backSide.Content);
                    //    TextRange backSideTextRange = new TextRange(backSideDocument.ContentStart, backSideDocument.ContentEnd);
                    //    backSide.Content = backSideTextRange.Text;
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n + !!!  Exception : \n {0}", ex.ToString());
            }
            Console.ReadLine();
        }


        private void ReadMSWord()
        {
            //Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
            //object miss = System.Reflection.Missing.Value;
            //object path = @"E:\3000.doc";
            //object readOnly = true;
            //Microsoft.Office.Interop.Word.Document docs = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);
            //string totaltext = "";
            //for (int i = 0; i < docs.Paragraphs.Count; i++)
            //{
            //    totaltext += " \r\n " + docs.Paragraphs[i + 1].Range.Text.ToString();
            //}
            //Console.WriteLine(totaltext);
            //docs.Close();
            //word.Quit();
        }
    }


    static class FlowDocumentConverter
    {
        public static string ConvertFlowDocumentToSUBStringFormat(object flowDocument)
        {
            //take the flow document and change all of its images into a base64 string
            //apply the XamlWriter to the newly transformed flowdocument
            using (StringWriter stringwriter = new StringWriter())
            {
                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stringwriter))
                {
                    XamlWriter.Save(flowDocument, writer);
                }
                return stringwriter.ToString();
            }
        }

        public static FlowDocument ConvertXMLToFlowDocument(string XmlContent)
        {
            try
            {
                var stringReader = new StringReader(XmlContent);
                var xmlTextReader = new XmlTextReader(stringReader);
                return (FlowDocument)XamlReader.Load(xmlTextReader);
            }
            catch (Exception ex)
            {
                throw ex;
            }



            //var flowDocument = new FlowDocument();
            //if (XmlContent != null)
            //{
            //    var stringReader = new StringReader(XmlContent);
            //    var xmlTextReader = new XmlTextReader(stringReader);
            //    var doc = new FlowDocument();
            //    Section sec = XamlReader.Load(xmlTextReader) as Section;
            //    while (sec.Blocks.Count > 0)
            //    {
            //        var block = sec.Blocks.FirstBlock;
            //        sec.Blocks.Remove(block);
            //        doc.Blocks.Add(block);
            //    }
            //    return flowDocument;
            //}
            //return null;
        }
    }
}
