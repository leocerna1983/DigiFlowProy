using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Traductor;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            Traductor.Traductor trad = new Traductor.Traductor(@"TraductorXMLite.sqlite3");
            if (trad.VerificarConexion())
            {
                CampoCalculado CampCalculado = new CampoCalculado();
                //string ruta = @"C:\01 TXTs\01 Homologacion\Grupo 11 - Ventas con Descuento Global\BB14_0003_Boleta3_con_6_Items_DG.txt";
                string ruta = @"C:\01 TXTs\01 Homologacion\Grupo 01 - Ventas gravadas IGV\DocumentoColumnaNueva.txt";
                
                byte[] bytes = File.ReadAllBytes(ruta);
                trad.Procesar(bytes,1);                
                Console.WriteLine("Conexion valida.");
            }
            else
            {
                Console.WriteLine("Conexion invalida.");
            }
            Console.ReadLine();

        }
    }
}