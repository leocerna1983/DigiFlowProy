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
                string ruta = @"C:\01 TXTs\01 Homologacion\Grupo 11 - Ventas con Descuento Global\descuento1.txt";

                var utf8 = Encoding.UTF8;

                string TextoArchivo  = File.ReadAllText(ruta, Encoding.Default);

                byte[] bytes = Encoding.Default.GetBytes(TextoArchivo);


                byte[] bytesoutput  = trad.Procesar(bytes,1);      
                
                string output = System.Text.Encoding.Default.GetString(bytesoutput);

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