using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _GlobalizeCode : System.Web.UI.Page
{
    public const string Culture = "pt-BR";
    public const string Path = "C:\\YoursoftBase2020\\SuasVendas\\SuasVendasApp\\Web";

    protected void Page_Load(object sender, EventArgs e)
    {

        DirectoryInfo di = new DirectoryInfo(Path);

        FileSystemInfo[] fsi = di.GetFileSystemInfos();

        EncontraArquivos(fsi);
    }

    private void EncontraArquivos(FileSystemInfo[] f)
    {
        if (f != null)
        {
            foreach (FileSystemInfo obj in f)
            {
                if (obj is DirectoryInfo)
                {
                    DirectoryInfo dinfo = (DirectoryInfo)obj;
                    EncontraArquivos(dinfo.GetFileSystemInfos());
                }
                else if (obj is FileInfo)
                {

                    try
                    {
                        Thread.Sleep(10);
                        string[] lines = System.IO.File.ReadAllLines(obj.FullName);

                        int nlinha = 0;
                        foreach (var line in lines)
                        {
                            nlinha++;

                            string patternForce = "<ccc>.*?<ccc>";
                            Regex regexForce = new Regex(patternForce, RegexOptions.None);

                            MatchCollection matches = regexForce.Matches(line);


                            foreach (Match match in matches)
                            {
                                if (match.Value.Contains("[$") ) continue;
                                if (match.Value.Length == 0) continue;


                                string texto = match.Value.Replace("<ccc>", "");
                                texto = texto.Replace("<ccc>", "");

                                if (texto.Length == 0) continue;

                                var Chave = $"[${Normalize(texto)}$]";
                               
                                try
                                {

                                    var linhas = File.ReadAllLines(obj.FullName);

                                    linhas[nlinha - 1] = linhas[nlinha - 1].Replace(match.Value, Chave);
                                    Response.Write(texto + " <br>");

                                    File.WriteAllLines(obj.FullName, linhas, UnicodeEncoding.GetEncoding(65001));

                                    YS.Dados.PostgreSQL.Data data = new YS.Dados.PostgreSQL.Data();

                                    data.Sql = @"INSERT INTO yoursales.globalizacao (
                                                    glob_cultura, 
                                                    glob_texto, 
                                                    glob_chave
                                                    ) VALUES (
                                                    @glob_cultura, 
                                                    @glob_texto, 
                                                    @glob_chave)";
                                    data.AddParameter("@glob_cultura", Culture);
                                    data.AddParameter("@glob_texto", texto);
                                    data.AddParameter("@glob_chave", Chave);
                                    data.ExecuteQuery();
                                    Response.Write("Success <br>");

                                }
                                catch(Exception er)
                                {
                                    Response.Write("Error <br>");
                                }
                            }

                        }
                    }
                    catch (Exception er)
                    {

                    }
                }
            }
        }
    }
        
    public string Normalize(string text)
    {
        StringBuilder sbReturn = new StringBuilder();
        var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
        foreach (char letter in arrayText)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                sbReturn.Append(letter);
        }

        sbReturn = sbReturn.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        return sbReturn.ToString();
    }
}