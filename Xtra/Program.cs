using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Globalization;

namespace Xtra
{
    class Program
    {
        static void Main(string[] args)
        {
            InsertarNuevoItem();

            //En espera = 2
            //Aprobado = 0
            //Rechazado = 1
        }

        private static void RecuperarElementoPorID(int itemId)
        {
            using (SPSite sps = new SPSite("http://abastecimiento"))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPListItem item = spw.Lists["Bitácora Pedidos"].GetItemById(itemId);
                    string apro = item["Item_x0020_pedido0"].ToString();

                    SPQuery consulta = new SPQuery();
                    consulta.Query = "<Where><Eq><FieldRef Name='ID'/>" +
                        "<Value Type='Counter'>" + itemId + "</Value></Eq></Where>";

                    if (spw.Lists["Ítems Pedidos"].GetItems(consulta).Count == 0)
                        Console.WriteLine("No existe.");
                    else
                        Console.WriteLine("Si existe.");

                    Console.ReadLine();
                }
            }
        }

        private static void Conversiones()
        {
            string cad = "10000";
            double dou = Convert.ToDouble(cad);
            //Console.WriteLine(dou);
            Console.WriteLine(String.Format("{0:#,0}", double.Parse(dou.ToString(), new CultureInfo("es-BO"))));
            Console.ReadLine();
        }

        private static void InsertarCampoLookup(int idItem)
        {
            using (SPSite sps = new SPSite("http://abastecimiento"))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPListItem item = spw.Lists["Órdenes de Compra Internas"].GetItemById(idItem);

                    SPFieldLookupValueCollection itemsPedidos = new SPFieldLookupValueCollection();
                    //SPFieldLookupValue itemPedido = new SPFieldLookupValue(

                    item["Ítems pedidos"] = 1;
                }
            }
        }

        private static void InsertarNuevoItem()
        {
            using (SPSite sps = new SPSite("http://abastecimiento"))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    #region Factura
                    SPListItemCollection listItemsFacturas = spw.Lists["Facturas"].Items;
                    SPListItem itemFactura = listItemsFacturas.Add();

                    itemFactura["Num. factura"] = "(Pre-factura automatica)";
                    itemFactura["Fecha factura"] = DateTime.Today;
                    itemFactura["Total factura"] = 1000;

                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        itemFactura.SystemUpdate();
                    }
                    #endregion

                    #region Envio
                    SPListItemCollection listItemsEnvios = spw.Lists["Envíos"].Items;
                    SPListItem itemEnvio = listItemsEnvios.Add();

                    SPFieldLookupValueCollection facturasAsociadas = new SPFieldLookupValueCollection();
                    facturasAsociadas.Add(new SPFieldLookupValue(itemFactura.ID, itemFactura.Title));

                    itemEnvio["Title"] = "(Pre-envío atomatica)";
                    itemEnvio["Facturas asociadas"] = facturasAsociadas;
                    itemEnvio["Total facturas"] = itemFactura["Total factura"];

                    using (DisabledItemEventsScope scope = new DisabledItemEventsScope())
                    {
                        itemEnvio.SystemUpdate();
                    }
                    #endregion

                    #region ???
                    itemFactura["Envío asociado"] = itemEnvio.ID;
                    #endregion
                }
            }
        }

        private static void CompararCadenas()
        {
            string cad1 = "{101}";
            
            List<string> lista =
                new List<string>(cad1.Split(new char[] {'{', '}'}, StringSplitOptions.None));
        }

        private static void EventReceivers()
        {
            using (SPSite sps = new SPSite("http://abastecimiento"))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    Console.WriteLine("Programa que muestra los eventos sobre una lista dada.");
                    Console.Write("Lista: ");
                    string lista = Console.ReadLine();
                    SPList splist = spw.Lists[lista];
                    foreach (SPEventReceiverDefinition sprd in splist.EventReceivers)
                    {
                        Console.WriteLine(sprd.Class + " " + sprd.Name + " " + sprd.Type + " <" + sprd.Id + ">");
                    }

                    Console.ReadLine();
                }
            }
        }

        private static void LeerLookupCollections()
        {
            using (SPSite sps = new SPSite("http://abastecimiento"))
            {
                using (SPWeb spw = sps.OpenWeb())
                {
                    SPListItem listItem = spw.Lists["Ítems Pedidos"].GetItemById(215);
                    //SPFieldLookupValueCollection collection = (SPFieldLookupValueCollection)listItem["Ítems pedidos"];

                    string mod =
                        Convert.ToDateTime(listItem["Modificado"].ToString()).ToString("dd/MM/yyyy HH:mm");

                    string caad = listItem["Estado de aprobación"].ToString();

                    //foreach (SPFieldLookupValue item in collection)
                    //{
                    //    cad = item.ToString();
                    //}
                }
            }
        }
    }
}
