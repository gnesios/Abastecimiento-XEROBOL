using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace AbastecimientoCustomActions.Layouts.AbastecimientoCustomActions
{
    public partial class ActualizarTotal : LayoutsPageBase
    {
        SPListItem elItem;
        SPList laLista;

        const string LISTA_OC = "Órdenes de Compra Internas";
        const string LISTA_FACTURAS = "Facturas";
        //const string LISTA_ENVIOS = "Envíos";
        const string LISTA_ALMACEN = "Almacén (Warehouse)";
        const string LISTA_ITEMS_PEDIDOS = "Ítems Pedidos";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string listaId = Request.QueryString["ListId"];
                string itemId = Request.QueryString["ItemId"];

                laLista = this.Web.Lists[new Guid(listaId)];
                elItem = laLista.GetItemByIdAllFields(Convert.ToInt32(itemId));

                switch (laLista.Title)
                {
                    case LISTA_OC:
                        this.ActualizarPrecioTotalOC(elItem);
                        ltlResultados.Text = "Actualización <b>Precio total</b> finalizada exitosamente.";
                        break;
                    case LISTA_FACTURAS:
                        this.ActualizarTotalFactura(elItem);
                        ltlResultados.Text = "Actualización <b>Total factura</b> finalizada exitosamente.";
                        break;
                    case LISTA_ALMACEN:
                        this.ActualizarTotalFacturas(elItem);
                        ltlResultados.Text = "Actualización <b>Total facturas</b> finalizada exitosamente.";
                        break;
                    default:
                        ltlResultados.Text = "Nada que actualizar.";
                        break;
                }
            }
            catch (Exception ex)
            {
                ltlResultados.Text = ex.Message;
            }
        }

        /// <summary>
        /// Actualiza el campo 'Total facturas' de la lista 'Almacén'
        /// </summary>
        /// <param name="elItem"></param>
        private void ActualizarTotalFacturas(SPListItem itemAlmacen)
        {
            double totalFacturas = 0;

            SPQuery consulta = new SPQuery();
            consulta.Query = "<Where><Eq><FieldRef Name='Almac_x00e9_n_x0020_asociado_x00' />" +
                "<Value Type='Text'>" + itemAlmacen.ID + "</Value></Eq></Where>";
            SPListItemCollection itemsFacturas =
                itemAlmacen.Web.Lists[LISTA_FACTURAS].GetItems(consulta);

            foreach (SPListItem itemFactura in itemsFacturas)
            {
                totalFacturas = totalFacturas +
                    double.Parse(itemFactura["Total factura"].ToString());
            }

            itemAlmacen["Total facturas"] = totalFacturas;
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                try
                {
                    itemAlmacen.Web.AllowUnsafeUpdates = true;
                    itemAlmacen.SystemUpdate();
                }
                finally
                {
                    itemAlmacen.Web.AllowUnsafeUpdates = false;
                }
            }
        }

        /// <summary>
        /// Actualiza el campo 'Total factura' de la lista 'Facturas'
        /// </summary>
        /// <param name="elItem"></param>
        private void ActualizarTotalFactura(SPListItem itemFactura)
        {
            double totalFactura = 0;

            SPQuery consulta = new SPQuery();
            consulta.Query = "<Where><Eq><FieldRef Name='Factura_x0020_asociada_x003a_ID' />" +
                "<Value Type='Text'>" + itemFactura.ID + "</Value></Eq></Where>";
            SPListItemCollection itemsPedidos =
                itemFactura.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consulta);

            foreach (SPListItem itemPedido in itemsPedidos)
            {
                totalFactura = totalFactura +
                    double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ','));
            }

            itemFactura["Total factura"] = totalFactura;
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                try
                {
                    itemFactura.Web.AllowUnsafeUpdates = true;
                    itemFactura.SystemUpdate();
                }
                finally
                {
                    itemFactura.Web.AllowUnsafeUpdates = false;
                }
            }
        }

        /// <summary>
        /// Actualiza el campo 'Precio total' de la lista 'Órdenes de Compra Internas'
        /// </summary>
        /// <param name="itemOC"></param>
        private void ActualizarPrecioTotalOC(SPListItem itemOC)
        {
            double precioTotal = 0;

            SPQuery consulta = new SPQuery();
            consulta.Query = "<Where><Eq><FieldRef Name='OC_x0020_asociada_x003a_ID' />" +
                "<Value Type='Text'>" + itemOC.ID + "</Value></Eq></Where>";
            SPListItemCollection itemsPedidos =
                itemOC.Web.Lists[LISTA_ITEMS_PEDIDOS].GetItems(consulta);

            foreach (SPListItem itemPedido in itemsPedidos)
            {
                precioTotal = precioTotal +
                    double.Parse(SubcadenaDespues(itemPedido["Precio extendido"]).Replace('.', ','));
            }

            itemOC["Precio total"] = precioTotal;
            using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
            {
                try
                {
                    itemOC.Web.AllowUnsafeUpdates = true;
                    itemOC.SystemUpdate();
                }
                finally
                {
                    itemOC.Web.AllowUnsafeUpdates = false;  
                }
            }
        }

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
