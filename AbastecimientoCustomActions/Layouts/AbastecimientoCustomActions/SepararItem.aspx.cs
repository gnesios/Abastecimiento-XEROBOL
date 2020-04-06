using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;

namespace AbastecimientoCustomActions.Layouts.AbastecimientoCustomActions
{
    public partial class SepararItem : LayoutsPageBase
    {
        SPListItem itemPedido;
        SPList listaItemsPedidos;
        int cantidadOriginal;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string listaItemsPedidosId = Request.QueryString["ListId"];
                string itemPedidoId = Request.QueryString["ItemId"];

                listaItemsPedidos = this.Web.Lists[new Guid(listaItemsPedidosId)];
                itemPedido = listaItemsPedidos.GetItemByIdAllFields(Convert.ToInt32(itemPedidoId));

                cantidadOriginal = int.Parse(itemPedido["Cantidad"].ToString());

                lblItemAccion.Text = "<b>" + itemPedido.Title + "</b>" + " definido originalmente con "
                    + "<b>" + cantidadOriginal + "</b>.";
            }
            catch (Exception ex)
            {
                pnlFormulario.Visible = false;
                ltlResultados.Text = ex.Message;
            }
        }

        /// <summary>
        /// Valida el ingreso de un valor menor a la cantidad orginal del item a separar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void ValidarCantidad(object source, ServerValidateEventArgs args)
        {
            try
            {
                int cantidadSeparada = int.Parse(args.Value);

                if (cantidadSeparada < cantidadOriginal && cantidadSeparada > 0)
                    args.IsValid = true;
                else
                    args.IsValid = false;
            }
            catch
            {
                args.IsValid = false;
            }
        }

        /// <summary>
        /// Crear una copia del item con la columna 'Cantidad' modificada segun la
        /// ingresada por el usuario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSepararItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    SPListItemCollection itemsPedidos = listaItemsPedidos.Items;
                    SPListItem nuevoItemPedido = itemsPedidos.Add();

                    #region Crear copia del ítem
                    string[] arrTitulo = itemPedido.Title.Split(new string[] { ";#" }, StringSplitOptions.None);
                    if (arrTitulo.Length == 1)
                    {
                        string titulo = itemPedido.Title + ";#1";
                        nuevoItemPedido["Title"] = titulo;
                    }
                    else
                    {
                        string titulo = arrTitulo[0] + ";#" +
                            (int.Parse(arrTitulo[arrTitulo.Length - 1]) + 1).ToString();
                        nuevoItemPedido["Title"] = titulo;
                    }
                    nuevoItemPedido["OCI asociada"] = itemPedido["OCI asociada"];
                    //nuevoItemPedido["Factura asociada"] = itemPedido["Factura asociada"];
                    nuevoItemPedido["Ítem asociado"] = itemPedido["Ítem asociado"];
                    nuevoItemPedido["Precio unitario"] = itemPedido["Precio unitario"];
                    nuevoItemPedido["Peso"] = itemPedido["Peso"];
                    nuevoItemPedido["Unidad medida"] = itemPedido["Unidad medida"];
                    nuevoItemPedido["Dimensiones"] = itemPedido["Dimensiones"];
                    nuevoItemPedido["País origen"] = itemPedido["País origen"];
                    nuevoItemPedido["Cliente"] = itemPedido["Cliente"];
                    nuevoItemPedido["Cliente asociado"] = itemPedido["Cliente asociado"];
                    nuevoItemPedido["Estado pedido"] = itemPedido["Estado pedido"];
                    nuevoItemPedido["Tipo pedido"] = itemPedido["Tipo pedido"];
                    //nuevoItemPedido["Tipo pago"] = itemPedido["Tipo pago"];
                    nuevoItemPedido["Fecha prevista llegada"] = itemPedido["Fecha prevista llegada"];
                    nuevoItemPedido["Observaciones"] = "Copia de ítem " + itemPedido.Title + " (ID " + itemPedido.ID + ").";
                    nuevoItemPedido["Cantidad"] = int.Parse(txbCantidad.Text);

                    nuevoItemPedido.Update();
                    #endregion

                    #region Actualizar ítem original
                    itemPedido["Cantidad"] = cantidadOriginal - int.Parse(txbCantidad.Text);

                    itemPedido.Update();
                    #endregion

                    #region Relacionar ítem copiado
                    Abastecimiento.EjecutorOperacionesSP.RelacionarItemCopiadoConOCP(itemPedido, nuevoItemPedido);
                    #endregion

                    this.Context.Response.Write("<script type='text/javascript'>window.frameElement.commitPopup();</script>");
                    this.Context.Response.Flush();
                    this.Context.Response.End();
                }
            }
            catch (Exception ex)
            {
                ltlResultados.Text = ex.Message;
            }
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
    }
}
