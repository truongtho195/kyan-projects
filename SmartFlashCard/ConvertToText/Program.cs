using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlashCard.DataAccess;
using System.Windows.Documents;
using ConvertToText.Database;
using System.IO;
using System.Windows.Markup;
using System.Xml;

namespace ConvertToText
{
    class Program
    {
        static void Main(string[] args)
        {
            UpdateLessonNrelation();
        }
        
        private static void InserLesson()
        {
            Console.WriteLine("============================Convert Lesson Start ?=============================");
            Console.WriteLine("Press enter key to ..");
            Console.ReadLine();
            Console.WriteLine("Starting.....");

            try
            {
                SmartFlashCardDBEntities flashCardEntity = new SmartFlashCardDBEntities();
                LessonDataAccess lessonDA = new LessonDataAccess();
                var allLesson= lessonDA.GetAll();
               
                foreach (var item in allLesson)
                {
                    
                    Console.WriteLine("-  Lesson item : {0}", item.LessonID);
                    TextRange textRange = new TextRange(item.Description.ContentStart, item.Description.ContentEnd);
                    flashCardEntity.Lessons.AddObject(new Lesson() {LessonID=item.LessonID.ToString(), LessonName=item.LessonName,Description=textRange.Text,CategoryID=item.TypeID.ToString(),CardID=item.CategoryID.ToString()});
                    flashCardEntity.SaveChanges();
                    Console.WriteLine("=====> Done");
                }
                Console.WriteLine("Finished.....");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n + !!!  Exception : \n {0}", ex.ToString());
            }
            Console.ReadLine();
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
                        Console.WriteLine("- - Back side item update :{0}",backSide.BackSideID);
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
