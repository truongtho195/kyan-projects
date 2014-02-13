using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Npgsql;
using System.Reflection;
using CPC.POS.Model;
using ICSharpCode.SharpZipLib.Zip;
using System.Windows;

namespace CPC.POS.ViewModel.Synchronization
{
    public class ExportFileHelper
    {
        #region Defines
        private static StreamWriter _tempFile;
        private static NpgsqlConnection _pgConnection;
        private static NpgsqlCommand _pgCommand;
        private static NpgsqlDataReader _pgDataReader;
        private static List<base_TempModel> _changedRecordList;
        private static string _directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        private static string _databaseName = "posServer";
        private static string _fullPath;
        private static int _subFileIndex;
        public static List<string> SyncFilePathCollection = new List<string>();
        private static FileInfo _tempFileInfo;
        private static int _maxSizeFile = 1048576;
        #endregion

        /// <summary>
        /// Generate a file.
        /// </summary>
        /// <param name="connection"></param>
        public static void ExportFile(string connection)
        {
            try
            {
                // Initial connection to database
                _pgConnection = new NpgsqlConnection(connection);
                // Open connection
                _pgConnection.Open();
                ClearData();
                GetChangedData();
                CreateTempFile();
                CreateScript();
                CreateSynchronousFile(_subFileIndex > 1);
                _pgConnection.Close();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Get all changed records from temp table
        /// </summary>
        private static void GetChangedData()
        {
            // Create command string
            string queryCommand = "SELECT * FROM \"base_Temp\" WHERE \"IsSynchronous\"=false";

            // Create commmand execute
            using (_pgCommand = new NpgsqlCommand(queryCommand, _pgConnection))
            {
                // Get fetch data to DataReader
                using (_pgDataReader = _pgCommand.ExecuteReader())
                {
                    if (_pgDataReader.HasRows)
                    {
                        // Initial changed record list
                        _changedRecordList = new List<base_TempModel>();

                        while (_pgDataReader.Read())
                        {
                            base_TempModel tempModel = new base_TempModel();
                            tempModel.Resource = _pgDataReader.GetString((int)SynchronizationColumn.Resource);
                            tempModel.TableName = _pgDataReader.GetString((int)SynchronizationColumn.TableName);
                            tempModel.Status = _pgDataReader.GetInt16((int)SynchronizationColumn.Status);
                            _changedRecordList.Add(tempModel);
                        }
                    }

                    _pgDataReader.Close();
                }
            }
        }

        /// <summary>
        /// Create synchronous file in bin directory
        /// </summary>
        private static void CreateTempFile()
        {
            string fileName = string.Format("{0}_{1}_temp.txt", _databaseName, DateTime.Now.ToString("yyMMddHHmmssfff"));
            _fullPath = Path.Combine(_directoryName, fileName);
            _tempFileInfo = new FileInfo(_fullPath);
            _tempFile = new StreamWriter(_fullPath);
        }

        /// <summary>
        /// Create synchronous file in bin directory
        /// </summary>
        private static void CreateSynchronousFile(bool isSplitFile)
        {
            _tempFile.Flush();
            _tempFile.Close();
            string splitFilePath = _fullPath.Replace("temp", _subFileIndex++.ToString("00#"));
            if (!isSplitFile)
                splitFilePath = _fullPath.Replace("_temp", "");
            // Rename temp file
            File.Move(_fullPath, splitFilePath);
            ///Compress file to .zip.
            Compress(new FileInfo(splitFilePath));
        }

        /// <summary>
        /// Create SQL script to synchronous database
        /// </summary>
        private static void CreateScript()
        {
            foreach (IGrouping<string, base_TempModel> group in _changedRecordList.GroupBy(x => x.TableName))
            {
                // Get column name list
                List<string> columnNameList = new List<string>(GetColumnName(group.Key));

                foreach (base_TempModel tempItem in group)
                {
                    switch ((SynchronizationStatus)tempItem.Status)
                    {
                        case SynchronizationStatus.Insert:
                            CreateInsertScript(tempItem, columnNameList);
                            break;
                        case SynchronizationStatus.Update:
                            CreateUpdateScript(tempItem, columnNameList);
                            break;
                        case SynchronizationStatus.Delete:
                            CreateDeleteScript(tempItem);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Create insert script
        /// </summary>
        /// <param name="tempItem"></param>
        private static void CreateInsertScript(base_TempModel tempItem, List<string> columnNameList)
        {
            // Create command string
            string queryCommand = string.Format("SELECT * FROM \"{0}\" WHERE \"Resource\" = '{1}'", tempItem.TableName, tempItem.Resource);

            // Create commmand execute
            using (_pgCommand = new NpgsqlCommand(queryCommand, _pgConnection))
            {
                // Get fetch data to DataReader
                using (_pgDataReader = _pgCommand.ExecuteReader())
                {
                    if (_pgDataReader.HasRows)
                    {
                        while (_pgDataReader.Read())
                        {
                            string insertCommand = string.Format("{0}|INSERT INTO \"{1}\" VALUES (", tempItem.Resource, tempItem.TableName);
                            for (int i = 0; i < _pgDataReader.FieldCount; i++)
                            {
                                string value = string.Empty;
                                if (columnNameList.ElementAt(i).ToLower().Equals("id"))
                                {
                                    // Insert default value of id column
                                    value = "DEFAULT";
                                }
                                else if (_pgDataReader.IsDBNull(i))
                                {
                                    // Insert null value
                                    value = "NULL";
                                }
                                else
                                {
                                    // Insert synchronous value
                                    value = string.Format("'{0}'", _pgDataReader.GetValue(i));
                                }
                                insertCommand += value;
                                if (i < _pgDataReader.FieldCount - 1)
                                    insertCommand += ", ";
                            }
                            insertCommand += ");";
                            WriteText(insertCommand);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create update script
        /// </summary>
        /// <param name="tempItem"></param>
        private static void CreateUpdateScript(base_TempModel tempItem, List<string> columnNameList)
        {
            // Create command string
            string queryCommand = string.Format("SELECT * FROM \"{0}\" WHERE \"Resource\" = '{1}'", tempItem.TableName, tempItem.Resource);

            // Create commmand execute
            using (_pgCommand = new NpgsqlCommand(queryCommand, _pgConnection))
            {
                // Get fetch data to DataReader
                using (_pgDataReader = _pgCommand.ExecuteReader())
                {
                    if (_pgDataReader.HasRows)
                    {
                        while (_pgDataReader.Read())
                        {
                            string updateCommand = string.Format("{0}|UPDATE \"{1}\" SET ", tempItem.Resource, tempItem.TableName);
                            for (int i = 0; i < _pgDataReader.FieldCount; i++)
                            {
                                string columnName = columnNameList.ElementAt(i);
                                if (columnName.ToLower().Equals("id"))
                                    continue;
                                object value = string.Format("'{0}'", _pgDataReader.GetValue(i));
                                if (_pgDataReader.IsDBNull(i))
                                    value = "NULL";
                                updateCommand += string.Format("\"{0}\" = {1}", columnName, value);
                                if (i < _pgDataReader.FieldCount - 1)
                                    updateCommand += ", ";
                            }
                            updateCommand += string.Format(" WHERE \"Resource\" = '{0}';", tempItem.Resource);
                            WriteText(updateCommand);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create delete script
        /// </summary>
        /// <param name="tempItem"></param>
        private static void CreateDeleteScript(base_TempModel tempItem)
        {
            // Create delete command string
            string deleteCommand = string.Format("{0}|DELETE FROM \"{1}\" WHERE \"Resource\" = '{0}';", tempItem.Resource, tempItem.TableName);
            WriteText(deleteCommand);
        }

        private static void UpdateSynchronizedData()
        {
            // Create update string
            string updateCommand = string.Format("UPDATE \"base_Temp\" SET \"IsSynchronous\"=true, \"SynchronizationDate\"=now()");

            // Create commmand execute
            using (_pgCommand = new NpgsqlCommand(updateCommand, _pgConnection))
            {
                // Execute update command
                _pgCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Get column name list
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static List<string> GetColumnName(string tableName)
        {
            List<string> columnNameList = null;

            // Create command string
            string queryCommand = string.Format("SELECT column_name FROM information_schema.columns WHERE table_name = '{0}'", tableName);

            // Create commmand execute
            using (_pgCommand = new NpgsqlCommand(queryCommand, _pgConnection))
            {
                // Get fetch data to DataReader
                using (_pgDataReader = _pgCommand.ExecuteReader())
                {
                    if (_pgDataReader.HasRows)
                    {
                        columnNameList = new List<string>();
                        while (_pgDataReader.Read())
                        {
                            columnNameList.Add(_pgDataReader.GetString(0));
                        }
                    }
                }
            }

            return columnNameList;
        }

        /// <summary>
        /// Write text
        /// </summary>
        /// <param name="text"></param>
        private static void WriteText(string text)
        {
            if (_tempFileInfo.Length + text.Length > _maxSizeFile)
            {
                // Rename temp file to synchronous file
                CreateSynchronousFile(true);

                // Creat new other temp file
                CreateTempFile();
            }

            // Write text to temp file
            _tempFile.WriteLine(text);
        }

        private static void ClearData()
        {
            _subFileIndex = 1;
            _changedRecordList = new List<base_TempModel>();
            _directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            _fullPath = string.Empty;
        }

        public static void Compress(FileInfo file)
        {
            try
            {
                //Lấy về tên file ở đây tên file lấy về sẽ là: Zipfile
                string fileName = file.Name;
                //Tên file zip được tạo ra ở đây tên sẽ là: C://FileToZip//Zipfile//Zipfile.zip
                string zipFile = file.Directory + "\\" + fileName + ".zip";
                ZipOutputStream zipOut = new ZipOutputStream(File.Create(zipFile));
                //Lấy về thông tin file có trong folder FileToZip
                ZipEntry entry = new ZipEntry(file.Name);
                FileStream fileStream = file.OpenRead();
                byte[] buffer = new byte[Convert.ToInt32(fileStream.Length)];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                entry.DateTime = file.LastWriteTime;
                entry.Size = fileStream.Length;
                fileStream.Close();
                zipOut.PutNextEntry(entry);
                zipOut.Write(buffer, 0, buffer.Length);
                // Xoá file sau khi được nén
                //File.Delete(file.DirectoryName);
                zipOut.Finish();
                zipOut.Close();
                SyncFilePathCollection.Add(zipFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static string DeCompress(string fileIn, string folderOut)
        {
            ZipInputStream zipIn = new ZipInputStream(File.OpenRead(fileIn));
            ZipEntry entry;
            while ((entry = zipIn.GetNextEntry()) != null)
            {
                FileStream streamWriter = File.Create(folderOut + "\\" + entry.Name);
                long size = entry.Size;
                byte[] data = new byte[size];
                while (true)
                {
                    size = zipIn.Read(data, 0, data.Length);
                    if (size > 0) streamWriter.Write(data, 0, (int)size);
                    else break;
                }
                streamWriter.Close();
            }
            return folderOut;
        }
    }
}
