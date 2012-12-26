using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PreFlashCardPhone.Database;

namespace PreFlashCardPhone
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter To Continous:");
            Console.ReadLine();
            UpDateBackSideType();
            Console.ReadLine();
        }

        static void UpDateBackSideType()
        {
            WPFlashCardDBEntities entities = new WPFlashCardDBEntities();

            var AllBackSides = entities.BackSides.ToList();//Part Of Speech
            List<string> itemError = new List<string>();
            int i = 0;
            foreach (var backside in AllBackSides)
            {
                //int refout;
                //if (!int.TryParse(backside.BackSideTypeID,out refout))
                //{
                //    try
                //    {
                //        i++;
                //        backside.BackSideTypeID = "12";
                //        entities.SaveChanges();
                //        Console.WriteLine("Current item idx : [{0}] -{1}", i, backside.BackSideTypeID);
                //    }
                //    catch (Exception)
                //    {
                //        int idex = AllBackSides.IndexOf(backside);
                //        itemError.Add(string.Format("Item Index {0}/{1} ", idex, backside.BackSideID));
                //    }
                //}

               
              
            }
            Console.WriteLine("Done: {0}/{1}", i, AllBackSides.Count);

            if (itemError.Count > 0)
            {
                Console.WriteLine("================================================");
                Console.WriteLine("Item Error");
                foreach (var item in itemError)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("================================================");

            }
        }

        //static void SetBackSideName(BackSide backSide)
        //{
        //    switch (backSide.BackSideName)
        //    {
        //        case "Main Back Side":
        //            backSide.BackSideName = "1";
        //            break;
        //        case "English mean":
        //        case "Description":
        //        case "Desciption":

        //        case "English Mean":
        //        case "English":
        //        case "English Name":
        //            backSide.BackSideName = "2";
        //            break;
        //        case "Example :":
        //        case "Example":
        //            backSide.BackSideName = "3";
        //            break;
        //        //case "Other":
        //        //    backSide.BackSideName = "1";
        //        //    break;
        //        case "IDIOMS":
        //            backSide.BackSideName = "4";
        //            break;
        //        case "synonym":
        //        case "synonym:":
        //        case "synonym :":

        //            backSide.BackSideName = "5";
        //            break;
        //        case "noun(Thing)":
        //        case "noun(sth)":
        //        case "noun(thing)":
        //            backSide.BackSideName = "6";
        //            break;
        //        case "noun(person)":
        //        case "noun(s.o)":
        //            backSide.BackSideName = "7";
        //            break;
        //        case "adjective":
        //            backSide.BackSideName = "8";
        //            break;
        //        case "adverb":
        //            backSide.BackSideName = "9";
        //            break;
        //        case "verb":
        //            backSide.BackSideName = "10";
        //            break;
        //        case "Part of Speech":
        //        case "part of speech":
        //        case "Part of speech":
        //        case "parth of speech":
        //        case "Part Of Speech":


        //        case "Other":
        //        case "Other:":

        //            backSide.BackSideName = "12";
        //            break;

        //    }
        //}

    }
}
