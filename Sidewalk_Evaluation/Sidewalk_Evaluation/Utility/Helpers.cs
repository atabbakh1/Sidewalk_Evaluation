using System.Collections.Generic;
using System.Linq;


namespace Sidewalk_Evaluation.Utility
{
    class Helpers
    {
        /// <summary>
        /// Return the data of a CSV file without the header line
        /// </summary>
        /// <param name="filePath">the path of the CSV file</param>
        /// <returns></returns>
        public static string[] RetreiveCSVData(string filePath)
        {
            string[] csvDataLines = System.IO.File.ReadAllLines(filePath);
            //skip the header line
            return csvDataLines.Skip(0).ToArray();
        }

        /// <summary>
        /// Return the data of a CSV file filtered based on a desired value for one of the columns
        /// </summary>
        /// <param name="filePath">the path of the CSV file</param>
        /// <param name="column">index column to search match value against</param>
        /// <param name="match">value to use for filtering records</param>
        /// <returns></returns>
        public static string[] RetreiveCSVData(string filePath, int column, string match)
        {
            string[] csvDataLines = System.IO.File.ReadAllLines(filePath);

            List<string> filteredLines = new List<string>();
            for(int i=0; i < csvDataLines.Length; i++)
            {
                string[] dataLine = csvDataLines[i].Split(',');
                if(dataLine[column] == match)
                {
                    filteredLines.Add(csvDataLines[i]);
                }
            }
            return filteredLines.ToArray();
        }

    }
}
