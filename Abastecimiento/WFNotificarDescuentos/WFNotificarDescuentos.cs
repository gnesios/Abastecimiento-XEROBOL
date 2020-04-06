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
using System.Collections.Generic;
using System.Globalization;

namespace Abastecimiento.WFNotificarDescuentos
{
    public sealed partial class WFNotificarDescuentos : SequentialWorkflowActivity
    {
        public WFNotificarDescuentos()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();

        public string GRUPO_SOLICITUD_OC = "Responsables de Solicitud de OC";
        public string LISTA_DESCUENTOS = "Descuentos Proveedores";

        public SPListItem itemObjetivo = null;
        public string mensajeHistorial = "";
        public string usuariosNotificados = "";
        public string usuariosNotificadosCC = "";
        public string asuntoNotificacion = "";
        public string cuerpoNotificacion = "";

        private void RecuperarValoresItem_ExecuteCode(object sender, EventArgs e)
        {
            itemObjetivo = workflowProperties.Item;

            try
            {
                usuariosNotificados = this.UsuariosANotificar();
                //usuariosNotificadosCC = workflowProperties.OriginatorEmail;
                asuntoNotificacion = "Abastecimiento, ítems con descuentos";
                cuerpoNotificacion = this.CuerpoCorreoNotificacion();
                mensajeHistorial =
                    "Notificación a 'Responsables de Solicitud de OC' realizada exitosamente.";
            }
            catch (Exception ex)
            {
                #region Registro de Evento Error
                LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
                log.WriteEvent("--- [FLUJO] RecuperarValoresItem_ExecuteCode flujo 'Notificar descuentos' ---");
                log.WriteException(ex);
                #endregion

                logNotificarDescuentos.EventId = SPWorkflowHistoryEventType.WorkflowError;
                logNotificarDescuentos.HistoryOutcome = "Error";
                logNotificarDescuentos.HistoryDescription = ex.Message;
            }
        }

        /// <summary>
        /// Recupera los correos de los usuarios del grupo 'Responsables de Solicitud de OC'
        /// </summary>
        /// <returns></returns>
        private string UsuariosANotificar()
        {
            string correosUsuarios = "";
            SPGroup grupoSolicitud = workflowProperties.Web.Groups[GRUPO_SOLICITUD_OC];

            foreach (SPUser usuario in grupoSolicitud.Users)
            {
                correosUsuarios = correosUsuarios + usuario.Email + "; ";
            }

            return correosUsuarios;
        }

        /// <summary>
        /// Formatea el cuerpo del correo con información sobre descuentos
        /// </summary>
        /// <returns></returns>
        private string CuerpoCorreoNotificacion()
        {
            string urlItemDescuentos = workflowProperties.Web.Url +
                workflowProperties.Web.Lists[LISTA_DESCUENTOS].DefaultDisplayFormUrl + "?ID=";
            string urlListaDescuentos = workflowProperties.Web.Url +
                workflowProperties.Web.Lists[LISTA_DESCUENTOS].DefaultViewUrl;
            string urlItemOC = workflowProperties.Web.Url +
                workflowProperties.List.DefaultDisplayFormUrl + "?ID=";

            #region Formatear items
            SPListItemCollection itemsDescuentos =
                workflowProperties.Web.Lists[LISTA_DESCUENTOS].Items;
            SPListItemCollection itemsPedidos =
                EjecutorOperacionesSP.RecuperarItemsPedidosAsociados(itemObjetivo.ID, itemObjetivo.Web);//TODO Probar descuentos

            string formatoItems = "";
            foreach (SPListItem itemPedido in itemsPedidos)
            {
                bool tieneDescuento = false;
                string formatoDescuentos = "";
                foreach (SPListItem itemDescuento in itemsDescuentos)
                {
                    if (itemPedido["Ítem asociado"].ToString() == itemDescuento["Ítem asociado"].ToString())
                    {
                        tieneDescuento = true;
                        formatoDescuentos = formatoDescuentos +
                            string.Format("<li><a href='{0}'>{1}</a> - {2}</li>",
                            urlItemDescuentos + itemDescuento.ID, itemDescuento.Title,
                            this.FormatoSubcadena(itemDescuento["Proveedor asociado"]));
                    }
                }

                if (tieneDescuento)
                {
                    formatoDescuentos = "<ul style='list-style-type:none'>" + formatoDescuentos + "</ul>";
                    formatoItems = formatoItems +
                        string.Format("<li><b>{0}</b>{1}</li>", this.FormatoSubcadena(itemPedido["Ítem asociado"]),
                        formatoDescuentos);
                }
            }

            if (formatoItems != "")
                formatoItems = "<ul style='margin-left:15px'>" + formatoItems + "</ul>";
            else
                formatoItems = "<i>No existen descuentos asociados a los ítems pedidos en la OC indicada.</i>";
            #endregion

            #region Formatear todo el cuerpo
            string cuerpoCorreo = string.Format(
                "<table border='0' cellspacing='0' cellpadding='0' width='100%' style='width:100%;border-collapse:collapse;'>" +
                "<tr><td style='border:solid #E8EAEC 1.0pt;background:#F8F8F9;padding:12.0pt 7.5pt 15.0pt 7.5pt'>" +
                "<p style='font-size:15.0pt;font-family:Verdana,sans-serif;'>" +
                "Sistema de Abastecimiento: Items con descuentos de la OC <a href='{0}'>{1}</a></p></td></tr>" +
                "<tr><td style='border:none;border-bottom:solid #9CA3AD 1.0pt;padding:4.0pt 7.5pt 4.0pt 7.5pt'>" +
                "<p style='font-size:10.0pt;font-family:Tahoma,sans-serif'>" +
                "A continuación se listan los descuentos aplicables a la Orden de Compra '{1}' (ID: <b>{2}</b>). " +
                "Puede navegar a la página de <a href='{3}'>{4}</a> para ver la lista de todos los descuentos aplicables, " +
                "o puede seguir el enlace asociado a cada ítem para tener un detalle de cada descuento.</p>" +
                formatoItems +
                "<p style='font-size:8.0pt;font-family:Tahoma,sans-serif;'>" +
                "Última modificación realizada por {5} en fecha {6}</p></td></tr></table>",
                urlItemOC + itemObjetivo.ID, itemObjetivo.Title, itemObjetivo.ID, urlListaDescuentos,
                workflowProperties.Web.Lists[LISTA_DESCUENTOS].Title, this.FormatoSubcadena(itemObjetivo["Modificado por"]),
                Convert.ToDateTime(itemObjetivo["Modificado"].ToString()).ToString("dd/MM/yyyy HH:mm"));
            #endregion

            return cuerpoCorreo;
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
