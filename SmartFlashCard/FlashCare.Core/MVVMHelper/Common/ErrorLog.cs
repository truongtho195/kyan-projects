using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class ErrorLog
{
    private string _errorTime;
    private string _logFormat;

    public ErrorLog()
    {
        // _logFormat used to create log files format :
        // MM/dd/yyyy hh:mm:ss AM/PM ==> Log Message
        _logFormat = String.Format("{0:d} {1:t} ==>", DateTime.Now);

        //this variable used to create log filename format "
        //for example filename : ErrorLogYYYYMMDD
        _errorTime = DateTime.Now.ToString("yyyyMMdd");
    }

    public void WriteLog(string pathName, string errorMsg)
    {
        StreamWriter sw = new StreamWriter(pathName + _errorTime, true);
        sw.WriteLine(_logFormat + errorMsg);
        sw.Flush();
        sw.Close();
    }
}