using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using System.Globalization;

namespace Abastecimiento.WFNotificarAprobacion
{
    public sealed partial class WFNotificarAprobacion : SequentialWorkflowActivity
    {
        public WFNotificarAprobacion()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();
        public SPListItem itemOC;

        public bool autonomina = false;

        public string asuntoNotificacion = "";
        public string cuerpoNotificacion = "";
        public string usuariosNotificados = "";
        public string usuariosCopiados = "";
        public string mensajeHistorial = "";


        string LISTA_PARAMETROS = "Parámetros del Sistema";
        string GRUPO_APROBADORES_OC = "Aprobadores de OC";
        string nombreParametro = "Autonomía OC";


        private void RecuperarInformacion_ExecuteCode(object sender, EventArgs e)
        {
            itemOC = workflowProperties.Item;

            try
            {
                autonomina = this.VerificarAutonomia();

                if (autonomina)
                {
                    #region Aprobar automaticamente el item
                    SPModerationInformation estadoAprobacion = itemOC.ModerationInformation;
                    estadoAprobacion.Status = SPModerationStatusType.Approved;

                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {//Para evitar que ejecute una y otra vez el Evento ItemUpdating sobre este item
                        itemOC.SystemUpdate();
                    }
                    #endregion

                    #region Actualizar bitacora
                    //EjecutorOperacionesSP.ActualizarBitacoraAprobacion(itemOC);
                    #endregion

                    #region Notificacion por correo
                    asuntoNotificacion = "Abastecimiento, orden de compra aprobada";
                    cuerpoNotificacion = this.CuerpoCorreoNotificacion();
                    usuariosNotificados = this.UsuariosANotificar();
                    usuariosCopiados = this.UsuariosACopiar();
                    mensajeHistorial = "Notificación por aprobación automática realizada exitosamente.";
                    #endregion

                    #region Iniciar el flujo 'Notificar Descuentos'
                    EjecutorOperacionesSP.IniciarFlujoNotificarDescuentos(itemOC);
                    #endregion
                }
                else
                {
                    #region Notificacion por correo
                    asuntoNotificacion = "Abastecimiento, solictud de aprobación";
                    cuerpoNotificacion = this.CuerpoCorreoSolicitud();
                    usuariosNotificados = this.UsuariosANotificar();
                    //usuariosCopiados = this.UsuariosACopiar();
                    mensajeHistorial = "Notificación a 'Aprobadores de OC' realizada exitosamente.";
                    #endregion
                }
            }
            catch (Exception ex)
            {
                #region Registro de Evento Error
                LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
                log.WriteEvent("--- [FLUJO] RecuperarInformacion_ExecuteCode flujo 'Notificar aprobación' ---");
                log.WriteException(ex);
                #endregion

                logNotificarAprobacion.EventId = SPWorkflowHistoryEventType.WorkflowError;
                logNotificarAprobacion.HistoryOutcome = "Error";
                logNotificarAprobacion.HistoryDescription = ex.Message;
            }
        }

        private bool VerificarAutonomia()
        {
            double montoAutonomia = 0;
            double precioTotalOC = double.Parse(itemOC["Precio total"].ToString());

            #region Recuperar autonomia de usuario
            SPQuery consulta = new SPQuery();
            consulta.Query = "<Where><Eq><FieldRef Name='Title'/>" +
                "<Value Type='Text'>" + nombreParametro + "</Value></Eq></Where>";

            SPListItemCollection parametros =
                workflowProperties.Web.Lists[LISTA_PARAMETROS].GetItems(consulta);

            foreach (SPListItem parametro in parametros)
            {
                if (parametro["Usuario parámetro"] != null &&
                    parametro["Usuario parámetro"].ToString() == itemOC["Creado por"].ToString())
                {
                    montoAutonomia = Convert.ToDouble(parametro["Valor parámetro"]);
                    break;
                }
            }
            #endregion

            if (montoAutonomia != 0 && montoAutonomia >= precioTotalOC)
                return true;
            return false;
        }

        /// <summary>
        /// Formato del cuerpo del correo de aprobacion automatica
        /// </summary>
        /// <returns></returns>
        private string CuerpoCorreoNotificacion()
        {
            string urlItemOC = workflowProperties.Web.Url +
                workflowProperties.List.DefaultDisplayFormUrl + "?ID=";

            string cuerpoCorreo = string.Format(
                "<table border='0' cellspacing='0' cellpadding='0' width='100%' style='width:100%;border-collapse:collapse;'>" +
                "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                "Sistema de Abastecimiento: OC <a href='{0}'>{1}</a> aprobada automáticamente</p></td></tr>" +
                "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                "La orden de compra '{1}' (ID: <b>{2}</b>) fue aprobada automáticamente para su pedido." +
                "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                "Orden de compra creada por {3} en fecha {4}</p></td></tr></table>",
                urlItemOC + itemOC.ID, itemOC.Title, itemOC.ID,
                this.FormatoSubcadena(itemOC["Creado por"]),
                Convert.ToDateTime(itemOC["Creado"].ToString()).ToString("dd/MM/yyyy HH:mm"));

            return cuerpoCorreo;
        }

        /// <summary>
        /// Formato del cuerpo del correo de solicitud de aprobacion
        /// </summary>
        /// <returns></returns>
        private string CuerpoCorreoSolicitud()
        {
            string urlItemOC = workflowProperties.Web.Url +
                workflowProperties.List.DefaultDisplayFormUrl + "?ID=";

            string cuerpoCorreo = string.Format(
                "<table border='0' cellspacing='0' cellpadding='0' width='100%' style='width:100%;border-collapse:collapse;'>" +
                "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                "Sistema de Abastecimiento: Solicitud de aprobación para OC <a href='{0}'>{1}</a></p></td></tr>" +
                "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                "La orden de compra '{1}' (ID: <b>{2}</b>) requiere aprobación para su pedido." +
                "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                "Orden de compra creada por {3} en fecha {4}</p></td></tr></table>",
                urlItemOC + itemOC.ID, itemOC.Title, itemOC.ID,
                this.FormatoSubcadena(itemOC["Creado por"]),
                Convert.ToDateTime(itemOC["Creado"].ToString()).ToString("dd/MM/yyyy HH:mm"));

            return cuerpoCorreo;
        }

        /// <summary>
        /// Recupera los correos de los usuarios del grupo 'Aprobadores de OC'
        /// </summary>
        /// <returns></returns>
        private string UsuariosANotificar()
        {
            string correosUsuarios = "";
            SPGroup grupoAprobadores = workflowProperties.Web.Groups[GRUPO_APROBADORES_OC];

            foreach (SPUser usuario in grupoAprobadores.Users)
            {
                correosUsuarios = correosUsuarios + usuario.Email + "; ";
            }

            return correosUsuarios;
        }

        /// <summary>
        /// Recupera los correos de los usuarios a los que sera copiado el correo
        /// </summary>
        /// <returns></returns>
        private string UsuariosACopiar()
        {
            return workflowProperties.OriginatorEmail;
        }

        /// <summary>
        /// Extrae el valor CLIENTE de una cadena de tipo "1;#CLIENTE"
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado de la cadena</returns>
        private string FormatoSubcadena(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno = valor.ToString().Substring(valor.ToString().IndexOf('#') + 1);

            return retorno;
        }
    }
}
