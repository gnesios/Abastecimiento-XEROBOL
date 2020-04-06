using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Collections.Generic;
using System.Globalization;

namespace Abastecimiento.EREventosListas
{
    public class EREventosListas : SPItemEventReceiver
    {
        private string LISTA_OCI = "Órdenes de Compra Internas";
        private string LISTA_OCP = "Órdenes de Compra Proveedor";
        private string LISTA_ITEMS_PEDIDOS = "Ítems Pedidos";
        private string LISTA_FACTURAS = "Facturas";
        private string LISTA_ALMACEN = "Almacén (Warehouse)";
        private string LISTA_ENVIOS = "Envíos";
        private string LISTA_DOCUMENTOS_LEGALES = "Documentos Legales";
        //private string GRUPO_PROPIETARIOS = "Propietarios Abastecimiento XEROBOL";
        //private string GRUPO_APROBADORES = "Aprobadores de OC";

       /// <summary>
       /// An item is being added.
       /// </summary>
       public override void ItemAdding(SPItemEventProperties properties)
       {
           //base.ItemAdding(properties);
           try
           {
               #region Evento lista 'Envíos'
               if (properties.ListTitle == LISTA_ENVIOS)
               {
                   #region Validar ítems pedidos validos para envio
                   List<string> itemsNoValidos =
                       EjecutorOperacionesSP.RecuperarItemsPedidosNoValidosParaEnvio(properties);

                   //Validar si la urgencia del ítem pedido corresponde o no al tipo de envio definido
                   if (itemsNoValidos.Count != 0)
                   {
                       string linksItems = "";

                       foreach (string itemNoValido in itemsNoValidos)
                       {
                           linksItems += itemNoValido + "<br/>";
                       }

                       properties.Status = SPEventReceiverStatus.CancelWithError;
                       properties.ErrorMessage =
                           string.Format("Los ítems siguientes no pueden ser asociados al " +
                           "tipo de envío elegido:<br/>{0}<br/><br/>Vuelva a la página anterior " +
                           "y seleccione otro tipo de envío.", linksItems);
                       properties.Cancel = true;
                   }
                   #endregion
               }
               #endregion
           }
           catch (Exception ex)
           {
               #region Registro de Evento Error
               LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
               log.WriteEvent("--- [EVENTO] ItemAdding ---");
               log.WriteException(ex);
               #endregion

               properties.Status = SPEventReceiverStatus.CancelWithError;
               properties.ErrorMessage = ex.Message;
               properties.Cancel = true;
           }
       }

       /// <summary>
       /// An item is being updated.
       /// </summary>
       public override void ItemUpdating(SPItemEventProperties properties)
       {
           //base.ItemUpdating(properties);
           try
           {
               #region Evento lista 'Órdenes de Compra Internas'
               if (properties.ListTitle == LISTA_OCI)
               {
                   
                    #region Permitir edicion de la OC
                    //string creadoPor = EjecutorOperacionesSP.SubcadenaAntes(properties.ListItem["Creado por"]);

                    //if (properties.CurrentUserId != Convert.ToInt32(creadoPor) &&
                    //    !properties.Web.Groups[GRUPO_PROPIETARIOS].ContainsCurrentUser &&
                    //    !properties.Web.Groups[GRUPO_APROBADORES].ContainsCurrentUser)
                    //{
                    //    properties.Status = SPEventReceiverStatus.CancelWithError;
                    //    properties.ErrorMessage = "Solo el usuario creador (" +
                    //        EjecutorOperacionesSP.SubcadenaDespues(properties.ListItem["Creado por"]) + ") " +
                    //        "tiene permitido modificar esta orden de compra.";
                    //    properties.Cancel = true;

                    //    return;
                    //}
                    #endregion

                    #region Negar edición de 'Precio total'
                    if (properties.AfterProperties["Precio_x0020_total"] != null)
                    {
                        string precioAntes =
                            properties.ListItem["Precio_x0020_total"].ToString();
                        string precioDespues =
                            properties.AfterProperties["Precio_x0020_total"].ToString().Replace('.', ',');

                        if (precioDespues != precioAntes)
                        {
                            properties.Status = SPEventReceiverStatus.CancelWithError;
                            properties.ErrorMessage = "El campo 'Precio total' no puede ser modificado directamente. " +
                                "Este campo es auto-calculado en base a los ítems pedidos asociados a esta OC.";
                            properties.Cancel = true;

                            return;
                        }
                    }
                    #endregion

                    #region Generar Codigo XBOL y actualizar descuentos
                    //TODO Cambiar
                    /*OJO tal vez se tenga que quitar. O ponerlo en la lista 'Órdenes de Compra Proveedor'
                    EjecutorOperacionesSP.ActualizarCodigoXbolYDescuentos(properties);
                    */
                    #endregion
               }
               #endregion

               #region Evento lista 'Envíos'
               if (properties.ListTitle == LISTA_ENVIOS)
               {
                   #region Validar ítems pedidos validos para envio
                   List<string> itemsNoValidos =
                       EjecutorOperacionesSP.RecuperarItemsPedidosNoValidosParaEnvio(properties);

                   //Validar si la urgencia del ítem pedido corresponde o no al tipo de envio definido
                   if (itemsNoValidos.Count != 0)
                   {
                       string linksItems = "";

                       foreach (string itemNoValido in itemsNoValidos)
                       {
                           linksItems += itemNoValido + "<br/>";
                       }

                       properties.Status = SPEventReceiverStatus.CancelWithError;
                       properties.ErrorMessage =
                           string.Format("Los ítems siguientes no pueden ser asociados al " +
                           "tipo de envío elegido:<br/>{0}<br/><br/>Vuelva a la página anterior " +
                           "y seleccione otro tipo de envío.", linksItems);
                       properties.Cancel = true;
                   }
                   #endregion
               }
               #endregion
           }
           catch (Exception ex)
           {
               #region Registro de Evento Error
               LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
               log.WriteEvent("--- [EVENTO] ItemUpdating ---");
               log.WriteException(ex);
               #endregion

               properties.Status = SPEventReceiverStatus.CancelWithError;
               properties.ErrorMessage = ex.Message;
               properties.Cancel = true;
           }
       }

       /// <summary>
       /// An item is being deleted.
       /// </summary>
       public override void ItemDeleting(SPItemEventProperties properties)
       {
           base.ItemDeleting(properties);
       }

       /// <summary>
       /// An item was added.
       /// </summary>
       public override void ItemAdded(SPItemEventProperties properties)
       {
           //base.ItemAdded(properties);
           try
           {
               #region Eventos de lista 'Ítems Pedidos'
               if (properties.ListTitle == LISTA_ITEMS_PEDIDOS)
               {
                   EjecutorOperacionesSP.ActualizarCamposOcultos(properties.ListItem);
                   EjecutorOperacionesSP.ActualizarCamposDePreciosTotales(properties);
                   /*try { EjecutorOperacionesSP.ActualizarBitacoraEstados(properties); }
                   catch { }*/
               }
               #endregion

               #region Eventos de lista 'Envíos'
               if (properties.ListTitle == LISTA_ENVIOS)
               {
                   EjecutorOperacionesSP.SincronizarAlmacenesAsociadosDeEnvio(properties);
                   EjecutorOperacionesSP.SincronizarFechaPrevistaLlegada(properties);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignado_Env(properties);
               }
               #endregion

               #region Eventos de lista 'Facturas'
               if (properties.ListTitle == LISTA_FACTURAS)
               {
                   EjecutorOperacionesSP.SincronizarItemsAsociadosDeFactura(properties);
                   EjecutorOperacionesSP.CalcularCampoTotalFactura(properties.ListItem);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignado_Fac(properties);
               }
               #endregion

               #region Eventos de lista 'Almacén'
               if (properties.ListTitle == LISTA_ALMACEN)
               {
                   EjecutorOperacionesSP.SincronizarFacturasAsociadasDeAlmacen(properties);
                   EjecutorOperacionesSP.CalcularCampoTotalFacturas(properties.ListItem);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignada_Alm(properties);
               }
               #endregion

               #region Eventos de lista Órdenes de Compra Proveedor
               if (properties.ListTitle == LISTA_OCP)
               {
                   EjecutorOperacionesSP.SincronizarItemsAsociadosDeOCP(properties);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignado_OCP(properties);
               }
               #endregion

               #region Eventos de lista Documentos Legales
               if (properties.ListTitle == LISTA_DOCUMENTOS_LEGALES)
               {
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asociada_DL(properties);
               }
               #endregion
           }
           catch (Exception ex)
           {
               #region Registro de Evento Error
               LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
               log.WriteEvent("--- [EVENTO] ItemAdded ---");
               log.WriteException(ex);
               #endregion
           }
       }

       /// <summary>
       /// An item was updated.
       /// </summary>
       public override void ItemUpdated(SPItemEventProperties properties)
       {
           //base.ItemUpdated(properties);
           try
           {
               #region Eventos de lista 'Ítems Pedidos'
               if (properties.ListTitle == LISTA_ITEMS_PEDIDOS)
               {
                   EjecutorOperacionesSP.ActualizarCamposOcultos(properties.ListItem);
                   EjecutorOperacionesSP.DeseleccionarItemAsociadoDeFacturas(properties);
                   EjecutorOperacionesSP.DeseleccionarItemOrdenadoDeOCPs(properties);
                   EjecutorOperacionesSP.ActualizarCamposDePreciosTotales(properties);
                   /*try { EjecutorOperacionesSP.ActualizarBitacoraEstados(properties); }
                   catch { }*/
               }
               #endregion

               #region Eventos de lista 'Órdenes de Compra Internas'
               if (properties.ListTitle == LISTA_OCI)
               {
                   EjecutorOperacionesSP.AprobarAutomaticamente(properties.ListItem);
                   EjecutorOperacionesSP.IniciarFlujoNotificarDescuentos(properties.ListItem);
                   /*try { EjecutorOperacionesSP.ActualizarBitacoraAprobacion(properties.ListItem); }
                   catch { }*/
               }
               #endregion

               #region Eventos de lista 'Envíos'
               if (properties.ListTitle == LISTA_ENVIOS)
               {
                   EjecutorOperacionesSP.SincronizarAlmacenesAsociadosDeEnvio(properties);
                   EjecutorOperacionesSP.SincronizarFechaPrevistaLlegada(properties);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignado_Env(properties);
               }
               #endregion

               #region Eventos de lista 'Facturas'
               if (properties.ListTitle == LISTA_FACTURAS)
               {
                   EjecutorOperacionesSP.SincronizarItemsAsociadosDeFactura(properties);
                   EjecutorOperacionesSP.DeseleccionarFacturaDeAlmacenes(properties);
                   EjecutorOperacionesSP.CalcularCampoTotalFactura(properties.ListItem);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignado_Fac(properties);
               }
               #endregion

               #region Eventos de lista 'Almacén'
               if (properties.ListTitle == LISTA_ALMACEN)
               {
                   EjecutorOperacionesSP.SincronizarFacturasAsociadasDeAlmacen(properties);
                   EjecutorOperacionesSP.DeseleccionarAlmacenDeEnvios(properties);
                   EjecutorOperacionesSP.CalcularCampoTotalFacturas(properties.ListItem);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignada_Alm(properties);
               }
               #endregion

               #region Eventos de Órdenes de Compra Proveedor
               if (properties.ListTitle == LISTA_OCP)
               {
                   EjecutorOperacionesSP.SincronizarItemsAsociadosDeOCP(properties);
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asignado_OCP(properties);
               }
               #endregion

               #region Eventos de lista Documentos Legales
               if (properties.ListTitle == LISTA_DOCUMENTOS_LEGALES)
               {
                   EjecutorOperacionesSP.CambiarCampoParaFiltro_Asociada_DL(properties);
               }
               #endregion
           }
           catch (Exception ex)
           {
               #region Registro de Evento Error
               LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
               log.WriteEvent("--- [EVENTO] ItemUpdated ---");
               log.WriteException(ex);
               #endregion
           }
       }

       /// <summary>
       /// An item was deleted.
       /// </summary>
       public override void ItemDeleted(SPItemEventProperties properties)
       {
           base.ItemDeleted(properties);
       }
    }
}
