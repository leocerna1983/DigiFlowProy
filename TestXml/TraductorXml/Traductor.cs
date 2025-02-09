﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Globalization;

namespace Traductor
{
    public class Traductor
    {
        private int companiaId;
        private int documentoId;
        private string file;

        private string CadenaDocumento = "";
        SQLiteConnection sqliteConexion;
        public Traductor(string File, int CompaniaId=1){
            companiaId = CompaniaId;            
            file = File;
        }        

        public bool VerificarConexion() {
            try
            {
                if (File.Exists(file))
                {
                    sqliteConexion = new SQLiteConnection("Data Source=" + file + "");
                    sqliteConexion.Open();
                    sqliteConexion.Close();
                    return true;
                }
                else {
                    sqliteConexion = null;
                    return false;
                }
            }
            catch (SQLiteException ex)
            {
                return false;
            }           
        }

        public byte[] Procesar(byte[] bytes,int pDocumentoId) {

            byte[] Resultado;
            documentoId = pDocumentoId;
            string NuevoDocumento = string.Empty;
            if (documentoId > 0)
            {
                if (VerificarConexion())
                {
                    string cadena = System.Text.Encoding.Default.GetString(bytes);                                                            
                    if (sqliteConexion == null)
                    {
                        throw new Exception("Error inicializando base de datos.");
                    }
                    else
                    {
                        String[] ColumnasProcesar = ObtenerColumnasProcesar(companiaId, documentoId);
                        List<configuracion> lConfiguracion = ObtenerConfiguracionDocumento(documentoId);

                        int i = 0;
                        bool listoc = false;
                        foreach (var item in cadena.Split('\n'))
                        {
                            //Encabezado
                            //verificar si la columna tiene alguna configuracion para hacer reemplazo del valor
                            //o asignacion
                            if (item.Split(';').Length == 4)
                            {
                                if (ColumnasProcesar.Contains(item.Split(';')[1].ToString()))
                                {
                                    configuracion Conf = ObtenerConfiguracionColumna(companiaId, item.Split(';')[1].ToString(), documentoId );
                                    //El reeemplazo es por catalogo
                                    if (Conf.escatalogo > 0)
                                    {
                                        string valorReemplazo = ObtenerValorCatalogo(Conf.companiaid, Conf.catalogoid, item.Split(';')[3]);                                        
                                        if(valorReemplazo!=string.Empty)
                                            NuevoDocumento += item.Split(';')[0] + ";" + item.Split(';')[1] + ";" + item.Split(';')[2] + ";" + valorReemplazo + "\r\n";
                                        else
                                            NuevoDocumento += item.Split(';')[0] + ";" + item.Split(';')[1] + ";" + item.Split(';')[2] + ";" + item.Split(';')[3] + "\r\n";
                                    }
                                    else
                                    {
                                        //El reemplazo es por valor por defecto
                                        if (Conf.esvalordefecto > 0)
                                        {
                                            NuevoDocumento += item.Split(';')[0] + ";" + item.Split(';')[1] + ";" + item.Split(';')[2] + ";" + Conf.valordefecto + "\r\n";
                                        }
                                        else
                                        {
                                            if (Conf.escalculado > 0)
                                            {
                                                CampoCalculado CampCalc = new CampoCalculado();

                                                switch (Conf.calculoid)
                                                {
                                                    case 1:
                                                        string Hora = CampCalc.FechaHoraActual(cadena);
                                                        NuevoDocumento += item.Split(';')[0] + ";" + item.Split(';')[1] + ";" + item.Split(';')[2] + ";" + Hora + "\r\n";

                                                        break;
                                                    case 2:
                                                        int CantidadLineas = CampCalc.CantidadDetalle(cadena);
                                                        NuevoDocumento += item.Split(';')[0] + ";" + item.Split(';')[1] + ";" + item.Split(';')[2] + ";" + CantidadLineas.ToString() + "\r\n";
                                                        break;
                                                    case 3:
                                                        decimal FactorCargoDescuento = CampCalc.FactorCargoDescuento(cadena);
                                                        NuevoDocumento += item.Split(';')[0] + ";" + item.Split(';')[1] + ";" + item.Split(';')[2] + ";" + FactorCargoDescuento.ToString() + "\r\n";
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                NuevoDocumento += item+ "\n";                                                
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    NuevoDocumento += item + "\n";
                                }
                            }
                        }

                        string seccionagregar = string.Empty;
                        string secciondocumento = string.Empty;
                        string nuevasecciondocumento = string.Empty;

                        string DocumentoResultante = string.Empty;
                        bool listo = false;

                        

                        foreach (var item in lConfiguracion.FindAll(p=>p.verificado==false).OrderBy(o=>o.seccion).ToList<configuracion>())
                        {
                            if (item.verificado == false)
                            { 
                                seccionagregar = item.seccion;
                                listo = false;
                                secciondocumento = string.Empty;
                                foreach (var item1 in NuevoDocumento.Split('\n'))
                                {
                                    if (item1.Split(';')[0] == seccionagregar)
                                    {
                                        secciondocumento = item1.Split(';')[0];
                                    }
                                    if (secciondocumento != string.Empty && item1.Split(';')[0] != secciondocumento && !listo)
                                    {

                                        if(!(secciondocumento=="C"))
                                            item.verificado = true;
                                        if (item.esvalordefecto > 0)
                                        {
                                            if (secciondocumento == "C")
                                            {
                                            
                                                CampoCalculado CC = new CampoCalculado();
                                                List<CuerpoC> lCuerpoc= CC.ObtenerCuerpoC(NuevoDocumento);
                                                string DocumentoCuerpoC = DocumentoResultante;
                                                string NuevoDocumentoCuerpoC = string.Empty;
                                                foreach (var itemc in DocumentoCuerpoC.Split('\n'))
                                                {
                                                    if (!listoc)
                                                    {
                                                        if (itemc.Split(';')[0] != "C")
                                                        {
                                                            NuevoDocumentoCuerpoC += itemc + "\n";
                                                        }
                                                        else
                                                        {
                                                    
                                                            listoc = true;
                                                            int linea = 0;
                                                            NumberFormatInfo nfi = new NumberFormatInfo();
                                                            foreach (var itemCuerpoC in lCuerpoc)
                                                            {
                                                                linea++;
                                                                NuevoDocumentoCuerpoC += "C" + ";" + "NroLinDR" + ";1;" + linea.ToString() + "\r\n";

                                                                NuevoDocumentoCuerpoC += "C" + ";" + "TpoMov" + ";1;" + itemCuerpoC.TpoMov + "\r\n";
                                                                nfi.NumberDecimalDigits = 2;
                                                                nfi.NumberGroupSeparator = "";
                                                                NuevoDocumentoCuerpoC += "C" + ";" + "ValorDR" + ";1;" + itemCuerpoC.ValorDR.ToString(nfi) + "\r\n";


                                                                foreach (var item2 in lConfiguracion.Where(p=>p.seccion=="C"))
                                                                {
                                                                    if (item2.esvalordefecto == 1)
                                                                    {
                                                                        NuevoDocumentoCuerpoC += "C" + ";" + item2.nombrecolumna + ";1;" + item2.valordefecto + "\r\n";
                                                                    }
                                                                    else {
                                                                        if (item2.escatalogo == 1)
                                                                        {

                                                                        }
                                                                        else {
                                                                            if (item2.escalculado == 1)
                                                                            {
                                                                                CampoCalculado CampCalc = new CampoCalculado();
                                                                                
                                                                                nfi.NumberDecimalSeparator = ".";

                                                                                switch (item2.calculoid)
                                                                                {
                                                                                    case 3:
                                                                                        decimal FactorCargoDescuento = CampCalc.FactorCargoDescuento(cadena);
                                                                                        nfi.NumberDecimalDigits = 6;
                                                                                        nfi.CurrencyDecimalDigits = 6;
                                                                                        nfi.NumberGroupSeparator = "";
                                                                                        NuevoDocumentoCuerpoC += "C" + ";"+item2.nombrecolumna+";1;" + FactorCargoDescuento.ToString(nfi) + "\r\n";
                                                                                        break;
                                                                                    case 4:
                                                                                        decimal MontoCargoDescuento = CampCalc.MontoCargoDescuento(cadena);
                                                                                        nfi.NumberDecimalDigits = 2;
                                                                                        nfi.NumberGroupSeparator = "";
                                                                                        NuevoDocumentoCuerpoC += "C" + ";" + item2.nombrecolumna + ";1;" + MontoCargoDescuento.ToString(nfi) + "\r\n";
                                                                                        break;
                                                                                    case 5:
                                                                                        decimal MtoTotal = CampCalc.MontoTotal(cadena);
                                                                                        nfi.NumberDecimalDigits = 2;
                                                                                        nfi.NumberGroupSeparator = "";
                                                                                        NuevoDocumentoCuerpoC += "C" + ";" + item2.nombrecolumna + ";1;" + MtoTotal.ToString(nfi) + "\r\n";
                                                                                        break;
                                                                                    default:
                                                                                        break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                            if(lConfiguracion.Find(p => p.nombrecolumna == "IndCargoDescuento")!=null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "IndCargoDescuento").verificado = true;

                                                            if (lConfiguracion.Find(p => p.nombrecolumna == "FactorCargoDescuento") != null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "FactorCargoDescuento").verificado = true;

                                                            if (lConfiguracion.Find(p => p.nombrecolumna == "MBaseCargoDescuento") != null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "MBaseCargoDescuento").verificado = true;

                                                            if (lConfiguracion.Find(p => p.nombrecolumna == "MontoCargoDescuento") != null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "MontoCargoDescuento").verificado = true;

                                                            if (lConfiguracion.Find(p => p.nombrecolumna == "IndCargoDescuento") != null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "IndCargoDescuento").verificado = true;

                                                            if (lConfiguracion.Find(p => p.nombrecolumna == "CodigoCargoDescuento") != null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "CodigoCargoDescuento").verificado = true;                                                        

                                                            if (lConfiguracion.Find(p => p.nombrecolumna == "IndCargoDescuento1") != null)
                                                                lConfiguracion.Find(p => p.nombrecolumna == "IndCargoDescuento1").verificado = true;

                                                        }
                                                    }
                                                    DocumentoResultante = NuevoDocumentoCuerpoC;
                                                    NuevoDocumento = NuevoDocumentoCuerpoC;
                                                }                                            
                                                DocumentoResultante = NuevoDocumentoCuerpoC;
                                            }
                                            else
                                                DocumentoResultante += secciondocumento + ";" + item.nombrecolumna + ";;" + item.valordefecto + "\r\n";
                                        }
                                        else
                                        {
                                            if (item.escalculado > 0)
                                            {
                                                CampoCalculado CampCalc = new CampoCalculado();

                                                switch (item.calculoid)
                                                {
                                                    case 1:
                                                        string Hora = CampCalc.FechaHoraActual(cadena);
                                                        DocumentoResultante += secciondocumento + ";" + item.nombrecolumna + ";;" + Hora + "\r\n";

                                                        break;
                                                    case 2:
                                                        int CantidadLineas = CampCalc.CantidadDetalle(cadena);
                                                        DocumentoResultante += secciondocumento + ";" + item.nombrecolumna + ";;" + CantidadLineas.ToString() + "\r\n";
                                                        break;
                                                    //case 3:
                                                    //    decimal FactorCargoDescuento = CampCalc.FactorCargoDescuento(cadena);
                                                    //    DocumentoResultante += secciondocumento + ";" + item.nombrecolumna + ";;" + FactorCargoDescuento.ToString() + "\r\n";
                                                    //    break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                DocumentoResultante += item + "\r\n";
                                            }
                                        }
                                        listo = true;
                                    }
                                    DocumentoResultante += item1 + "\n";
                                }
                            NuevoDocumento = DocumentoResultante;
                            DocumentoResultante = string.Empty;
                            }
                        }
                                         
                    }
                }
                else
                {
                    throw new Exception("Base Datos Configuracion no encontrada.");
                }
            }
            else
            {
                throw new Exception("Tipo Documento Id invalido para procesamiento, por favor verifique.");
            }
            Resultado = Encoding.Default.GetBytes(NuevoDocumento.Trim());
            return Resultado;
        }


        string[] ObtenerColumnasProcesar(int pCompaniaId, int pdocumentoid) {
            List<string> lColumnasProcesar = new List<string>();

            DataSet ds = new DataSet();
            SQLiteCommand cmd = new SQLiteCommand(string.Format("select NombreColumna from configuracion where companiaid = '{0}' and documentoid = {1}", pCompaniaId, pdocumentoid), sqliteConexion);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            sqliteConexion.Open();
            da.Fill(ds);
            sqliteConexion.Close();

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                lColumnasProcesar.Add(item["NombreColumna"].ToString());
            }

            return lColumnasProcesar.ToArray();
        }

        configuracion ObtenerConfiguracionColumna(int pCompaniaId, string pColumna, int pDocumentoid)
        {
            configuracion conf = new configuracion();
            DataSet ds = new DataSet();
            SQLiteCommand cmd = new SQLiteCommand(string.Format(@"select id, nombrecolumna, Escatalogo, catalogoid, 
            companiaid, esvalordefecto, valordefecto, escalculado, calculoid from configuracion where companiaid = '{0}' and nombrecolumna= '{1}' and documentoid = {2}", pCompaniaId, pColumna, pDocumentoid), sqliteConexion);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            sqliteConexion.Open();
            da.Fill(ds);
            sqliteConexion.Close();

            if (ds.Tables[0].Rows.Count > 0)
            {
                conf = new configuracion() {
                    calculoid = int.Parse(ds.Tables[0].Rows[0]["calculoid"].ToString()),
                    catalogoid = int.Parse(ds.Tables[0].Rows[0]["catalogoid"].ToString()),
                    companiaid = int.Parse(ds.Tables[0].Rows[0]["companiaid"].ToString()),
                    escalculado = int.Parse(ds.Tables[0].Rows[0]["escalculado"].ToString()),
                    escatalogo = int.Parse(ds.Tables[0].Rows[0]["Escatalogo"].ToString()),
                    esvalordefecto = int.Parse(ds.Tables[0].Rows[0]["esvalordefecto"].ToString()),
                    id = int.Parse(ds.Tables[0].Rows[0]["id"].ToString()),
                    nombrecolumna = ds.Tables[0].Rows[0]["nombrecolumna"].ToString(),
                    valordefecto = ds.Tables[0].Rows[0]["valordefecto"].ToString()
                };
            }
            return conf;
        }
        string ObtenerValorCatalogo(int CompaniaId, int CatalogoId, string ValorColumna)
        {
            string valordestino = string.Empty;
            configuracion conf = new configuracion();
            DataSet ds = new DataSet();
            SQLiteCommand cmd = new SQLiteCommand(string.Format(@"select valordestino from detallecatalogo 
            where catalogoid={0} and companiaid ={1} and valororigen='{2}'", CatalogoId, CompaniaId, ValorColumna.Trim()), sqliteConexion);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            sqliteConexion.Open();
            da.Fill(ds);
            sqliteConexion.Close();

            if (ds.Tables[0].Rows.Count > 0)
            {
                valordestino = ds.Tables[0].Rows[0]["valordestino"].ToString();
            }
            return valordestino;
        }


        List<configuracion> ObtenerConfiguracionDocumento(int DocumentoId)
        {
            List<configuracion> lConfiguracionDocumento = new List<configuracion>();

            configuracion conf = new configuracion();
            DataSet ds = new DataSet();
            SQLiteCommand cmd = new SQLiteCommand(
                string.Format(@"SELECT [Id],[NombreColumna],[EsCatalogo],[CatalogoId],
                                [CompaniaId],[EsValorDefecto],[ValorDefecto],[EsCalculado],
                                [CalculoId],[DocumentoId],[Seccion]
                                FROM [Configuracion] where DocumentoId = {0}", DocumentoId), sqliteConexion);
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            sqliteConexion.Open();
            da.Fill(ds);
            foreach (DataRow item in ds.Tables[0].Rows)
            {
                lConfiguracionDocumento.Add(new configuracion() {
                    calculoid = int.Parse(item["CalculoId"].ToString()),
                    catalogoid = int.Parse(item["CatalogoId"].ToString()),
                    companiaid = int.Parse(item["CompaniaId"].ToString()),
                    documentoid = int.Parse(item["DocumentoId"].ToString()),
                    escalculado = int.Parse(item["EsCalculado"].ToString()),
                    escatalogo = int.Parse(item["EsCatalogo"].ToString()),
                    esvalordefecto = int.Parse(item["EsValorDefecto"].ToString()),
                    id = int.Parse(item["Id"].ToString()),
                    nombrecolumna = item["NombreColumna"].ToString(),
                    valordefecto = item["ValorDefecto"].ToString(),
                    verificado = (int.Parse(item["EsCatalogo"].ToString()) > 0) ? true: false,
                    seccion = item["Seccion"].ToString()
                });
            }

            sqliteConexion.Close();

            return lConfiguracionDocumento;
        }

    }

    public class Compania
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}

