using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace AbastecimientoCustomActions.Layouts.AbastecimientoCustomActions
{
    public partial class RedistribuirCostos : LayoutsPageBase
    {
        SPListItem elItem;
        SPList laLista;

        string LISTA_PARAMETROS = "Parámetros del Sistema";
        string LISTA_FACTURAS = "Facturas";
        string LISTA_ALMACEN = "Almacén (Warehouse)";
        string LISTA_ITEMS_PEDIDOS = "Ítems Pedidos";
        string LISTA_PRODUCTOS = "Productos";
        //string LISTA_ITEMS_CATALOGO = "Productos";
        string PARAMETRO_TITULO = "Prorrateo costos";
        string PARAMETRO_VALOR = "SIN ARANCEL";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string listaId = Request.QueryString["ListId"];
                string itemId = Request.QueryString["ItemId"];

                laLista = this.Web.Lists[new Guid(listaId)];
                elItem = laLista.GetItemByIdAllFields(Convert.ToInt32(itemId));

                this.RedistribuirCostosEnvio(elItem);

                ltlResultados.Text = "Redistribución de costos finalizada exitosamente.";
            }
            catch (Exception ex)
            {
                ltlResultados.Text = ex.Message;
            }
        }

        /// <summary>
        /// Redistribuye (prorratea) los costos definidos al envio
        /// para cada ítem pedido asociado
        /// </summary>
        /// <param name="itemEnvio"></param>
        private void RedistribuirCostosEnvio(SPListItem itemEnvio)
        {
            SPList listaParametros = itemEnvio.Web.Lists[LISTA_PARAMETROS];
            SPList listaFacturas = itemEnvio.Web.Lists[LISTA_FACTURAS];
            SPList listaAlmacen = itemEnvio.Web.Lists[LISTA_ALMACEN];
            SPList listaItemsPedidos = itemEnvio.Web.Lists[LISTA_ITEMS_PEDIDOS];

            bool prorrateoConArancel = true;

            #region Obtener el tipo de prorrateo
            SPQuery consultaParam = new SPQuery();
            consultaParam.Query = "<Where><Eq><FieldRef Name='Title' />" +
                "<Value Type='Text'>" + PARAMETRO_TITULO + "</Value></Eq></Where>";
            SPListItemCollection itemsParametros = listaParametros.GetItems(consultaParam);

            if (itemsParametros.Count != 0 &&
                itemsParametros[0]["Valor parámetro"].ToString() == PARAMETRO_VALOR)
            {
                prorrateoConArancel = false;
            }
            #endregion

            #region Distribuir (prorratear) costos
            double totalEnvio = 0;
            double costoFletes = 0;
            double costoAranceles = 0;
            double costoAduana = 0;
            double costoTransportador = 0;
            double costoOtros = 0;

            totalEnvio = this.ObtenerTotalDelEnvio(itemEnvio);
            if (itemEnvio["Costo fletes"] != null)
                costoFletes = double.Parse(itemEnvio["Costo fletes"].ToString());
            if (itemEnvio["Costo aranceles"] != null)
                costoAranceles = double.Parse(itemEnvio["Costo aranceles"].ToString());
            if (itemEnvio["Costo aduana"] != null)
                costoAduana = double.Parse(itemEnvio["Costo aduana"].ToString());
            if (itemEnvio["Costo transportador"] != null)
                costoTransportador = double.Parse(itemEnvio["Costo transportador"].ToString());
            if (itemEnvio["Costo otros"] != null)
                costoOtros = double.Parse(itemEnvio["Costo otros"].ToString());
            
            SPFieldLookupValueCollection almacenesAsociados =
                (SPFieldLookupValueCollection)itemEnvio["Almacenes asociados"];

            foreach (SPFieldLookupValue almacenAsociado in almacenesAsociados)
            {
                SPListItem elAlmacen =
                    listaAlmacen.GetItemByIdSelectedFields(almacenAsociado.LookupId,
                    "Facturas_x0020_asociadas");
                SPFieldLookupValueCollection facturasAsociadas =
                    (SPFieldLookupValueCollection)elAlmacen["Facturas asociadas"];

                foreach (SPFieldLookupValue facturaAsociada in facturasAsociadas)
                {
                    SPListItem laFactura =
                        listaFacturas.GetItemByIdSelectedFields(facturaAsociada.LookupId,
                        "_x00cd_tems_x0020_asociados");
                    SPFieldLookupValueCollection itemsAsociados =
                            (SPFieldLookupValueCollection)laFactura["Ítems asociados"];

                    foreach (SPFieldLookupValue itemAsociado in itemsAsociados)
                    {
                        SPListItem itemPedido = listaItemsPedidos.GetItemByIdSelectedFields(
                            itemAsociado.LookupId, "Costo_x0020_fletes", "Costo_x0020_aranceles",
                            "Costo_x0020_aduana", "Costo_x0020_transportador", "Costo_x0020_otros",
                            "Precio_x0020_extendido", "Item_x0020_asociado");
                        double precioItem =
                            double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ','));
                        double porcentajeArancel = this.ObtenerPorcentajeArancel(itemPedido);

                        if (prorrateoConArancel)
                        {//TODO Prorrateo con aranceles, verificar el calculo
                            itemPedido["Costo fletes"] =
                                this.AplicarProrrateoConArancelFletes(precioItem, totalEnvio, costoFletes);
                            itemPedido["Costo aranceles"] =
                                this.AplicarProrrateoConArancelAranceles(precioItem, totalEnvio, costoFletes, porcentajeArancel);
                            itemPedido["Costo aduana"] =
                                this.AplicarProrrateoConArancelAduana(precioItem, totalEnvio, costoAduana, costoFletes);
                            itemPedido["Costo transportador"] =
                                this.AplicarProrrateoConArancelTransp(precioItem, totalEnvio, costoTransportador, costoFletes);
                            itemPedido["Costo otros"] =
                                this.AplicarProrrateoConArancelOtros(precioItem, totalEnvio, costoOtros, costoFletes);
                        }
                        else
                        {//Prorrateo sin aranceles
                            itemPedido["Costo fletes"] =
                                this.AplicarProrrateoSinArancel(precioItem, totalEnvio, costoFletes);
                            itemPedido["Costo aranceles"] =
                                this.AplicarProrrateoSinArancel(precioItem, totalEnvio, costoAranceles);
                            itemPedido["Costo aduana"] =
                                this.AplicarProrrateoSinArancel(precioItem, totalEnvio, costoAduana);
                            itemPedido["Costo transportador"] =
                                this.AplicarProrrateoSinArancel(precioItem, totalEnvio, costoTransportador);
                            itemPedido["Costo otros"] =
                                this.AplicarProrrateoSinArancel(precioItem, totalEnvio, costoOtros);
                        }

                        using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                        {
                            try
                            {
                                itemPedido.Web.AllowUnsafeUpdates = true;
                                itemPedido.Update();
                            }
                            finally
                            {
                                itemPedido.Web.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                }
            }
            #endregion
        }

        private double ObtenerPorcentajeArancel(SPListItem itemPedido)
        {
            double porcentajeArancel = 0;

            int idProductoAsociado = int.Parse(this.SubcadenaAntes(itemPedido["Ítem asociado"]));
            SPListItem producto = itemPedido.Web.Lists[LISTA_PRODUCTOS].GetItemByIdSelectedFields(
                idProductoAsociado, "Arancel");

            if (producto["Arancel"] != null)
                porcentajeArancel = double.Parse(producto["Arancel"].ToString());

            return porcentajeArancel;
        }

        /// <summary>
        /// Obtiene la sumatoria del campo 'Total facturas' de la lista Almacén
        /// asoacido a los almacenes de un envío.
        /// </summary>
        /// <param name="itemEnvio"></param>
        /// <returns></returns>
        private double ObtenerTotalDelEnvio(SPListItem itemEnvio)
        {
            double totalFacturas = 0;

            SPFieldLookupValueCollection almacenesAsociados =
                (SPFieldLookupValueCollection)itemEnvio["Almacenes asociados"];
            
            //Recorre los Almacenes asociados al Envío
            foreach (SPFieldLookupValue almacenAsociado in almacenesAsociados)
            {
                SPListItem itemAlmacen =
                    itemEnvio.Web.Lists[LISTA_ALMACEN].GetItemByIdSelectedFields(almacenAsociado.LookupId,
                    "Total_x0020_facturas");

                if (itemAlmacen["Total facturas"] != null)
                {
                    totalFacturas = totalFacturas +
                        double.Parse(itemAlmacen["Total facturas"].ToString());
                }
            }

            return totalFacturas;
        }

        /// <summary>
        /// Realiza el prorrateo SIN ARANCEL
        /// </summary>
        /// <param name="precioItem">Precio extendido del item</param>
        /// <param name="totalEnvio"></param>
        /// <param name="totalCosto"></param>
        /// <returns></returns>
        private double AplicarProrrateoSinArancel(double precioItem, double totalEnvio, double totalCosto)
        {
            double valorProrrateado = totalCosto * (precioItem / totalEnvio);

            return valorProrrateado;
        }

        /// <summary>
        /// Calcula el valor prorrateado para FLETES
        /// </summary>
        /// <param name="precioItem"></param>
        /// <param name="totalEnvio"></param>
        /// <param name="costoFletes"></param>
        /// <returns></returns>
        private double AplicarProrrateoConArancelFletes(double precioItem, double totalEnvio, double costoFletes)
        {
            double valorFletes = costoFletes * (precioItem / totalEnvio);

            return valorFletes;
        }

        /// <summary>
        /// Calcula el valor prorrateado para ARANCELES
        /// </summary>
        /// <param name="precioItem"></param>
        /// <param name="totalEnvio"></param>
        /// <param name="costoFletes"></param>
        /// <param name="porcentajeArancel"></param>
        /// <returns></returns>
        private double AplicarProrrateoConArancelAranceles(double precioItem, double totalEnvio, double costoFletes,
            double porcentajeArancel)
        {
            double valorArancel = porcentajeArancel * (precioItem + (costoFletes * precioItem / totalEnvio));

            return valorArancel;
        }

        /// <summary>
        /// Calcula el valor prorrateado para ADUANA
        /// </summary>
        /// <param name="precioItem"></param>
        /// <param name="totalEnvio"></param>
        /// <param name="costoAduana"></param>
        /// <param name="costoFletes"></param>
        /// <returns></returns>
        private double AplicarProrrateoConArancelAduana(double precioItem, double totalEnvio, double costoAduana,
            double costoFletes)
        {
            double valorAduana =
                costoAduana * ((precioItem + (costoFletes * precioItem / totalEnvio)) / (totalEnvio + costoFletes));

            return valorAduana;
        }

        /// <summary>
        /// Calcula el valor prorrateado para TRANSPORTADOR
        /// </summary>
        /// <param name="precioItem"></param>
        /// <param name="totalEnvio"></param>
        /// <param name="costoTransp"></param>
        /// <param name="costoFletes"></param>
        /// <returns></returns>
        private double AplicarProrrateoConArancelTransp(double precioItem, double totalEnvio, double costoTransp,
            double costoFletes)
        {
            double valorTransp =
                costoTransp * ((precioItem + (costoFletes * precioItem / totalEnvio)) / (totalEnvio + costoFletes));

            return valorTransp;
        }

        /// <summary>
        /// Calcula el valor prorrateado para OTROS
        /// </summary>
        /// <param name="precioItem"></param>
        /// <param name="totalEnvio"></param>
        /// <param name="costoOtros"></param>
        /// <param name="costoFletes"></param>
        /// <returns></returns>
        private double AplicarProrrateoConArancelOtros(double precioItem, double totalEnvio, double costoOtros,
            double costoFletes)
        {
            double valorOtros =
                costoOtros * ((precioItem + (costoFletes * precioItem / totalEnvio)) / (totalEnvio + costoFletes));

            return valorOtros;
        }

        /// <summary>
        /// Extrae el valor 1 de una cadena de tipo "1;#CLIENTE"
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado de la cadena</returns>
        public string SubcadenaAntes(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno = valor.ToString().Remove(valor.ToString().IndexOf(';'));

            return retorno;
        }
        
        /// <summary>
        /// Extrae el valor CLIENTE de una cadena de tipo "1;#CLIENTE"
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado de la cadena</returns>
        public string SubcadenaDespues(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno = valor.ToString().Substring(valor.ToString().IndexOf('#') + 1);

            return retorno;
        }
    }
}
