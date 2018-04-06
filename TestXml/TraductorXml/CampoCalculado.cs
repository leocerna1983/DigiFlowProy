using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traductor
{
    public class CampoCalculado
    {
        public string FechaHoraActual(string Documento) {
            string fechastring = Documento.Split('\n').Where(p => p.Split(';')[0] == "A" && p.Split(';')[1] == "FchEmis").First().Split(';')[3].Trim();

            DateTime dt =  new DateTime(int.Parse(fechastring.Split('-')[0]), int.Parse(fechastring.Split('-')[1]), int.Parse(fechastring.Split('-')[2]));
            if (DateTime.Now.ToString("yyyyMMdd") == dt.ToString("yyyyMMdd"))
            {
                return DateTime.Now.ToString("hh:mm:ss.fff").Replace(':', '-');
            }
            else
            {
                return "00:00:00";
            }
        }

        public int CantidadDetalle(string Documento)
        {
            int count = Documento.Split('\n').Where(p => p.Split(';')[0] == "B" && p.Split(';')[1] == "NroLinDet").Count();
            return count;
        }


        public decimal FactorCargoDescuento(string Documento)
        {
            decimal MontoTotal = 0;
            Decimal SumaCuerpoC = 0;
            if (Documento.Split('\n').Where(p => p.Split(';')[0] == "A" && p.Split(';')[1] == "MntTotal").Count() > 0)
            {
                MontoTotal = decimal.Parse(Documento.Split('\n').Where(p => p.Split(';')[0] == "A" && p.Split(';')[1] == "MntTotal").First().Split(';')[3]);
            }
            if (Documento.Split('\n').Where(p => p.Split(';')[0] == "A" && p.Split(';')[1] == "MntTotal").Count() > 0)
            {
                List<CuerpoC> lCuerpoC = ObtenerCuerpoC(Documento);
                SumaCuerpoC = lCuerpoC.Where(p => p.TpoMov == "D").Sum(a=>a.ValorDR);
            }
            if (MontoTotal > 0)
                return (SumaCuerpoC / MontoTotal);
            else
                return 0;
        }

        public List<CuerpoC> ObtenerCuerpoC(string Documento)
        {
            List<CuerpoC> lCuerpoC = new List<CuerpoC>();
            CuerpoC oCuerpoC = new CuerpoC();
            if (Documento.Split('\n').Where(p => p.Split(';')[0] == "C").Count() > 0)
            {
                string nuevodocumento = string.Empty;
                foreach (var item in Documento.Split('\n').Where(p => p.Split(';')[0] == "C"))
                {
                    nuevodocumento += item.Trim()+ '\n';
                }


                for (int i = 0; i < Documento.Split('\n').Where(p => p.Split(';')[0] == "C").Count(); i++)
                {
                    oCuerpoC = new CuerpoC();
                    if (nuevodocumento.Split('\n')[i].Split(';')[1] == "NroLinDR")
                    {
                        oCuerpoC.NroLinDR = int.Parse(nuevodocumento.Split('\n')[i].Split(';')[3].Trim());
                        i++;
                        oCuerpoC.TpoMov = nuevodocumento.Split('\n')[i].Split(';')[3];
                        i++;
                        oCuerpoC.ValorDR = decimal.Parse(nuevodocumento.Split('\n')[i].Split(';')[3].Replace('.', ','));
                        lCuerpoC.Add(oCuerpoC);
                    }
                }
            }
            return lCuerpoC;
        }
    }
}
