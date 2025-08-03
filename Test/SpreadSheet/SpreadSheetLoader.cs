using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Test.SpreadSheet
{
    public class SpreadSheetLoader
    {
        public static List<List<string>> LoadSheetData(string sheetId, string sheetGid)
        {
            string sheetCsvUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={sheetGid}";
            List<List<string>> result = new List<List<string>>();
            try
            {
                using var client = new HttpClient();
                var csvData = client.GetStringAsync(sheetCsvUrl).Result;
                var rows = csvData.Split('\n');
                foreach (var row in rows)
                {
                    var cols = row.Split(',');
                    result.Add(cols.Select(item=>item.Replace("\r",string.Empty)).ToList());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }
    }
}
