using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace RWFileSupporter
{
    public class RWFileSupporter : IDisposable
    {
        private static bool flagOnlyDefine = true;

        private bool allowInit;

        public RWFileSupporter(){
            allowInit = flagOnlyDefine;
            if (flagOnlyDefine)
            {
                flagOnlyDefine = false;
            }
        }

        ~RWFileSupporter() {
            flagOnlyDefine = true;
        }

        public void Write<T>(string path, IEnumerable<T> enumerables )
        {
            if (allowInit)
            {
                // Write sample data to CSV file
                using (CsvFileWriter writer = new CsvFileWriter(path))
                {
                    PropertyInfo[] fields = typeof(T).GetProperties();

                    foreach (var item in enumerables)
                    {
                        CsvRow row = new CsvRow();

                        foreach (PropertyInfo p in fields)
                        {
                            if (p.PropertyType == typeof(string))
                            {
                                row.Add((string)p.GetValue(item));
                            }
                        }
                        writer.WriteRow(row);
                    }
                }
            }
            else
            {
                throw new Exception("Has a process is reading|writing this file");
            }
        }

        public List<T> Read<T>(string path) where T : new()
        {
            if (allowInit)
            {
                List<T> listModules = new List<T>();

                // Read sample data from CSV file
                using (CsvFileReader reader = new CsvFileReader(path))
                {
                    CsvRow row = new CsvRow();

                    while (reader.ReadRow(row))
                    {
                        T data = new T();
                        PropertyInfo[] props = typeof(T).GetProperties();
                        if (row.Count <= props.Length)
                        {
                            int j = 0;
                            for (int i = 0; i < props.Length; i++)
                            {
                                if (props[i].PropertyType == typeof(string))
                                {
                                    props[i].SetValue(data, row[j]);
                                    j++;
                                }
                            }
                        }
                        else
                        {

                        }
                        listModules.Add(data);
                    }
                }

                return listModules;
            }
            else
            {
                throw new Exception("Has a process is reading|writing this file");
            }
        }

        public void Dispose()
        {
            flagOnlyDefine = true;
        }
    }

    /// <summary>
    /// Class to store one CSV row
    /// </summary>
    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvFileWriter(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="row">The row to be written</param>
        public void WriteRow(CsvRow row)
        {
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            foreach (string value in row)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Add separator if this isn't the first value
                    if (!firstColumn)
                        builder.Append(',');
                    // Implement special handling for values that contain comma or quote
                    // Enclose in quotes and double up any double quotes
                    if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                        builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                    else
                        builder.Append(value);
                    firstColumn = false;
                }
            }
            row.LineText = builder.ToString();
            WriteLine(row.LineText);
        }
    }

    /// <summary>
    /// Class to read data from a CSV file
    /// </summary>
    public class CsvFileReader : StreamReader
    {
        public CsvFileReader(Stream stream)
            : base(stream)
        {
        }

        public CsvFileReader(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool ReadRow(CsvRow row)
        {
            row.LineText = ReadLine();
            if (String.IsNullOrEmpty(row.LineText))
                return false;

            int pos = 0;
            int rows = 0;

            while (pos < row.LineText.Length)
            {
                string value;

                // Special handling for quoted field
                if (row.LineText[pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < row.LineText.Length)
                    {
                        // Test for quote character
                        if (row.LineText[pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = row.LineText.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < row.LineText.Length && row.LineText[pos] != ',')
                        pos++;
                    value = row.LineText.Substring(start, pos - start);
                }

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value;
                else
                    row.Add(value);
                rows++;

                // Eat up to and including next comma
                while (pos < row.LineText.Length && row.LineText[pos] != ',')
                    pos++;
                if (pos < row.LineText.Length)
                    pos++;
            }
            // Delete any unused items
            while (row.Count > rows)
                row.RemoveAt(rows);

            // Return true if any columns read
            return (row.Count > 0);
        }
    }
}
