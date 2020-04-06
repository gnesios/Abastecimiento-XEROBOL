using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.SharePoint;

namespace LogEventos
{
    public class LogArchivo
    {
        private string m_archivo;

        public string Archivo
        {
            get { return m_archivo; }
            set { m_archivo = value; }
        }

        public LogArchivo(string archivo)
        {
            this.Archivo = archivo;
        }

        #region Metodos
        public string Read(string codigo)
        {
            // Prepara linea codigo
            string lineaCodigo = "### " + codigo + " ###";

            // Busca linea codigo en archivo
            string[] lineas = File.ReadAllLines(Archivo);
            int i = 0;
            for (i = 0; i <= lineas.Length - 1; i++)
            {
                if (lineas[i] == lineaCodigo)
                {
                    break;
                }
            }

            // Si encuentra, devuelve mensaje
            if (i <= lineas.Length - 1)
            {
                int j = i + 1;
                StringBuilder mensaje = new StringBuilder();
                while (j < lineas.Length && !string.IsNullOrEmpty(lineas[j]))
                {
                    mensaje.AppendLine(lineas[j]);
                    j += 1;
                }
                return mensaje.ToString();
            }

            // Devuelve mensaje de errror
            return "*** ERROR ***";

        }

        public string WriteEvent(string mensaje)
        {
            // Arma codigo con fecha y Guid
            string fechaHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string subcodigo = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            string codigo = fechaHora + " " + subcodigo;

            // Prepara linea codigo
            string lineaCodigo = "### " + codigo + " ###";

            // Prepara lineas mensaje
            string lineasMensajes = lineaCodigo + Environment.NewLine + mensaje + Environment.NewLine;

            // Agrega a archivo log
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {//con privilegios
                File.AppendAllText(Archivo, lineasMensajes);
            });

            // Devuelve codigo
            return codigo;

        }
        public string WriteException(Exception ex)
        {
            StringBuilder mensaje = new StringBuilder();

            do
            {
                mensaje.AppendLine("*** Exception ***");
                mensaje.AppendLine(string.Format("Type: {0}", ex.GetType().FullName));
                mensaje.AppendLine(string.Format("Message: {0}", ex.Message));
                mensaje.AppendLine(string.Format("Source: {0}", ex.Source));
                mensaje.AppendLine(string.Format("Procedure: {0}", ex.TargetSite.Name));
                mensaje.AppendLine(string.Format("StackTrace: {0}", ex.StackTrace));
                ex = ex.InnerException;
            } while (ex != null);

            return WriteEvent(mensaje.ToString());
        }
        #endregion
    }
}
