using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Configuration;
using System.Globalization;
using Microsoft.SharePoint.Workflow;

namespace Abastecimiento
{
    public class EjecutorOperacionesSP
    {
        static string UrlAbastecimiento = ConfigurationManager.AppSettings["UrlAbastecimiento"];

        static string LISTA_OCI = "Órdenes de Compra Internas";
        static string LISTA_OCP = "Órdenes de Compra Proveedor";
        static string LISTA_ITEMS_PEDIDOS = "Ítems Pedidos";
        static string LISTA_ITEMS_CATALOGO = "Productos";
        static string LISTA_PROVEEDORES = "Proveedores";
        static string LISTA_DESCUENTOS = "Descuentos Proveedores";
        static string LISTA_ENVIOS = "Envíos";
        static string LISTA_FACTURAS = "Facturas";
        static string LISTA_ALMACEN = "Almacén (Warehouse)";
        static string LISTA_TIPOS_ENVIO = "Tipos de Envío";
        //static string LISTA_BITACORA = "Bitácora Pedidos";
        static string ESTADO_PEDIDO = "1"; //NO PROCESADO
        static string GRUPO_RESPONSABLES_SOL = "Responsables de Solicitud de OC";

        #region INSERCIONES
        /// <summary>
        /// Agrega un nuevo elemento a la lista 'Órdenes de Compra Internas'
        /// </summary>
        /// <param name="titulo"></param>
        /// <param name="fechaSolicitada"></param>
        /// <param name="observaciones"></param>
        /// <returns>ID del item creado</returns>
        public static int InsertarOC(string titulo, string fechaSolicitada, string observaciones)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPListItemCollection listItems = spw.Lists[LISTA_OCI].Items;
                    SPListItem itemOC = listItems.Add();

                    itemOC["Title"] = titulo;
                    itemOC["Fecha solicitada"] = Convert.ToDateTime(fechaSolicitada);
                    if (!string.IsNullOrEmpty(observaciones))
                        itemOC["Observaciones"] = observaciones;

                    itemOC.Update();

                    return itemOC.ID;
                }
            }
        }

        /// <summary>
        /// Agrega un nuevo elemento a la lista 'Ítems Pedidos'
        /// </summary>
        /// <param name="itemAsociado"></param>
        /// <param name="ocAsociada"></param>
        /// <param name="titulo"></param>
        /// <param name="cantidad"></param>
        /// <param name="precioU"></param>
        /// <param name="peso"></param>
        /// <param name="unidadM"></param>
        /// <param name="dims"></param>
        /// <param name="cliente"></param>
        /// <param name="tipoPedido"></param>
        /// <param name="clienteAsociado"></param>
        /// <returns>ID del item creado</returns>
        public static int InsertarItemPedido(string itemAsociado, string ocAsociada, string titulo,
            string cantidad, string precioU, string peso, string unidadM, string dims,
            string cliente, string tipoPedido, string clienteAsociado)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPListItemCollection listItems = spw.Lists[LISTA_ITEMS_PEDIDOS].Items;
                    SPListItem itemPedido = listItems.Add();

                    //itemPedido["Title"] = titulo;
                    itemPedido["Cantidad"] = cantidad;
                    itemPedido["Precio unitario"] = double.Parse(precioU, new CultureInfo("es-BO"));
                    //itemPedido["Moneda"] = moneda;
                    if (!string.IsNullOrEmpty(peso))
                        itemPedido["Peso"] = double.Parse(peso, new CultureInfo("es-BO"));
                    if (!string.IsNullOrEmpty(unidadM))
                        itemPedido["Unidad medida"] = unidadM;
                    itemPedido["Dimensiones"] = dims;
                    itemPedido["Cliente"] = cliente;
                    if (!string.IsNullOrEmpty(clienteAsociado))
                        itemPedido["Cliente asociado"] = clienteAsociado;
                    itemPedido["Tipo pedido"] = tipoPedido;
                    itemPedido["Ítem asociado"] = itemAsociado;
                    itemPedido["OCI asociada"] = ocAsociada;
                    itemPedido["Estado pedido"] = ESTADO_PEDIDO;
                    //itemPedido["Item_OC"] = string.Format("{0} ({1}) ({2:C})",
                    //    titulo, ocAsociada, (int.Parse(cantidad) * double.Parse(precioU)).ToString());

                    itemPedido.Update();
                    //using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    //{
                    //    itemPedido.Update();
                    //}

                    return itemPedido.ID;
                }
            }
        }

        /// <summary>
        /// Agrega un nuevo elemento a la lista 'Facturas' y 'Envíos'
        /// </summary>
        /// <param name="tipoEnvio"></param>
        /// <param name="idOrden"></param>
        public static void InsertarEnvioParaOC(string tipoEnvio, int idOrden)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {//Como usuario administrador
                using (SPSite sps = new SPSite(UrlAbastecimiento))
                {
                    using (SPWeb spw = sps.OpenWeb())
                    {
                        #region Insertar Factura
                        SPListItemCollection itemsPedidos = RecuperarItemsPedidosAsociados(idOrden, spw);
                        SPFieldLookupValueCollection itemsAsociados = new SPFieldLookupValueCollection();

                        foreach (SPListItem itemPedido in itemsPedidos)
                        {
                            itemsAsociados.Add(new SPFieldLookupValue(
                                itemPedido.ID, itemPedido["Item_OC"].ToString()));

                            #region El ítem pedido SI esta asignado a una Factura
                            itemPedido["Asignado_Fac"] = "SI";
                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {
                                itemPedido.SystemUpdate();
                            }
                            #endregion
                        }

                        SPListItemCollection itemsFacturas = spw.Lists[LISTA_FACTURAS].Items;
                        SPListItem itemFactura = itemsFacturas.Add();

                        itemFactura["Num. factura"] = "(Pre-factura OC " + idOrden + ")";
                        itemFactura["Fecha factura"] = DateTime.Today;
                        itemFactura["Ítems asociados"] = itemsAsociados;
                        itemFactura["Total factura"] =
                            spw.Lists[LISTA_OCI].GetItemById(idOrden)["Precio total"];
                        itemFactura["Asignada_Alm"] = "SI"; //La factura creada SI esta asignada a un Almacén

                        itemFactura.SystemUpdate();
                        #endregion

                        #region Insertar Almacén
                        SPFieldLookupValueCollection facturasAsociadas = new SPFieldLookupValueCollection();
                        facturasAsociadas.Add(new SPFieldLookupValue(itemFactura.ID, itemFactura.Title));

                        SPListItemCollection itemsAlmacen = spw.Lists[LISTA_ALMACEN].Items;
                        SPListItem itemAlmacen = itemsAlmacen.Add();

                        itemAlmacen["Title"] = "(Pre-almacén OC " + idOrden + ")";
                        itemAlmacen["Facturas asociadas"] = facturasAsociadas;
                        itemAlmacen["Total facturas"] = itemFactura["Total factura"];
                        itemAlmacen["Asignado_Env"] = "SI"; //El almacén creado SI esta asignado a un Envío

                        itemAlmacen.SystemUpdate();
                        #endregion

                        #region Insertar Envío
                        SPFieldLookupValueCollection almacenesAsociados = new SPFieldLookupValueCollection();
                        almacenesAsociados.Add(new SPFieldLookupValue(itemAlmacen.ID, itemAlmacen.Title));

                        SPListItemCollection itemsEnvios = spw.Lists[LISTA_ENVIOS].Items;
                        SPListItem itemEnvio = itemsEnvios.Add();

                        itemEnvio["Title"] = "(Pre-envío OC " + idOrden + ")";
                        //itemEnvio["Fecha prevista llegada"] = DateTime.Today.AddDays(15);
                        itemEnvio["Tipo envío"] = tipoEnvio;
                        itemEnvio["Almacenes asociados"] = almacenesAsociados;

                        using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                        {
                            itemEnvio.SystemUpdate();
                            
                            itemAlmacen["Envío asociado"] = itemEnvio.ID;
                            itemAlmacen.SystemUpdate();
                        }
                        #endregion
                    }
                }
            });
        }
        #endregion

        #region ACTUALIZACIONES
        /// <summary>
        /// Actualiza un elemento de la lista 'Órdenes de Compra Internas'
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="titulo"></param>
        /// <param name="fechaSolicitada"></param>
        /// <param name="observaciones"></param>
        /// <param name="precioTotal"></param>
        /// <param name="moneda"></param>
        /// <returns>ID del item actualizado. 0 en caso de no encontrar el item a actualizar</returns>
        public static int ActualizarOC(int itemId, string titulo, string fechaSolicitada,
            string observaciones, string precioTotal, string moneda, string sender)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    try
                    {
                        SPListItem itemOC = spw.Lists[LISTA_OCI].GetItemById(itemId);

                        itemOC["Title"] = titulo;
                        itemOC["Fecha solicitada"] = Convert.ToDateTime(fechaSolicitada);
                        //itemOC["Ítems Pedidos"] = RecuperarItemsPedidosAsociados(itemId, spw);

                        if (sender == "LinkButton")
                        {//Como administrador
                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {//Para evitar que ejecute una y otra vez el Evento ItemUpdating sobre este item
                                itemOC.SystemUpdate();
                            }
                        }
                        else
                        {//Como usuario actual
                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {//Para evitar que ejecute una y otra vez el Evento ItemUpdating sobre este item
                                itemOC["Observaciones"] = observaciones;
                                itemOC["Precio total"] = double.Parse(precioTotal, new CultureInfo("es-BO"));
                                itemOC["Moneda"] = moneda;

                                itemOC.Update();
                            }

                            #region Inicio flujo
                            if (precioTotal != "0")
                                IniciarFlujoNotificarAprobacion(itemOC);
                            #endregion
                        }

                        return itemOC.ID;
                    }
                    catch (Exception ex)
                    {//Si el item no existe retorna 0
                        #region Registro de Evento Error
                        LogEventos.LogArchivo log = new LogEventos.LogArchivo("LogErrores.txt");
                        log.WriteEvent("--- [ActualizarOC] Actualiza un elemento de la lista 'Órdenes de Compra Internas' ---");
                        log.WriteException(ex);
                        #endregion

                        return 0;
                    }
                }
            }
        }
        
        /// <summary>
        /// Actualiza un elemento de la lista 'Órdenes de Compra Internas' (todos sus campos)
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="titulo"></param>
        /// <param name="fechaSolicitada"></param>
        /// <param name="observaciones"></param>
        /// <param name="precioTotal"></param>
        /// <param name="moneda"></param>
        /// <param name="proveedor"></param>
        /// <returns>ID del item actualizado. 0 en caso de no encontrar el item a actualizar</returns>
        /*public static int ActualizarOC(int itemId, string titulo, string fechaSolicitada,
            string observaciones, string precioTotal, string moneda, string proveedor)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    try
                    {
                        SPListItem itemOC = spw.Lists[LISTA_OC].GetItemByIdAllFields(itemId);

                        itemOC["Title"] = titulo;
                        itemOC["Fecha solicitada"] = Convert.ToDateTime(fechaSolicitada);
                        if (!string.IsNullOrEmpty(observaciones))
                            itemOC["Observaciones"] = observaciones;
                        itemOC["Precio total"] = double.Parse(precioTotal, new CultureInfo("es-BO"));
                        if (!string.IsNullOrEmpty(moneda))
                            itemOC["Moneda"] = moneda;
                        //itemOC["Ítems Pedidos"] = RecuperarItemsPedidosAsociados(itemId, spw);
                        if (!string.IsNullOrEmpty(proveedor))
                            itemOC["Proveedor asociado"] = proveedor;

                        itemOC.Update();

                        return itemOC.ID;
                    }
                    catch
                    {//Si el item no existe retorna 0
                        return 0;
                    }
                }
            }
        }*/

        /// <summary>
        /// Actualiza un elemento de la lista 'Ítems Pedidos'
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="cantidad"></param>
        /// <param name="precioU"></param>
        /// <param name="peso"></param>
        /// <param name="unidadM"></param>
        /// <param name="dims"></param>
        /// <param name="cliente"></param>
        /// <param name="tipoPedido"></param>
        /// <param name="clienteAsociado"></param>
        public static void ActualizarItemPedido(int itemId, string cantidad, string precioU,
            string peso, string unidadM, string dims, string cliente, string tipoPedido, string clienteAsociado)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPQuery consulta = new SPQuery();
                    consulta.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                        "<Value Type='Counter'>" + itemId + "</Value></Eq></Where>";

                    SPListItemCollection listItems = spw.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consulta);
                    SPListItem itemPedido = listItems.GetItemById(itemId);

                    itemPedido["Cantidad"] = cantidad;
                    itemPedido["Precio unitario"] = double.Parse(precioU, new CultureInfo("es-BO"));
                    //itemPedido["Moneda"] = moneda;
                    if (!string.IsNullOrEmpty(peso))
                        itemPedido["Peso"] = double.Parse(peso, new CultureInfo("es-BO"));
                    if (!string.IsNullOrEmpty(unidadM))
                        itemPedido["Unidad medida"] = unidadM;
                    itemPedido["Dimensiones"] = dims;
                    itemPedido["Cliente"] = cliente;
                    if (!string.IsNullOrEmpty(clienteAsociado))
                        itemPedido["Cliente asociado"] = clienteAsociado;
                    itemPedido["Tipo pedido"] = tipoPedido;

                    itemPedido.Update();
                    //using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    //{
                    //    itemPedido.Update();
                    //}
                }
            }
        }

        /// <summary>
        /// Actualiza los campos 'Código XBOL' y 'Código prov.' de las listas 'Órdenes de Compra Internas' y 'Proveedores'.
        /// Tambien actualiza los descuentos asociados que pudieran existir para el proveedor elegido.
        /// </summary>
        /// <param name="properties">Evento ItemUpdating generado sobre la lista 'Órdenes de Compra Internas'</param>
        public static void ActualizarCodigoXbolYDescuentos(SPItemEventProperties properties)
        {
            SPListItem itemOCAntes = properties.ListItem;

            string proveedorAntes = "";
            string proveedorDespues = "";
            if (itemOCAntes["Proveedor_x0020_asociado"] != null)
                proveedorAntes = itemOCAntes["Proveedor_x0020_asociado"].ToString().Remove(
                    itemOCAntes["Proveedor_x0020_asociado"].ToString().IndexOf(';'));
            if (properties.AfterProperties["Proveedor_x0020_asociado"] != null)
                proveedorDespues = properties.AfterProperties["Proveedor_x0020_asociado"].ToString();

            if (proveedorAntes != proveedorDespues)
            {
                #region Actualizar descuentos
                string proveedorElegido =
                    properties.AfterProperties["Proveedor_x0020_asociado"].ToString();

                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='Proveedor_x0020_asociado'/>" +
                    "<Value Type='Text'>" +
                    properties.Web.Lists[LISTA_PROVEEDORES].GetItemById(Convert.ToInt32(proveedorElegido)).Title +
                    "</Value></Eq></Where>";
                SPListItemCollection itemsDescuentos = properties.Web.Lists[LISTA_DESCUENTOS].GetItems(query);

                foreach (SPListItem itemDescuento in itemsDescuentos)
                {
                    DateTime fechaActual = DateTime.Now;
                    DateTime fechaInicio = new DateTime(1900, 1, 1);
                    if (itemDescuento["Fecha inicio"] != null)
                        fechaInicio = (DateTime)itemDescuento["Fecha inicio"];
                    DateTime fechaFin = new DateTime(1900, 1, 1);
                    if (itemDescuento["Fecha fin"] != null)
                        fechaFin = (DateTime)itemDescuento["Fecha fin"];

                    if ((itemDescuento["Fecha inicio"] == null && itemDescuento["Fecha fin"] == null) ||
                        fechaActual >= fechaInicio && fechaActual <= fechaFin)
                    {
                        int cantidadDescuento = 0;
                        if (itemDescuento["Cantidad desc."] != null)
                            cantidadDescuento = Convert.ToInt32(itemDescuento["Cantidad desc."]);

                        if (cantidadDescuento != 0)
                        {
                            SPListItemCollection itemsPedidos =
                                RecuperarItemsPedidosAsociados(itemOCAntes.ID, properties.Web);
                            foreach (SPListItem itemPedido in itemsPedidos)
                            {
                                if (itemPedido["Ítem asociado"].ToString() == itemDescuento["Ítem asociado"].ToString())
                                {
                                    int cantidadPedido = Convert.ToInt32(itemPedido["Cantidad"]);
                                    int cantidadActualizada = cantidadDescuento - cantidadPedido;
                                    if (cantidadActualizada < 0)
                                        cantidadActualizada = 0;

                                    itemDescuento["Cantidad desc."] = cantidadActualizada;
                                    itemDescuento.Update();
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Actualiza los campos 'Precio total' (Órdenes de Compra Internas), 'Total factura' (Facturas), 
        /// 'Total facturas' (Envíos) cada vez que se actualiza un ítem pedido.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="resta"></param>
        public static void ActualizarCamposDePreciosTotales(SPItemEventProperties properties)
        {
            double precioTotal = 0; //OC
            string ocAsociada = SubcadenaAntes(properties.ListItem["OCI asociada"]);
            
            SPQuery consultaOc = new SPQuery();
            consultaOc.Query = "<Where><Eq><FieldRef Name='OC_x0020_asociada_x003a_ID' />" +
                "<Value Type='Text'>" + ocAsociada + "</Value></Eq></Where>";
            SPListItemCollection itemsPedidosOc = properties.List.GetItems(consultaOc);

            foreach (SPListItem itemPedido in itemsPedidosOc)
            {
                //"float;#957.600000000000"
                precioTotal = precioTotal +
                    double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ','));
            }

            #region Actualizar 'Precio total'
            SPListItem itemOC =
                properties.Web.Lists[LISTA_OCI].GetItemById(Convert.ToInt32(ocAsociada));

            itemOC["Precio total"] = precioTotal;
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                itemOC.SystemUpdate();
            }
            #endregion

            if (properties.ListItem["Factura asociada"] != null)
            {
                double totalFactura = 0; //Factura
                string facturaAsociada = SubcadenaAntes(properties.ListItem["Factura asociada"]);

                SPQuery consultaFct = new SPQuery();
                consultaFct.Query = "<Where><Eq><FieldRef Name='Factura_x0020_asociada_x003a_ID' />" +
                    "<Value Type='Text'>" + facturaAsociada + "</Value></Eq></Where>";
                SPListItemCollection itemsPedidosFct = properties.List.GetItems(consultaFct);

                foreach (SPListItem itemPedido in itemsPedidosFct)
                {
                    //"float;#957.600000000000"
                    totalFactura = totalFactura +
                        double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ','));
                }

                #region Actualizar 'Total factura' en Facturas
                SPListItem itemFactura =
                    properties.Web.Lists[LISTA_FACTURAS].GetItemById(
                    Convert.ToInt32(SubcadenaAntes(properties.ListItem["Factura asociada"])));

                itemFactura["Total factura"] = totalFactura;
                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                {
                    itemFactura.SystemUpdate();
                }
                #endregion

                if (itemFactura["Almacén asociado"] != null)
                {
                    double totalFacturas = 0; //Almacén
                    string envioAsociado = SubcadenaAntes(itemFactura["Almacén asociado"]);

                    SPQuery consultaEnv = new SPQuery();
                    consultaEnv.Query = "<Where><Eq><FieldRef Name='Almac_x00e9_n_x0020_asociado_x00' />" +
                        "<Value Type='Text'>" + envioAsociado + "</Value></Eq></Where>";
                    SPListItemCollection lasFacturas = itemFactura.ParentList.GetItems(consultaEnv);

                    foreach (SPListItem laFactura in lasFacturas)
                    {
                        totalFacturas = totalFacturas +
                            double.Parse(laFactura["Total factura"].ToString());
                    }

                    #region Actualizar 'Total facturas' en Almacén
                    SPListItem itemAlmacen =
                        properties.Web.Lists[LISTA_ALMACEN].GetItemById(
                        Convert.ToInt32(SubcadenaAntes(itemFactura["Almacén asociado"])));

                    itemAlmacen["Total facturas"] = totalFacturas;
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        itemAlmacen.SystemUpdate();
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Actualiza el valor de los campos ocultos 'Item_OC' y 'Título' en la lista 'Ítems Pedidos'
        /// </summary>
        /// <param name="item"></param>
        public static void ActualizarCamposOcultos(SPListItem item)
        {
            SPListItem itemPedido = item;
            string ocpTitulo = "";

            if (itemPedido["Title"] == null)
                itemPedido["Title"] = SubcadenaDespues(itemPedido["Ítem asociado"]);
            if (itemPedido["OCP asociada"] != null)
                ocpTitulo = SubcadenaDespues(itemPedido["OCP asociada"]);
            
            itemPedido["Item_OC"] =
                string.Format("{0} ({1}) ({2:$#,0.00})", itemPedido.Title,
                ocpTitulo,
                double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ',')));

            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                itemPedido.SystemUpdate();
            }
        }

        /// <summary>
        /// Aprueba automaticamente el item entregado (siempre y cuando se cumplan sus condiciones)
        /// </summary>
        /// <param name="itemOC"></param>
        public static void AprobarAutomaticamente(SPListItem itemOC)
        {
            if (itemOC.Web.Groups[GRUPO_RESPONSABLES_SOL].ContainsCurrentUser)
            {
                itemOC.ModerationInformation.Status = SPModerationStatusType.Approved;

                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                {
                    itemOC.SystemUpdate();
                }
            }
        }

        /// <summary>
        /// Modifica el campo 'Fecha prevista llegada' de la lista 'Ítems Pedidos' con el campo
        /// del mismo nombre de la lista 'Envíos'
        /// </summary>
        /// <param name="properties"></param>
        public static void SincronizarFechaPrevistaLlegada(SPItemEventProperties properties)
        {//ítem de lista Envíos
            DateTime fechaPrevista = (DateTime)properties.ListItem["Fecha prevista llegada"];
            SPList listaFacturas = properties.Web.Lists[LISTA_FACTURAS];
            SPList listaAlmacen = properties.Web.Lists[LISTA_ALMACEN];
            SPList listaItemsPedidos = properties.Web.Lists[LISTA_ITEMS_PEDIDOS];

            SPFieldLookupValueCollection almacenesAsociados =
                (SPFieldLookupValueCollection)properties.ListItem["Almacenes asociados"];

            //Recorre los Almacenes asociados al Envío
            foreach (SPFieldLookupValue almacenAsociado in almacenesAsociados)
            {
                SPQuery consulta1 = new SPQuery();
                consulta1.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                    "<Value Type='Counter'>" + almacenAsociado.LookupId + "</Value></Eq></Where>";
                SPListItemCollection losAlmacenes = listaAlmacen.GetItems(consulta1);
                
                foreach (SPListItem elAlmacen in losAlmacenes)
                {
                    SPFieldLookupValueCollection facturasAsociadas =
                        (SPFieldLookupValueCollection)elAlmacen["Facturas asociadas"];
                    //Recorre las Facturas asociadas al Almacén
                    foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
                    {
                        SPQuery consulta2 = new SPQuery();
                        consulta2.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                            "<Value Type='Counter'>" + facturaAsociada.LookupId + "</Value></Eq></Where>";
                        SPListItemCollection lasFacturas = listaFacturas.GetItems(consulta2);

                        foreach (SPListItem laFactura in lasFacturas)
                        {
                            SPFieldLookupValueCollection itemsAsociados =
                                (SPFieldLookupValueCollection)laFactura["Ítems asociados"];
                            //Recorre los Ítems Pedidos asociados a la Factura
                            foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
                            {
                                SPListItem itemPedido = listaItemsPedidos.GetItemByIdSelectedFields(
                                    itemAsociado.LookupId, "Fecha_x0020_prevista_x0020_llega");

                                itemPedido["Fecha prevista llegada"] = fechaPrevista;
                                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                                {
                                    itemPedido.Update();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Modifica el campo 'Factura asociada' de la lista 'Ítems Pedidos' en base
        /// a la seleccion de 'Ítems asociados' en la factura.
        /// </summary>
        /// <param name="properties"></param>
        public static void SincronizarItemsAsociadosDeFactura(SPItemEventProperties properties)
        {//ítem de lista Facturas
            SPListItem itemFactura = properties.ListItem;
            SPFieldLookupValueCollection itemsAsociados =
                (SPFieldLookupValueCollection)itemFactura["Ítems asociados"];

            foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
            {
                SPListItem itemPedido =
                    properties.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItemByIdSelectedFields(
                    itemAsociado.LookupId, "Factura_x0020_asociada");

                itemPedido["Factura asociada"] = itemFactura.ID;
                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                {
                    itemPedido.Update();
                }
            }
        }

        /// <summary>
        /// Modifica el campo 'Envío asociado' de la lista 'Almacén' en base
        /// a la seleccion de 'Almacenes asociados' en el envío.
        /// </summary>
        /// <param name="properties"></param>
        public static void SincronizarAlmacenesAsociadosDeEnvio(SPItemEventProperties properties)
        {//ítem de lista Envíos
            SPListItem itemEnvio = properties.ListItem;
            SPFieldLookupValueCollection almacenesAsociados =
                (SPFieldLookupValueCollection)itemEnvio["Almacenes asociados"];

            //Recorre los Almacenes asociados al Envío
            foreach (SPFieldLookupValue almacenAsociado in almacenesAsociados)
            {
                SPListItem itemAlmacen =
                    properties.Web.Lists[LISTA_ALMACEN].GetItemByIdSelectedFields(
                    almacenAsociado.LookupId, "Env_x00ed_o_x0020_asociado");

                itemAlmacen["Envío asociado"] = itemEnvio.ID;
                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                {
                    itemAlmacen.Update();
                }
            }
        }

        /// <summary>
        /// Modifica el campo 'Almacén asociado' de la lista 'Facturas' en base
        /// a la seleccion de 'Facturas asociadas' en el almacén.
        /// </summary>
        /// <param name="properties"></param>
        public static void SincronizarFacturasAsociadasDeAlmacen(SPItemEventProperties properties)
        {//ítem de lista Almacén
            SPListItem itemAlmacen = properties.ListItem;
            SPFieldLookupValueCollection facturasAsociadas =
                (SPFieldLookupValueCollection)itemAlmacen["Facturas asociadas"];

            //Recorre los Almacenes asociados al Envío
            foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
            {
                SPListItem itemfactura =
                    properties.Web.Lists[LISTA_FACTURAS].GetItemByIdSelectedFields(
                    facturaAsociada.LookupId, "Almac_x00e9_n_x0020_asociado");

                itemfactura["Almacén asociado"] = itemAlmacen.ID;
                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                {
                    itemfactura.Update();
                }
            }
        }

        /// <summary>
        /// Modifica el campo 'OCP asociada' de la lista 'Ítems Pedidos' en base
        /// a la seleccion de 'Ítems ordenados' en la ocp.
        /// </summary>
        /// <param name="properties"></param>
        public static void SincronizarItemsAsociadosDeOCP(SPItemEventProperties properties)
        {//ítem de lista Órdenes de Compra Proveedor
            SPListItem itemOCP = properties.ListItem;
            SPFieldLookupValueCollection itemsOrdenados =
                (SPFieldLookupValueCollection)itemOCP["Ítems ordenados"];

            foreach (SPFieldLookupValue itemOrdenado in itemsOrdenados)
            {
                SPListItem itemPedido =
                    properties.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItemByIdSelectedFields(
                    itemOrdenado.LookupId, "OCP_x0020_asociada");

                itemPedido["OCP asociada"] = itemOCP.ID;
                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                {
                    itemPedido.Update();
                }

                ActualizarCamposOcultos(itemPedido);
            }
        }

        /// <summary>
        /// Actualiza el campo 'Total factura' de la lista 'Facturas' con la informacion
        /// del campo 'Precio extendido' de los ítems asociados elegidos.
        /// </summary>
        /// <param name="properties"></param>
        public static void CalcularCampoTotalFactura(SPListItem itemFactura)
        {//ítem de lista Facturas
            double totalFactura = 0;
            SPFieldLookupValueCollection itemsAsociados =
                (SPFieldLookupValueCollection)itemFactura["Ítems asociados"];

            foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
            {
                SPListItem itemPedido =
                    itemFactura.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItemByIdSelectedFields(
                    itemAsociado.LookupId, "Precio_x0020_extendido");
                totalFactura = totalFactura +
                    double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ','));
            }

            itemFactura["Total factura"] = totalFactura;
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                itemFactura.SystemUpdate();
            }
        }

        /// <summary>
        /// Actualiza el campo 'Total facturas' de la lista Almacén con la informacion
        /// del campo 'Total factura' de las facturas asociadas elegidas.
        /// </summary>
        /// <param name="itemAlmacen"></param>
        public static void CalcularCampoTotalFacturas(SPListItem itemAlmacen)
        {//ítem de lista Almacén
            double totalFacturas = 0;
            SPFieldLookupValueCollection facturasAsociadas =
                (SPFieldLookupValueCollection)itemAlmacen["Facturas asociadas"];

            foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
            {
                double totalFactura = double.Parse(
                    itemAlmacen.Web.Lists[LISTA_FACTURAS].GetItemByIdSelectedFields(
                    facturaAsociada.LookupId, "Total_x0020_factura")["Total factura"].ToString());
                totalFacturas = totalFacturas + totalFactura;
            }

            itemAlmacen["Total facturas"] = totalFacturas;
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                itemAlmacen.SystemUpdate();
            }
        }

        /// <summary>
        /// Cambia el valor del campo 'Asignado_OCP' de la lista Ítems Pedidos
        /// una vez que éste es asignado a una Órden de Compra Proveedor.
        /// (ItemAdded, ItemUpdated)
        /// </summary>
        /// <param name="properties"></param>
        public static void CambiarCampoParaFiltro_Asignado_OCP(SPItemEventProperties properties)
        {//ítem de lista Órdenes de Compra Proveedor
            try
            {
                SPListItem itemOCP = properties.ListItem;
                SPFieldLookupValueCollection itemsOrdenados =
                    (SPFieldLookupValueCollection)itemOCP["Ítems ordenados"];

                foreach (SPFieldLookupValue itemOrdenado in itemsOrdenados)
                {
                    SPListItem itemPedido =
                        properties.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItemByIdSelectedFields(
                        itemOrdenado.LookupId, "Asignado_OCP", "Estado_x0020_pedido");

                    itemPedido["Asignado_OCP"] = "SI";
                    itemPedido["Estado pedido"] = "2"; //PROCESADO
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        itemPedido.SystemUpdate();
                    }
                }
            }
            catch { }//Si el campo 'Ítems ordenados' es vacio, continua.
        }

        /// <summary>
        /// Cambia el valor del campo 'Asignado_Fac' de la lista Ítems Pedidos
        /// una vez que éste es asignado a una Factura.
        /// (ItemAdded, ItemUpdated)
        /// </summary>
        /// <param name="properties"></param>
        public static void CambiarCampoParaFiltro_Asignado_Fac(SPItemEventProperties properties)
        {//ítem de lista Facturas
            try
            {
                SPListItem itemFacturas = properties.ListItem;
                SPFieldLookupValueCollection itemsAsociados =
                    (SPFieldLookupValueCollection)itemFacturas["Ítems asociados"];

                foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
                {
                    SPListItem itemPedido =
                        properties.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItemByIdSelectedFields(
                        itemAsociado.LookupId, "Asignado_Fac");

                    itemPedido["Asignado_Fac"] = "SI";
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        itemPedido.SystemUpdate();
                    }
                }
            }
            catch { }//Si el campo 'Ítems asociados' es vacio, continua.
        }

        /// <summary>
        /// Cambia el valor del campo 'Asignada_Alm' de la lista Facturas
        /// una vez que éste es asignado a un Almacén.
        /// (ItemAdded, ItemUpdated)
        /// </summary>
        /// <param name="properties"></param>
        public static void CambiarCampoParaFiltro_Asignada_Alm(SPItemEventProperties properties)
        {//ítem de lista Almacén
            try
            {
                SPListItem itemAlmacen = properties.ListItem;
                SPFieldLookupValueCollection facturasAsociadas =
                    (SPFieldLookupValueCollection)itemAlmacen["Facturas asociadas"];

                foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
                {
                    SPListItem laFactura =
                        properties.Web.Lists[LISTA_FACTURAS].GetItemByIdSelectedFields(
                        facturaAsociada.LookupId, "Asignada_Alm");

                    laFactura["Asignada_Alm"] = "SI";
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        laFactura.SystemUpdate();
                    }
                }
            }
            catch { }//Si el campo 'Facturas asociadas' es vacio, continua.
        }

        /// <summary>
        /// Cambia el valor del campo 'Asignado_Env' de la lista Almacén
        /// una vez que éste es asignado a un Envío.
        /// (ItemAdded, ItemUpdated)
        /// </summary>
        /// <param name="properties"></param>
        public static void CambiarCampoParaFiltro_Asignado_Env(SPItemEventProperties properties)
        {//ítem de lista Envíos
            try
            {
                SPListItem itemEnvio = properties.ListItem;
                SPFieldLookupValueCollection almacenesAsociados =
                    (SPFieldLookupValueCollection)itemEnvio["Almacenes asociados"];

                foreach (SPFieldLookupValue almacenAsociado in almacenesAsociados)
                {
                    SPListItem elAlmacen =
                        properties.Web.Lists[LISTA_ALMACEN].GetItemByIdSelectedFields(
                        almacenAsociado.LookupId, "Asignado_Env");

                    elAlmacen["Asignado_Env"] = "SI";
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        elAlmacen.SystemUpdate();
                    }
                }
            }
            catch { }//Si el campo 'Almacenes asociados' es vacio, continua.
        }

        /// <summary>
        /// Cambia el valor del campo 'Asociada_DL' de la lista Órdenes de Compra Proveedor
        /// una vez que éste es asignado a un Documento Legal.
        /// (ItemAdded, ItemUpdated)
        /// </summary>
        /// <param name="properties"></param>
        public static void CambiarCampoParaFiltro_Asociada_DL(SPItemEventProperties properties)
        {//ítem de lista Documentos Legales
            try
            {
                SPListItem itemDocumento = properties.ListItem;
                SPFieldLookupValueCollection ocAsociadas =
                    (SPFieldLookupValueCollection)itemDocumento["OC asociadas"];

                foreach (SPFieldLookupValue ocAsociada in ocAsociadas)
                {
                    SPListItem laOC =
                        properties.Web.Lists[LISTA_OCP].GetItemByIdSelectedFields(
                        ocAsociada.LookupId, "Asociada_DL");

                    laOC["Asociada_DL"] = "SI";
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        laOC.SystemUpdate();
                    }
                }
            }
            catch { }//Si el campo 'OC asociadas' es vacio, continua.
        }

        /// <summary>
        /// Remueve el ítem pedido indicado de la columna 'Ítems asociados' de la lista Facturas.
        /// La exepcion a esto es cuando dicha columna solo tiene un único ítem asociado.
        /// </summary>
        /// <param name="properties"></param>
        public static void DeseleccionarItemAsociadoDeFacturas(SPItemEventProperties properties)
        {//ítem de lista Ítems Pedidos
            try
            {
                SPListItem itemPedido = properties.ListItem;

                if (itemPedido["Asignado_Fac"].ToString() == "NO" &&
                    itemPedido["Factura asociada"] != null)
                {
                    SPListItem laFactura =
                        properties.Web.Lists[LISTA_FACTURAS].GetItemByIdSelectedFields(
                        Convert.ToInt32(SubcadenaAntes(itemPedido["Factura asociada"])),
                        "_x00cd_tems_x0020_asociados");
                    SPFieldLookupValueCollection itemsAsociados =
                        (SPFieldLookupValueCollection)laFactura["Ítems asociados"];

                    foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
                    {
                        if (itemAsociado.LookupId == itemPedido.ID)
                        {
                            itemsAsociados.Remove(itemAsociado);
                            laFactura["Ítems asociados"] = itemsAsociados;
                            itemPedido["Factura asociada"] = string.Empty;
                            
                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {//En caso remover el unico elemento, la factura no se actualiza
                                itemPedido.SystemUpdate();
                                laFactura.Update();
                            }

                            break;
                        }
                    }
                }
            }
            catch { } //En caso de error, continua con el siguiente evento de lista
        }

        /// <summary>
        /// Remueve el ítem pedido indicado de la columna 'Ítems ordenados' de la lista Órdenes de Compra Proveedor.
        /// La exepcion a esto es cuando dicha columna solo tiene un único ítem ordenado.
        /// </summary>
        /// <param name="properties"></param>
        public static void DeseleccionarItemOrdenadoDeOCPs(SPItemEventProperties properties)
        {//ítem de lista Ítems Pedidos
            try
            {
                SPListItem itemPedido = properties.ListItem;

                if (itemPedido["Asignado_OCP"].ToString() == "NO" &&
                    itemPedido["OCP asociada"] != null)
                {
                    SPListItem laOCP =
                        properties.Web.Lists[LISTA_OCP].GetItemByIdSelectedFields(
                        Convert.ToInt32(SubcadenaAntes(itemPedido["OCP asociada"])),
                        "_x00cd_tems_x0020_ordenados");
                    SPFieldLookupValueCollection itemsOrdenados =
                        (SPFieldLookupValueCollection)laOCP["Ítems ordenados"];

                    foreach (SPFieldLookupValue itemOrdenado in itemsOrdenados)
                    {
                        if (itemOrdenado.LookupId == itemPedido.ID)
                        {
                            itemsOrdenados.Remove(itemOrdenado);
                            laOCP["Ítems ordenados"] = itemsOrdenados;
                            itemPedido["OCP asociada"] = string.Empty;

                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {//En caso remover el unico elemento, la OCP no se actualiza
                                itemPedido.SystemUpdate();
                                laOCP.Update();
                            }

                            break;
                        }
                    }
                }
            }
            catch { } //En caso de error, continua con el siguiente evento de lista
        }

        /// <summary>
        /// Remueve la factura indicada de la columna 'Facturas asociadas' de la lista Almacén.
        /// La exepcion a esto es cuando dicha columna solo tiene una única factura.
        /// </summary>
        /// <param name="properties"></param>
        public static void DeseleccionarFacturaDeAlmacenes(SPItemEventProperties properties)
        {//ítem de lista Facturas
            try
            {
                SPListItem itemFactura = properties.ListItem;

                if (itemFactura["Asignada_Alm"].ToString() == "NO" &&
                    itemFactura["Almacén asociado"] != null)
                {
                    SPListItem elAlmacen =
                        properties.Web.Lists[LISTA_ALMACEN].GetItemByIdSelectedFields(
                        Convert.ToInt32(SubcadenaAntes(itemFactura["Almacén asociado"])),
                        "Facturas_x0020_asociadas");
                    SPFieldLookupValueCollection facturasAsociadas =
                        (SPFieldLookupValueCollection)elAlmacen["Facturas asociadas"];

                    foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
                    {
                        if (facturaAsociada.LookupId == itemFactura.ID)
                        {
                            facturasAsociadas.Remove(facturaAsociada);
                            elAlmacen["Facturas asociadas"] = facturasAsociadas;
                            itemFactura["Almacén asociado"] = string.Empty;

                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {//En caso remover el unico elemento, el almacén no se actualiza
                                itemFactura.SystemUpdate();
                                elAlmacen.Update();
                            }

                            break;
                        }
                    }
                }
            }
            catch { } //En caso de error, continua con el siguiente evento de lista
        }

        /// <summary>
        /// Remueve el almacen indicado de la columna 'Almacenes asociados' de la lista Envíos.
        /// La exepcion a esto es cuando dicha columna solo tiene un único almacén.
        /// </summary>
        /// <param name="properties"></param>
        public static void DeseleccionarAlmacenDeEnvios(SPItemEventProperties properties)
        {//ítem de lista Almacén
            try
            {
                SPListItem itemAlmacen = properties.ListItem;

                if (itemAlmacen["Asignado_Env"].ToString() == "NO" &&
                    itemAlmacen["Envío asociado"] != null)
                {
                    SPListItem elEnvio =
                        properties.Web.Lists[LISTA_ENVIOS].GetItemByIdSelectedFields(
                        Convert.ToInt32(SubcadenaAntes(itemAlmacen["Envío asociado"])),
                        "Almacenes_x0020_asociados");
                    SPFieldLookupValueCollection almacenesAsociados =
                        (SPFieldLookupValueCollection)elEnvio["Almacenes asociados"];

                    foreach (SPFieldLookupValue almacenAsociado in almacenesAsociados)
                    {
                        if (almacenAsociado.LookupId == itemAlmacen.ID)
                        {
                            almacenesAsociados.Remove(almacenAsociado);
                            elEnvio["Facturas asociadas"] = almacenesAsociados;
                            itemAlmacen["Envío asociado"] = string.Empty;

                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {//En caso remover el unico elemento, el envío no se actualiza
                                itemAlmacen.SystemUpdate();
                                elEnvio.Update();
                            }

                            break;
                        }
                    }
                }
            }
            catch { } //En caso de error, continua con el siguiente evento de lista
        }

        /// <summary>
        /// Actualiza los campos de la lista 'Bitácora Pedidos', excepto el campo 'Aprobado'
        /// </summary>
        /// <param name="properties"></param>
        /*public static void ActualizarBitacoraEstados(SPItemEventProperties properties)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {//Como usuario administrador
                using (SPSite sps = new SPSite(UrlAbastecimiento))
                {
                    using (SPWeb spw = sps.OpenWeb())
                    {
                        SPListItem itemPedido = properties.ListItem;

                        SPQuery consultaBit = new SPQuery();
                        consultaBit.Query = "<Where><Eq><FieldRef Name='Item_x0020_pedido_x003A_ID0'/>" +
                            "<Value Type='Text'>" + itemPedido.ID + "</Value></Eq></Where>";
                        SPListItemCollection colItemsBitacora =
                            spw.Lists[LISTA_BITACORA].GetItems(consultaBit);

                        if (colItemsBitacora.Count == 0)
                        {//No existe
                            #region Crear nueva entrada en bitacora
                            DateTime fechaHoy = DateTime.Now;
                            SPListItem itemBitacora = colItemsBitacora.Add();

                            itemBitacora["Title"] = itemPedido.ID;
                            itemBitacora["Item_x0020_pedido0"] = itemPedido.ID;
                            itemBitacora["OCI asociada"] = itemPedido["OCI asociada"];
                            itemBitacora["Solicitado"] = itemPedido["Creado"];

                            int idOC = Convert.ToInt32(SubcadenaAntes(itemPedido["OCI asociada"]));
                            SPListItem itemOC = spw.Lists[LISTA_OC].GetItemById(idOC);
                            if (itemOC.ModerationInformation != null &&
                                itemOC.ModerationInformation.Status == SPModerationStatusType.Approved)
                                itemBitacora["Aprobado"] = fechaHoy;

                            if (SubcadenaDespues(itemPedido["Estado pedido"]) == "PROCESADO")
                                itemBitacora["Procesado"] = fechaHoy;

                            if (SubcadenaDespues(itemPedido["Estado pedido"]) == "CONFIRMADO")
                                itemBitacora["Confirmado"] = fechaHoy;

                            if (SubcadenaDespues(itemPedido["Estado pedido"]) == "ABASTECIDO")
                                itemBitacora["Abastecido"] = fechaHoy;

                            if (SubcadenaDespues(itemPedido["Estado pedido"]) == "FACTURADO")
                                itemBitacora["Facturado"] = fechaHoy;

                            if (SubcadenaDespues(itemPedido["Estado pedido"]) == "TRANSITO ADUANA")
                                itemBitacora["Transito adn."] = fechaHoy;

                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {
                                itemBitacora.SystemUpdate();
                            }

                            #endregion
                        }
                        else
                        {
                            #region Actualizar entrada existente en bitacora
                            SPListItem itemBitacora = colItemsBitacora[0]; //el primer y unico item de la coleccion
                            string estadoPedido = SubcadenaDespues(itemPedido["Estado pedido"]);
                            DateTime fechaHoy = DateTime.Now;

                            itemBitacora["OCI asociada"] = itemPedido["OCI asociada"];

                            switch (estadoPedido.Trim())
                            {
                                case "PROCESADO":
                                    if (itemBitacora["Procesado"] == null && itemBitacora["Aprobado"] != null)
                                        itemBitacora["Procesado"] = fechaHoy;
                                    break;
                                case "CONFIRMADO":
                                    if (itemBitacora["Confirmado"] == null && itemBitacora["Procesado"] != null)
                                        itemBitacora["Confirmado"] = fechaHoy;
                                    break;
                                case "ABASTECIDO":
                                    if (itemBitacora["Abastecido"] == null && itemBitacora["Confirmado"] != null)
                                        itemBitacora["Abastecido"] = fechaHoy;
                                    break;
                                case "FACTURADO":
                                    if (itemBitacora["Facturado"] == null && itemBitacora["Abastecido"] != null)
                                        itemBitacora["Facturado"] = fechaHoy;
                                    break;
                                case "TRANSITO ADUANA":
                                    if (itemBitacora["Transito adn."] == null && itemBitacora["Facturado"] != null)
                                        itemBitacora["Transito adn."] = fechaHoy;
                                    break;
                                case "BACKORDER":
                                    if (itemBitacora["Backorder"] == null)
                                        itemBitacora["Backorder"] = fechaHoy;
                                    break;
                                case "CANCELADO":
                                    if (itemBitacora["Cancelado"] == null)
                                        itemBitacora["Cancelado"] = fechaHoy;
                                    break;
                            }

                            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                            {
                                itemBitacora.SystemUpdate();
                            }
                            #endregion
                        }
                    }
                }
            });
        }*/

        /// <summary>
        /// Actualiza el campo 'Aprobado' de la lista 'Bitácora Pedidos'
        /// </summary>
        /// <param name="properties"></param>
        /*public static void ActualizarBitacoraAprobacion(SPListItem itemOC)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {//Como usuario administrador
                using (SPSite sps = new SPSite(UrlAbastecimiento))
                {
                    using (SPWeb spw = sps.OpenWeb())
                    {
                        if (itemOC.ModerationInformation.Status == SPModerationStatusType.Approved)
                        {
                            SPQuery consultaItm = new SPQuery();
                            consultaItm.Query = "<Where><Eq><FieldRef Name='OC_x0020_asociada_x003a_ID'/>" +
                                "<Value Type='Text'>" + itemOC.ID + "</Value></Eq></Where>";
                            SPListItemCollection colItemsPedidos =
                                spw.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consultaItm);

                            foreach (SPListItem itemPedido in colItemsPedidos)
                            {
                                SPQuery consultaBit = new SPQuery();
                                consultaBit.Query = "<Where><Eq><FieldRef Name='Item_x0020_pedido_x003A_ID0'/>" +
                                    "<Value Type='Text'>" + itemPedido.ID + "</Value></Eq></Where>";
                                SPListItemCollection colItemsBitacora =
                                    spw.Lists[LISTA_BITACORA].GetItems(consultaBit);

                                if (colItemsBitacora.Count != 0)
                                {
                                    SPListItem itemBitacora = colItemsBitacora[0];

                                    if (itemBitacora["Aprobado"] == null)
                                    {
                                        itemBitacora["Aprobado"] = DateTime.Now;
                                        using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                                        {
                                            itemBitacora.SystemUpdate();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }*/

        public static void RelacionarItemCopiadoConOCP(SPListItem itemPedido, SPListItem nuevoItemPedido)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPQuery consulta = new SPQuery();
                    consulta.Query = "<OrderBy><FieldRef Name='ID' Ascending='FALSE' /></OrderBy>";
                    SPListItemCollection itemsOCP = spw.Lists[LISTA_OCP].GetItems(consulta);

                    foreach (SPListItem itemOCP in itemsOCP)
                    {
                        SPFieldLookupValueCollection itemsAsociados =
                            (SPFieldLookupValueCollection) itemOCP["Ítems ordenados"];

                        //Recorrer la coleccion de items asociados del campo indicado
                        foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
                        {
                            if (itemAsociado.LookupId == itemPedido.ID)
                            {
                                SPFieldLookupValue theNewLookupField = new SPFieldLookupValue();
                                itemsAsociados.Add(new SPFieldLookupValue(
                                    nuevoItemPedido.ID, nuevoItemPedido["Item_OC"].ToString()));
                                itemOCP["Ítems ordenados"] = itemsAsociados;
                                nuevoItemPedido["Asignado_OCP"] = "SI";
                                using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                                {
                                    itemOCP.SystemUpdate();
                                    nuevoItemPedido.SystemUpdate();
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region ELIMINACIONES
        /// <summary>
        /// Elimina un elemento de la lista 'Órdenes de Compra Internas' y los items
        /// en los que es referenciado en la lista 'Ítems Pedidos'.
        /// </summary>
        /// <param name="itemIdOC">ID del item</param>
        public static void EliminarOC(int itemIdOC)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    #region Eliminar Ítems Pedidos asociados a la Orden de Compra
                    SPQuery consulta = new SPQuery();
                    consulta.Query = "<Where><Eq><FieldRef Name='OC_x0020_asociada_x003a_ID'/>" +
                        "<Value Type='Text'>" + itemIdOC + "</Value></Eq></Where>";

                    SPListItemCollection listItemsPedidos = spw.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consulta);
                    int itemsCount = listItemsPedidos.Count;

                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        for (int i = 0; i < itemsCount; i++)
                            listItemsPedidos.Delete(0); //Truco para borrar todos los items!!!
                    }
                    #endregion

                    #region Eliminar Orden de Compra
                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        try
                        {
                            SPListItemCollection listItemsOC = spw.Lists[LISTA_OCI].Items;
                            listItemsOC.DeleteItemById(itemIdOC);
                        }//Si el item no existe, continua sin problemas
                        catch { }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Elimina un item de la lista 'Ítems Pedidos'
        /// </summary>
        /// <param name="itemId">ID del item</param>
        /*public static void EliminarItemPedido(int itemId)
        {
            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPQuery consulta = new SPQuery();
                    consulta.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                        "<Value Type='Counter'>" + itemId + "</Value></Eq></Where>";

                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        try
                        {
                            SPListItemCollection listItemsPedidos = spw.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consulta);
                            listItemsPedidos.Delete(0);
                        }//Si el item no existe, continua sin problemas
                        catch { }
                    }
                }
            }
        }*/
        #endregion

        #region RECUPERACIONES
        /// <summary>
        /// Retorna el precio (y no moneda)de un item de lista 'Productos'
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>Arreglo del precio (y no moneda) del item</returns>
        public static List<string> RecuperarPrecioYMonedaItem(int itemId)
        {
            List<string> precioYMoneda = new List<string>();

            using (SPSite sps = new SPSite(UrlAbastecimiento))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPListItem item = spw.Lists[LISTA_ITEMS_CATALOGO].GetItemByIdSelectedFields(
                        itemId, "Precio_x0020_cat_x00e1_logo");

                    if (item["Precio catálogo"] != null)
                        precioYMoneda.Add(item["Precio catálogo"].ToString());
                    else
                        precioYMoneda.Add("");

                    /*if (item["Moneda"] != null)
                        precioYMoneda.Add(item["Moneda"].ToString());
                    else
                        precioYMoneda.Add("");*/

                    return precioYMoneda;
                }
            }
        }

        /// <summary>
        /// Busca y recupera los ítems asociados a la OC en la lista 'Ítems Pedidos'
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="spw"></param>
        /// <returns>Los ítems pedidos asociados a la OC</returns>
        public static SPListItemCollection RecuperarItemsPedidosAsociados(int idOC, SPWeb spw)
        {
            SPListItemCollection itemsPedidos = null;

            SPQuery consulta = new SPQuery();
            consulta.Query = "<Where><Eq><FieldRef Name='OC_x0020_asociada_x003a_ID'/>" +
                "<Value Type='Text'>" + idOC + "</Value></Eq></Where>";
            itemsPedidos = spw.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consulta);

            return itemsPedidos;
        }

        /// <summary>
        /// Recupera los ítems pedidos (lista 'Ítems Pedidos') cuya urgencia no corresponde
        /// al tipo de envio definido (lista 'Envíos').
        /// Si URGENTE => No MARITIMA & Si NORMAL => No CURRIER
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static List<string> RecuperarItemsPedidosNoValidosParaEnvio(SPItemEventProperties properties)
        {//ítem de lista Envíos
            List<string> itemsNoValidos = new List<string>();

            if (properties.AfterProperties["Tipo_x0020_env_x00ed_o"] != null &&
                properties.AfterProperties["Tipo_x0020_env_x00ed_o"].ToString() != "")
            {
                string envioElegido = properties.Web.Lists[LISTA_TIPOS_ENVIO].GetItemById(
                    Convert.ToInt32(properties.AfterProperties["Tipo_x0020_env_x00ed_o"])).Title;

                SPList listaAlmacen = properties.Web.Lists[LISTA_ALMACEN];
                SPList listaFacturas = properties.Web.Lists[LISTA_FACTURAS];
                SPList listaItemsPedidos = properties.Web.Lists[LISTA_ITEMS_PEDIDOS];

                string cadAlmacenesAsociados =
                    properties.AfterProperties["Almacenes_x0020_asociados"].ToString();
                string[] arrAlmacenesAsociados =
                    cadAlmacenesAsociados.Split(new string[] { ";#" }, StringSplitOptions.None);

                //Recorre los Almacenes asociadas al Envío
                for (int i = 0; i < arrAlmacenesAsociados.Length; i += 2)
                {
                    SPQuery consulta = new SPQuery();
                    consulta.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                        "<Value Type='Counter'>" + arrAlmacenesAsociados[i] + "</Value></Eq></Where>";
                    SPListItemCollection losAlmacenes = listaAlmacen.GetItems(consulta);

                    foreach (SPListItem elAlmacen in losAlmacenes)
                    {
                        SPFieldLookupValueCollection facturasAsociadas =
                        (SPFieldLookupValueCollection)elAlmacen["Facturas asociadas"];
                        //Recorre las Facturas asociadas al Almacén
                        foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
                        {
                            SPQuery consulta2 = new SPQuery();
                            consulta2.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                                "<Value Type='Counter'>" + facturaAsociada.LookupId + "</Value></Eq></Where>";
                            SPListItemCollection lasFacturas = listaFacturas.GetItems(consulta2);

                            foreach (SPListItem laFactura in lasFacturas)
                            {
                                SPFieldLookupValueCollection itemsAsociados =
                                    (SPFieldLookupValueCollection)laFactura["Ítems asociados"];
                                //Recorre los Ítems Pedidos asociados a la Factura
                                foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
                                {
                                    SPListItem itemPedido = listaItemsPedidos.GetItemByIdSelectedFields(
                                        itemAsociado.LookupId, "Tipo_x0020_pedido");

                                    if (SubcadenaDespues(itemPedido["Tipo pedido"]).Trim() == "URGENTE" &&
                                        envioElegido.Trim() == "MARITIMA")
                                    {
                                        itemsNoValidos.Add(string.Format("<b>{0}</b>: Un ítem URGENTE no puede ser asociado a un envío de tipo MARITIMA.",
                                            itemPedido["Item_OC"].ToString()));
                                    }
                                    else if (SubcadenaDespues(itemPedido["Tipo pedido"]).Trim() == "NORMAL" &&
                                        envioElegido.Trim() == "CURRIER")
                                    {//NORMAL
                                        itemsNoValidos.Add(string.Format("<b>{0}</b>: Un ítem NORMAL no puede ser asociado a un envío de tipo CURRIER.",
                                            itemPedido["Item_OC"].ToString()));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return itemsNoValidos;
        }
        #endregion
        
        #region FLUJOS
        /// <summary>
        /// Inicia el flujo WFNotificarAprobacion
        /// </summary>
        /// <param name="itemOC">Item sobre el que se iniciara el flujo</param>
        public static void IniciarFlujoNotificarAprobacion(SPListItem itemOC)
        {
            SPList listaOC = itemOC.ParentList;

            SPWorkflowAssociation flujoAsociado =
                listaOC.WorkflowAssociations.GetAssociationByName("Notificar aprobación",
                new CultureInfo(Convert.ToInt32(listaOC.ParentWeb.RegionalSettings.LocaleId)));
            SPSite sitio = listaOC.ParentWeb.Site;

            sitio.WorkflowManager.StartWorkflow(itemOC, flujoAsociado, String.Empty);
        }

        public static void IniciarFlujoNotificarDescuentos(SPListItem itemOC)
        {
            SPModerationInformation estadoAprobacion = itemOC.ModerationInformation;

            if (estadoAprobacion.Status == SPModerationStatusType.Approved)
            {
                SPList listaOC = itemOC.ParentList;

                SPWorkflowAssociation flujoAsociado =
                    listaOC.WorkflowAssociations.GetAssociationByName("Notificar descuentos",
                    new CultureInfo(Convert.ToInt32(listaOC.ParentWeb.RegionalSettings.LocaleId)));
                SPSite sitio = listaOC.ParentWeb.Site;

                sitio.WorkflowManager.StartWorkflow(itemOC, flujoAsociado, String.Empty);
            }
        }
        #endregion

        #region EXTRAS
        /// <summary>
        /// Extrae el valor CLIENTE de una cadena de tipo "1;#CLIENTE"
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado de la cadena</returns>
        public static string SubcadenaDespues(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno = valor.ToString().Substring(valor.ToString().IndexOf('#') + 1);

            return retorno;
        }

        /// <summary>
        /// Extrae el valor 1 de una cadena de tipo "1;#CLIENTE"
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado de la cadena</returns>
        public static string SubcadenaAntes(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno = valor.ToString().Remove(valor.ToString().IndexOf(';'));

            return retorno;
        }
        #endregion
    }
}
