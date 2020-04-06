using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint.WebControls;
using System.Globalization;
using System.Collections.Generic;

namespace Abastecimiento.WPNuevaOC
{
    public partial class WPNuevaOCUserControl : UserControl
    {
        #region Variables Globales
        double PrecioTotal = 0;
        int ItemsTotal = 0;
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Asignar IDOrdenCompra
            lblIDD.Text = this.IDOrdenCompra.Value;
            
            txtTituloD.ItemIdAsString = this.IDOrdenCompra.Value;
            dtfFechaSolicitadaD.ItemIdAsString = this.IDOrdenCompra.Value;

            dsItemsPedidos.SelectCommand =
                "<Query><OrderBy><FieldRef Name='ID'/></OrderBy><Where><Eq>" +
                "<FieldRef Name='OC_x0020_asociada_x003a_ID'/><Value Type='Text'>" +
                this.IDOrdenCompra.Value + "</Value></Eq></Where></Query>";
            #endregion
        }

        protected void lnkDefinirItems_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                this.GuardarOrdenCompra(sender.GetType().Name);

                #region Asignar IDOrdenCompra
                lblIDD.Text = this.IDOrdenCompra.Value;
                //txtCodigoXbolD.ItemIdAsString = IDOrdenCompra.Value;
                txtTituloD.ItemIdAsString = this.IDOrdenCompra.Value;
                dtfFechaSolicitadaD.ItemIdAsString = this.IDOrdenCompra.Value;
                #endregion

                mvOrdenCompra.ActiveViewIndex = 1;
            }
        }

        protected void lnkDefinirOC_Click(object sender, EventArgs e)
        {
            mvOrdenCompra.ActiveViewIndex = 0;
        }

        protected void btnAgregarItem_Click(object sender, EventArgs e)
        {
            lblValidacionItems.Text = "";

            mvOrdenCompra.ActiveViewIndex = 2;
        }

        protected void btnGuardarOC_Click(object sender, EventArgs e)
        {
            #region Validar ingreso items
            /*if (grvItemsPedidos.Rows.Count == 0)
            {
                lblValidacionItems.Text = "Debe agregar al menos un ítem a la Orden de Compra.";
                return;
            }*/
            #endregion

            this.GuardarOrdenCompra(sender.GetType().Name);
            this.GenerarEnvioParaOC();

            string url = this.Page.Request.Url.LocalPath;
            string urlLista = url.Remove(url.LastIndexOf('/'));
            this.Response.Redirect(urlLista);
        }

        protected void btnGuardarItem_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                this.InsertarItemPedido();

                #region Limpiar campos de formulario
                ddlItemAsociado.SelectedIndex = 0;
                numCantidad.Text = String.Empty;
                numPrecioUnitario.Text = String.Empty;
                numPeso.Text = String.Empty;
                lufUnidadMedida.EnableViewState = false;
                txtDimensiones.Text = String.Empty;
                lufCliente.EnableViewState = false;
                lufClienteAsociado.EnableViewState = false;
                lufTipoPedido.EnableViewState = false;
                #endregion

                grvItemsPedidos.DataBind();

                mvOrdenCompra.ActiveViewIndex = 1;
            }
        }

        protected void btnCancelarOC_Click(object sender, EventArgs e)
        {
            this.EliminarOrdenCompra();

            string url = this.Page.Request.Url.LocalPath;
            string urlLista = url.Remove(url.LastIndexOf('/'));
            this.Response.Redirect(urlLista);
        }

        protected void btnCancelarItem_Click(object sender, EventArgs e)
        {
            mvOrdenCompra.ActiveViewIndex = 1;
        }

        protected void grvItemsPedidos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                PrecioTotal += Convert.ToDouble(
                    this.FormatoSubcadena(DataBinder.Eval(e.Row.DataItem, "Precio extendido")).Replace('.', ','));
                ItemsTotal += Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Cantidad"));
            }
            else if(e.Row.RowType == DataControlRowType.Footer)
            {
                Label lbPrecioTotal = (Label)e.Row.FindControl("lblPrecioTotal");
                Label lbCantidadTotal = (Label)e.Row.FindControl("lblCantidadTotal");

                lbPrecioTotal.Text = this.FormatoNumeroDecimal(PrecioTotal.ToString());
                lbCantidadTotal.Text = this.FormatoNumeroEntero(ItemsTotal.ToString());

                PrecioTotalOC.Value = PrecioTotal.ToString();

                e.Row.Font.Bold = true;
            }
        }

        protected void grvItemsPedidos_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            if (this.Page.IsValid)
            {
                int itemId = Convert.ToInt32(grvItemsPedidos.DataKeys[e.RowIndex].Value.ToString());
                GridViewRow row = grvItemsPedidos.Rows[e.RowIndex];

                this.ActualizarItemPedido(itemId, row);
            }
        }

        /*protected void grvItemsPedidos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int itemId = Convert.ToInt32(grvItemsPedidos.DataKeys[e.RowIndex].Value.ToString());

            EjecutorOperacionesSP.EliminarItemPedido(itemId);
        }*/

        protected void ddlItemAsociado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlItemAsociado.SelectedValue))
            {
                List<string> precioYMoneda =
                    EjecutorOperacionesSP.RecuperarPrecioYMonedaItem(Convert.ToInt32(ddlItemAsociado.SelectedValue));

                if (!string.IsNullOrEmpty(precioYMoneda[0]))
                    numPrecioUnitario.Value = double.Parse(precioYMoneda[0], new CultureInfo("es-BO"));
                else
                    numPrecioUnitario.Value = precioYMoneda[0];

                //lufMonedaItem.Value = precioYMoneda[1];
            }
            else
            {
                numPrecioUnitario.Value = "";
                //lufMonedaItem.Value = "";
            }
        }

        /// <summary>
        /// Extrae el valor CLIENTE de una cadena de tipo "1;#CLIENTE"
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado de la cadena</returns>
        protected string FormatoSubcadena(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno = valor.ToString().Substring(valor.ToString().IndexOf('#') + 1);

            return retorno;
        }

        /// <summary>
        /// Formatea un valor numerico 10000,50 en 10.000,50
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado</returns>
        protected string FormatoNumeroDecimal(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno =
                    String.Format("{0:#,0.00}", double.Parse(valor.ToString(), new CultureInfo("es-BO")));

            return retorno;
        }

        /// <summary>
        /// Formatea un valor numerico 10000 en 10.000
        /// </summary>
        /// <param name="valor"></param>
        /// <returns>El valor formateado</returns>
        protected string FormatoNumeroEntero(object valor)
        {
            string retorno = "";
            if (valor != null)
                retorno =
                    String.Format("{0:#,0}", double.Parse(valor.ToString(), new CultureInfo("es-BO")));

            return retorno;
        }

        /// <summary>
        /// Guardar (como nuevo) o actualizar un item de la lista 'Órdenes de Compra Internas'
        /// </summary>
        private void GuardarOrdenCompra(string sender)
        {
            int idOrden = Convert.ToInt32(this.IDOrdenCompra.Value);

            string titulo = txtTitulo.Text;
            string fechaSolicitada = dtfFechaSolicitada.Value.ToString();
            string observaciones = txtObservaciones.Text;
            string precioTotal = this.PrecioTotalOC.Value;
            string moneda = null;
            if (lufMoneda.Value != null)
                moneda = lufMoneda.Value.ToString();

            int itemId = EjecutorOperacionesSP.ActualizarOC(idOrden, titulo,  fechaSolicitada,
                observaciones, precioTotal, moneda, sender);

            if (itemId == 0)
            {//Se asigna el ID del item creado a la variable de tipo HiddenField
                this.IDOrdenCompra.Value =
                    EjecutorOperacionesSP.InsertarOC(titulo, fechaSolicitada, observaciones).ToString();
            }
        }

        /// <summary>
        /// Genera el Envío asociado para los ítems pedidos en la OC.
        /// Adicionalmente, tambien genera la Factura que se asocia al Envío generado.
        /// </summary>
        private void GenerarEnvioParaOC()
        {
            int idOrden = Convert.ToInt32(this.IDOrdenCompra.Value);

            string tipoEnvio = null;
            if (ddlTipoEnvio.SelectedValue != "")
                tipoEnvio = ddlTipoEnvio.SelectedValue;

            if (tipoEnvio != null)
            {
                EjecutorOperacionesSP.InsertarEnvioParaOC(tipoEnvio, idOrden);
            }
        }

        /// <summary>
        /// Eliminar un item de la lista 'Órdenes de Compra Internas'
        /// y sus items relacionados en la lista 'Ítems Pedidos'
        /// </summary>
        private void EliminarOrdenCompra()
        {
            int idOrden = Convert.ToInt32(this.IDOrdenCompra.Value);

            EjecutorOperacionesSP.EliminarOC(idOrden);
        }

        /// <summary>
        /// Guardar nuevo ítem asociado a la OC
        /// </summary>
        private void InsertarItemPedido()
        {
            //string ocAsociada = this.Request.QueryString["oc"];
            //int itemId = Convert.ToInt32(this.IDItemPedido.Value);

            string ocAsociada = this.IDOrdenCompra.Value;
            string titulo = ddlItemAsociado.SelectedItem.Text;
            string itemAsociado = ddlItemAsociado.SelectedItem.Value;
            string cantidad = numCantidad.Value.ToString();
            string precioU = numPrecioUnitario.Value.ToString();
            //string moneda = lufMonedaItem.Value.ToString();
            string peso = "";
            if (numPeso.Value != null)
                peso = numPeso.Value.ToString();
            string unidadM = "";
            if (lufUnidadMedida.Value != null)
                unidadM = lufUnidadMedida.Value.ToString();
            string dims = "";
            if (txtDimensiones.Value != null)
                dims = txtDimensiones.Value.ToString();
            string cliente = lufCliente.Value.ToString();
            string clienteAsociado = "";
            if (lufClienteAsociado.Value != null)
                clienteAsociado = lufClienteAsociado.Value.ToString();
            string tipoPedido = lufTipoPedido.Value.ToString();

            EjecutorOperacionesSP.InsertarItemPedido(itemAsociado, ocAsociada, titulo, cantidad,
                precioU, peso, unidadM, dims, cliente, tipoPedido, clienteAsociado);
        }

        /// <summary>
        /// Actualiza la fila seleccionada del grid grvItemsPedidos
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="row"></param>
        private void ActualizarItemPedido(int itemId, GridViewRow row)
        {
            NumberField fCantidad = (NumberField)row.FindControl("numCantidad");
            NumberField fPrecioU = (NumberField)row.FindControl("numPrecioUnitario");
            //LookupField fMoneda = (LookupField)row.FindControl("lufMonedaItem");
            NumberField fPeso = (NumberField)row.FindControl("numPeso");
            LookupField fUnidadM = (LookupField)row.FindControl("lufUnidadMedida");
            TextField fDims = (TextField)row.FindControl("txtDimensiones");
            LookupField fCliente = (LookupField)row.FindControl("lufCliente");
            LookupField fClienteAsociado = (LookupField)row.FindControl("lufClienteAsociado");
            LookupField fTipoPedido = (LookupField)row.FindControl("lufTipoPedido");

            string cantidad = fCantidad.Value.ToString();
            string precioU = fPrecioU.Value.ToString();
            //string moneda = fMoneda.Value.ToString();
            string peso = "";
            if (fPeso.Value != null)
                peso = fPeso.Value.ToString();
            string unidadM = "";
            if (fUnidadM.Value != null)
                unidadM = fUnidadM.Value.ToString();
            string dims = "";
            if (fDims.Value != null)
                dims = fDims.Value.ToString();
            string cliente = fCliente.Value.ToString();
            string clienteAsociado = "";
            if (fClienteAsociado.Value != null)
                clienteAsociado = fClienteAsociado.Value.ToString();
            string tipoPedido = fTipoPedido.Value.ToString();

            EjecutorOperacionesSP.ActualizarItemPedido(itemId, cantidad, precioU,
                peso, unidadM, dims, cliente, tipoPedido, clienteAsociado);
        }
    }
}
