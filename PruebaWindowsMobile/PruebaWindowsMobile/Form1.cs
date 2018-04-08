using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using TraductorTxt;

namespace SmartDeviceProject1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Traductor trad = new Traductor(@"TraductorXMLite.sqlite3", 1);
            if (trad.VerificarConexion())
            {
                CampoCalculado CampCalculado = new CampoCalculado();                
                string ruta = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\" + "1.txt";
                FileStream stream = File.OpenRead(ruta);
                byte[] fileBytes = new byte[stream.Length];
                stream.Read(fileBytes, 0, fileBytes.Length);
                stream.Close();
                trad.Procesar(fileBytes);
                Console.WriteLine("Conexion valida.");
            }
            else
            {
                Console.WriteLine("Conexion invalida.");
            }
            Console.ReadLine();          
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32 * 1024]; // Or whatever size you want
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}