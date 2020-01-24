using System;
using System.Data;
using System.Globalization;
using System.Text;

public partial class _GlobalizeDataBase : System.Web.UI.Page
{

    public const string Culture = "pt-BR";

    protected void Page_Load(object sender, EventArgs e)
    {
        ReplaceDB("form", "form_titulo", "form_id");
        ReplaceDB("form_campo", "foca_rotulo", "foca_id");
        ReplaceDB("form_grupo", "fogr_nome", "fogr_id");
        ReplaceDB("form_campo_multi", "focm_rotulo", "focm_id");
        ReplaceDB("form_campo_multi_campo", "fcmc_rotulo", "fcmc_id");
        ReplaceDB("grade", "grad_titulo", "grad_id");
        ReplaceDB("grade_acao", "grac_msg", "grac_id");
        ReplaceDB("grade_coluna", "grco_rotulo", "grco_id");
        ReplaceDB("grade_coluna", "grco_filtro_valor_nulo", "grco_id");
        ReplaceDB("grade_context_menu", "grcm_titulo", "grcm_id");
        ReplaceDB("grade_context_menu", "grcm_titulo_mais_acoes", "grcm_id");
        ReplaceDB("menu_item", "menu_nome", "menu_id");
        ReplaceDB("modulo", "modu_titulo", "modu_id");
        ReplaceDBAgrupado("modulo_acao", "moac_nome");
        ReplaceDBAgrupado("modulo_acao", "moac_dica");
        ReplaceDBAgrupado("modulo_acao", "moac_restrito_texto");
        ReplaceDB("gadgetv2", "gadg_titulo", "gadg_id", "yoursales");
        ReplaceDB("gadgetv2", "gadg_descricao", "gadg_id", "yoursales");

        ReplaceDBAgrupado("pedido_modelo", "pemo_nome");
        ReplaceDBAgrupado("pedido_modelo", "pemo_nome_distribuidor");

        ReplaceDBAgrupado("pedido_opcoes", "peop_nome");
        ReplaceDBAgrupado("pedido_opcoes", "peop_grupo");

        Response.Write("Finished");

    }


    private void ReplaceDB(string tabelaAlterar, string campoAlterar, string idTabela, string schema = "neo")
    {
        try
        {
            YS.Dados.PostgreSQL.Data data = new YS.Dados.PostgreSQL.Data();
            data.Sql = $@"SELECT * FROM {schema}.{tabelaAlterar} 
                            WHERE {campoAlterar} IS NOT NULL 
                            AND {campoAlterar} NOT LIKE '%[$%'";

            DataTable dt = data.GetDataTable();

            foreach (DataRow dr in dt.Rows)
            {
                var textoOriginal = dr[campoAlterar].ToString();

                var Chave = $"[${Normalize(textoOriginal)}$]";
                if (Chave == "[$$]") continue;

                try
                {
                    data.Sql = @"INSERT INTO yoursales.globalizacao (
                                    glob_cultura, 
                                    glob_texto, 
                                    glob_chave
                                    ) VALUES (
                                    @glob_cultura, 
                                    @glob_texto, 
                                    @glob_chave)";

                    data.AddParameter("@glob_cultura", Culture);
                    data.AddParameter("@glob_texto", textoOriginal);
                    data.AddParameter("@glob_chave", Chave);
                    data.ExecuteQuery();
                }
                catch { }
                data.Sql = $@"UPDATE {schema}.{tabelaAlterar} 
                                SET {campoAlterar}=@chave 
                                WHERE {idTabela}=@id 
                                AND {campoAlterar} NOT LIKE '%[$%'";

                data.AddParameter("@chave", Chave);
                data.AddParameter("@id", dr[idTabela]);
                data.ExecuteQuery();

            }

        }
        catch (Exception er)
        {
            Response.Write(er);
            Response.Write("error");
        }
    }


    private void ReplaceDBAgrupado(string tabelaAlterar, string campoAlterar, string schema = "neo")
    {
        try
        {

            YS.Dados.PostgreSQL.Data data = new YS.Dados.PostgreSQL.Data();
            data.Sql = $@"SELECT MAX({campoAlterar}) AS {campoAlterar} 
                            FROM {schema}.{tabelaAlterar} 
                            WHERE {campoAlterar} IS NOT NULL 
                            AND {campoAlterar} NOT LIKE '%[$%' 
                            GROUP BY {campoAlterar}";

            DataTable dt = data.GetDataTable();

            foreach (DataRow dr in dt.Rows)
            {
                var textoOriginal = dr[campoAlterar].ToString();

                var Chave = $"[${Normalize(textoOriginal)}$]";

                if (Chave == "[$$]") continue;
                try
                {
                    data.Sql = @"INSERT INTO yoursales.globalizacao (
                                    glob_cultura, 
                                    glob_texto, 
                                    glob_chave
                                    ) VALUES (
                                    @glob_cultura, 
                                    @glob_texto, 
                                    @glob_chave)";

                    data.AddParameter("@glob_cultura", Culture);
                    data.AddParameter("@glob_texto", textoOriginal);
                    data.AddParameter("@glob_chave", Chave);
                    data.ExecuteQuery();
                }
                catch { } 
                data.Sql = $@"UPDATE {schema}.{tabelaAlterar} 
                                SET {campoAlterar}=@chave 
                                WHERE {campoAlterar}=@id 
                                AND {campoAlterar} NOT LIKE '%[$%'";

                data.AddParameter("@chave", Chave);
                data.AddParameter("@id", dr[campoAlterar]);
                data.ExecuteQuery();

            }
        }
        catch (Exception er)
        {
            Response.Write(er);
            Response.Write("error");
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