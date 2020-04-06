<%@ Assembly Name="Abastecimiento, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8edd06d77339fe05" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WPNuevaOCUserControl.ascx.cs" Inherits="Abastecimiento.WPNuevaOC.WPNuevaOCUserControl" %>

<style type="text/css">
    .ms-formlabel{
        text-align:left;
        font-family:verdana;
        font-size:.7em;
        border-top:1px solid #d8d8d8;
        padding-top:3px;
        padding-right:8px;
        padding-bottom:6px;
        color:#525252;
        font-weight:bold;
        }
        
    .ms-formbody{
        font-family:verdana;
        font-size:.7em;
        vertical-align:top;
        background:#f6f6f6;
        border-top:1px solid #d8d8d8;
        padding:3px 6px 4px 6px;
        }
</style>

<asp:HiddenField runat="server" ID="IDOrdenCompra" Value="0" />
<asp:HiddenField runat="server" ID="PrecioTotalOC" Value="0" />

<asp:MultiView ID="mvOrdenCompra" runat="server" ActiveViewIndex="0">
    <asp:View ID="vOrdenCompra" runat="server">
        <%--Informacion OC--%>
        <SharePoint:SPDataSource runat="server" ID="dsTiposEnvio" DataSourceMode="List"
             SelectCommand="<Query><OrderBy><FieldRef Name='Title' /></OrderBy></Query>">
            <SelectParameters>
                <asp:Parameter Name="WebUrl" DefaultValue="/" />
                <asp:Parameter Name="ListID" 
                    DefaultValue="3E4EB4ED-7229-4B34-BD7C-567E84D9233C" />
            </SelectParameters>
        </SharePoint:SPDataSource>
        <table border="0" cellspacing="0" width="100%">
            <tr><%--Título--%>
		        <td width="190px" valign="top" class="ms-formlabel">
			        <SharePoint:FieldLabel runat="server" ID="lblTitulo" ControlMode="New" FieldName="Título" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
		        </td>
		        <td width="400px" valign="top" class="ms-formbody">
			        <SharePoint:TextField runat="server" ID="txtTitulo" ControlMode="New" FieldName="Título" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
			        <SharePoint:FieldDescription runat="server" ID="desTitulo" ControlMode="New" FieldName="Título" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
		        </td>
	        </tr>
            <tr><%--Fecha solicitada--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblFechaSolicitada" ControlMode="New" FieldName="Fecha solicitada" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:DateTimeField runat="server" ID="dtfFechaSolicitada" ControlMode="New" FieldName="Fecha solicitada" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                    <SharePoint:FieldDescription runat="server" ID="desFechaSolicitada" ControlMode="New" FieldName="Fecha solicitada" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
            </tr>
            <tr><%--Tipo envío--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <%--<SharePoint:FieldLabel runat="server" ID="lblTipoEnvio" ControlMode="Display" FieldName="Tipo envío" ListId="A0B39C46-97A3-4CCD-B2E1-5A4818CEF9B9" />--%>
                    Tipo envío
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <asp:DropDownList runat="server" ID="ddlTipoEnvio" DataSourceID="dsTiposEnvio" AppendDataBoundItems="true"
                        DataTextField="Título" DataValueField="ID">
                        <asp:ListItem Text="(ninguno)" Value="" />
                    </asp:DropDownList>
                    <br />
                    <asp:Label runat="server" ID="desTipoEnvio" Text="Si sabe el tipo de envío para esta OC, elíjala." />
                    <%--<SharePoint:LookupField runat="server" ID="lufTipoEnvio" ControlMode="Display" FieldName="Tipo envío" ListId="A0B39C46-97A3-4CCD-B2E1-5A4818CEF9B9" />
                    <SharePoint:FieldDescription runat="server" ID="desTipoEnvio" ControlMode="Display" FieldName="Tipo envío" ListId="A0B39C46-97A3-4CCD-B2E1-5A4818CEF9B9" />--%>
                </td>
            </tr>
            <tr><%--Observaciones--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblObservaciones" ControlMode="New" FieldName="Observaciones" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:RichTextField runat="server" ID="txtObservaciones" ControlMode="New" FieldName="Observaciones" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                    <SharePoint:FieldDescription runat="server" ID="desObservaciones" ControlMode="New" FieldName="Observaciones" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
            </tr>
            <tr><%--[Botones]--%>
                <td class="ms-toolbar" nowrap="nowrap" colspan="2">
                    <table>
                        <tr>
                            <td width="99%" class="ms-toolbar" nowrap="nowrap"><IMG SRC="/_layouts/images/blank.gif" width="1" height="18"/></td>
                            <td class="ms-toolbar" nowrap="nowrap">
                                <%--<SharePoint:SaveButton runat="server" ID="btnGuardar" ControlMode="New" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />--%>
                                <asp:LinkButton runat="server" ID="lnkDefinirItems" CausesValidation="true" Text="Definir ítems >"
                                    ToolTip="Avanzar a la página siguiente para la definición de ítems a ser pedidos." OnClick="lnkDefinirItems_Click" />
                            </td>
                            <td class="ms-separator">&nbsp;</td>
                            <td class="ms-toolbar" nowrap="nowrap" align="right">
                                <%--<SharePoint:GoBackButton runat="server" ID="btnCancelar" ControlMode="New" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />--%>
                                <asp:Button runat="server" ID="btnCancelarOC1" CausesValidation="false" Text="Cancelar" OnClick="btnCancelarOC_Click" CssClass="ms-ButtonHeightWidth"
                                    OnClientClick="return confirm('¿Está seguro de querer cancelar la creación de esta orden de compra?')" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <%--Informacion OC--%>
    </asp:View>
    <asp:View ID="vItemsPedidos" runat="server">
        <%--Informacion Items OC--%>
        <SharePoint:SPDataSource runat="server" ID="dsItemsPedidos" DataSourceMode="List">
            <SelectParameters>
                <asp:Parameter Name="WebUrl" DefaultValue="/" />
                <asp:Parameter Name="ListID" DefaultValue="267F27A1-690D-4994-91DA-F927F7421756" />
            </SelectParameters>
            <DeleteParameters>
                <asp:Parameter Name="ListID" DefaultValue="267F27A1-690D-4994-91DA-F927F7421756" />
            </DeleteParameters>
            <UpdateParameters>
                <asp:Parameter Name="ListID" DefaultValue="267F27A1-690D-4994-91DA-F927F7421756" />
            </UpdateParameters>
        </SharePoint:SPDataSource>
        <table border="0" cellspacing="0" width="100%">
            <tr><%--ID--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    ID OC
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <asp:Label runat="server" ID="lblIDD" />
                </td>
            </tr>
            <tr><%--Título--%>
		        <td width="190px" valign="top" class="ms-formlabel">
			        <SharePoint:FieldLabel runat="server" ID="lblTituloD" ControlMode="Display" FieldName="Título" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
		        </td>
		        <td width="400px" valign="top" class="ms-formbody">
			        <SharePoint:TextField runat="server" ID="txtTituloD" ControlMode="Display" FieldName="Título" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
		        </td>
	        </tr>
            <tr><%--Fecha solicitada--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblFechaSolicitadaD" ControlMode="Display" FieldName="Fecha solicitada" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:DateTimeField runat="server" ID="dtfFechaSolicitadaD" ControlMode="Display" FieldName="Fecha solicitada" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
            </tr>
            <tr><%--Moneda--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblMoneda" ControlMode="New" FieldName="Moneda" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:LookupField runat="server" ID="lufMoneda" ControlMode="New" FieldName="Moneda" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                    <SharePoint:FieldDescription runat="server" ID="desMoneda" ControlMode="New" FieldName="Moneda" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
            </tr>
            <tr>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblItemsPedidos" ControlMode="New" FieldName="Ítems pedidos" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:SPGridView runat="server" ID="grvItemsPedidos" DataSourceID="dsItemsPedidos" AutoGenerateColumns="false" DataKeyNames="ID" ShowHeader="true"
                    ShowFooter="true" OnRowUpdating="grvItemsPedidos_RowUpdating" OnRowDataBound="grvItemsPedidos_RowDataBound">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="/_layouts/images/EDITITEM.GIF" CommandName="Edit" CausesValidation="false" />
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:ImageButton runat="server" ID="btnUpdate" ImageUrl="/_layouts/images/SAVEITEM.GIF" CommandName="Update" CausesValidation="true" />
                                    <asp:ImageButton runat="server" ID="btnCancel" ImageUrl="/_layouts/images/CRIT_16.GIF" CommandName="Cancel" CausesValidation="false" />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Item" HeaderStyle-HorizontalAlign="Left">
                                <ItemTemplate>
                                    <%# this.FormatoSubcadena(Eval("Ítem asociado"))%>
                                </ItemTemplate>
                                <FooterTemplate>
                                    Totales
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Cantidad" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <%# this.FormatoNumeroEntero(Eval("Cantidad")) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:NumberField runat="server" ID="numCantidad" ControlMode="New" FieldName="Cantidad" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Text='<%# Eval("Cantidad") %>' DisplaySize="5" />
                                </EditItemTemplate>
                                <FooterTemplate>
                                    <asp:Label runat="server" ID="lblCantidadTotal" Text="" />
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="p/u" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <%# this.FormatoNumeroDecimal(Eval("Precio unitario")) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:NumberField runat="server" ID="numPrecioUnitario" ControlMode="New" FieldName="Precio unitario" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Text='<%# Eval("Precio unitario") %>' DisplaySize="5" />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <%--<asp:TemplateField HeaderText="Moneda" HeaderStyle-HorizontalAlign="Left">
                                <ItemTemplate>
                                    <%# this.FormatoSubcadena(Eval("Moneda")) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:LookupField runat="server" ID="lufMonedaItem" ControlMode="New" FieldName="Moneda" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Value='<%# Eval("Moneda") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>--%>
                            <asp:TemplateField HeaderText="Peso" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <%# this.FormatoSubcadena(Eval("Unidad medida")) %>
                                    <asp:Label runat="server" ID="lblPeso"><%# this.FormatoNumeroDecimal(Eval("Peso")) %></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:NumberField runat="server" ID="numPeso" ControlMode="New" FieldName="Peso" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Text='<%# Eval("Peso") %>' DisplaySize="5" />
                                    <SharePoint:LookupField runat="server" ID="lufUnidadMedida" ControlMode="New" FieldName="Unidad medida" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Value='<%# Eval("Unidad medida") %>' DisplaySize="5" />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Dims." HeaderStyle-HorizontalAlign="Left">
                                <ItemTemplate>
                                    <%# Eval("Dimensiones") %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:TextField runat="server" ID="txtDimensiones" ControlMode="New" FieldName="Dimensiones" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Text='<%# Eval("Dimensiones") %>' DisplaySize="20" />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Cliente" HeaderStyle-HorizontalAlign="Left">
                                <ItemTemplate>
                                    <%# this.FormatoSubcadena(Eval("Cliente")) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:LookupField runat="server" ID="lufCliente" ControlMode="New" FieldName="Cliente" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Value='<%# Eval("Cliente") %>' />
                                    <SharePoint:LookupField runat="server" ID="lufClienteAsociado" ControlMode="New" FieldName="Cliente asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Value='<%# Eval("Cliente asociado") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Urgencia" HeaderStyle-HorizontalAlign="Left">
                                <ItemTemplate>
                                    <%# this.FormatoSubcadena(Eval("Tipo pedido")) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <SharePoint:LookupField runat="server" ID="lufTipoPedido" ControlMode="New" FieldName="Tipo pedido" ListId="267F27A1-690D-4994-91DA-F927F7421756"
                                        Value='<%# Eval("Tipo pedido") %>' />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Precio ext." HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" FooterStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <%# this.FormatoNumeroDecimal(this.FormatoSubcadena(Eval("Precio extendido")).Replace('.', ',')) %>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:Label runat="server" ID="lblPrecioTotal" Text="" />
                                </FooterTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <span onclick="return confirm('¿Está seguro de querer eliminar este ítem?')">
                                        <asp:ImageButton runat="server" ID="imgEliminar" CommandName="Delete" ImageUrl="/_layouts/images/DELETE.GIF" ToolTip="Eliminar ítem" />
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>Ningún ítem agregado.</EmptyDataTemplate>
                    </SharePoint:SPGridView>
                    <asp:Label runat="server" ID="lblValidacionItems" Text="" ForeColor="Red" /><br />
                    <asp:LinkButton runat="server" ID="btnAgregarItem" CausesValidation="false"
                        ToolTip="Agregar ítem a la OC" onclick="btnAgregarItem_Click">
                        <img src="/_layouts/images/CALADD.GIF" alt="" border="0" /> Agregar ítem
                    </asp:LinkButton>
                </td>
            </tr>
            <tr><%--[Botones]--%>
                <td class="ms-toolbar" nowrap="nowrap" colspan="2">
                    <table>
                        <tr>
                            <td width="99%" class="ms-toolbar" nowrap="nowrap"><IMG SRC="/_layouts/images/blank.gif" width="1" height="18"/></td>
                            <td class="ms-toolbar" nowrap="nowrap">
                                <asp:LinkButton runat="server" ID="lnkDefinirOC" CausesValidation="false" Text="< Modificar orden"
                                    ToolTip="Regresar a la página anterior para realizar cambios en la Orden de Compra." OnClick="lnkDefinirOC_Click" />
                            </td>
                            <td class="ms-separator">&nbsp;</td>
                            <td class="ms-toolbar" nowrap="nowrap">
                                <%--<SharePoint:SaveButton runat="server" ID="btnGuardar" ControlMode="New" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />--%>
                                <asp:Button runat="server" ID="btnGuardarOC" CausesValidation="true" Text="Guardar" OnClick="btnGuardarOC_Click" CssClass="ms-ButtonHeightWidth" />
                            </td>
                            <td class="ms-separator">&nbsp;</td>
                            <td class="ms-toolbar" nowrap="nowrap" align="right">
                                <%--<SharePoint:GoBackButton runat="server" ID="btnCancelar" ControlMode="New" ListId="CB44C8D2-E5F2-4434-8E58-EADB18726C30" />--%>
                                <asp:Button runat="server" ID="btnCancelarOC2" CausesValidation="false" Text="Cancelar" OnClick="btnCancelarOC_Click" CssClass="ms-ButtonHeightWidth"
                                    OnClientClick="return confirm('¿Está seguro de querer cancelar la creación de esta orden de compra?')" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <%--Informacion Items OC--%>
    </asp:View>
    <asp:View ID="vFormItem" runat="server">
        <%--Formulario Nuevo Item--%>
        <SharePoint:SPDataSource runat="server" ID="dsItemsCatalogo" DataSourceMode="List"
             SelectCommand="<Query><OrderBy><FieldRef Name='Title' /></OrderBy></Query>">
            <SelectParameters>
                <asp:Parameter Name="WebUrl" DefaultValue="/" />
                <asp:Parameter Name="ListID" 
                    DefaultValue="9FC67FAC-6FBB-4986-B6B2-AE05557EC9F2" />
            </SelectParameters>
        </SharePoint:SPDataSource>
        <table border="0" cellspacing="0" width="100%">
            <tr><td colspan="2" class="ms-formlabel">Adición de nuevo ítem a la OC actual:</td></tr>
            <tr><%--Ítem asociado--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblItemAsociado" ControlMode="New" FieldName="Ítem asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <%--<SharePoint:LookupField runat="server" ID="lufItemAsociado" ControlMode="New" FieldName="Ítem asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756" />--%>
                    <asp:DropDownList runat="server" ID="ddlItemAsociado" AutoPostBack="true" DataSourceID="dsItemsCatalogo" AppendDataBoundItems="true"
                        DataTextField="Código ítem" DataValueField="ID" OnSelectedIndexChanged="ddlItemAsociado_SelectedIndexChanged" TabIndex="1">
                        <asp:ListItem Text="(Seleccione un producto)" Value="" />
                    </asp:DropDownList><br />
                    <asp:RequiredFieldValidator runat="server" ID="rfvtemAsociado" Text="Debe especificar un valor para este campo obligatorio." InitialValue=""
                        Display="Dynamic" ControlToValidate="ddlItemAsociado" />
                    <SharePoint:FieldDescription runat="server" ID="desItemAsociado" ControlMode="New" FieldName="Ítem asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Cantidad--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblCantidad" ControlMode="New" FieldName="Cantidad" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:NumberField runat="server" ID="numCantidad" ControlMode="New" FieldName="Cantidad" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="2" />
                    <SharePoint:FieldDescription runat="server" ID="desCantidad" ControlMode="New" FieldName="Cantidad" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Precio unitario--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblPrecioUnitario" ControlMode="New" FieldName="Precio unitario" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:NumberField runat="server" ID="numPrecioUnitario" ControlMode="New" FieldName="Precio unitario" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="3" />
                    <SharePoint:FieldDescription runat="server" ID="desPrecioUnitario" ControlMode="New" FieldName="Precio unitario" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <%--<tr> "Moneda"
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblMonedaItem" ControlMode="New" FieldName="Moneda" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:LookupField runat="server" ID="lufMonedaItem" ControlMode="New" FieldName="Moneda" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                    <SharePoint:FieldDescription runat="server" ID="desMonedaItem" ControlMode="New" FieldName="Moneda" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>--%>
            <tr><%--Peso--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblPeso" ControlMode="New" FieldName="Peso" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:NumberField runat="server" ID="numPeso" ControlMode="New" FieldName="Peso" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="4" />
                    <SharePoint:FieldDescription runat="server" ID="desPeso" ControlMode="New" FieldName="Peso" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Unidad medida--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblUnidadMedida" ControlMode="New" FieldName="Unidad medida" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:LookupField runat="server" ID="lufUnidadMedida" ControlMode="New" FieldName="Unidad medida" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="5" />
                    <SharePoint:FieldDescription runat="server" ID="desUnidadMedida" ControlMode="New" FieldName="Unidad medida" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Dimensiones--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblDimensiones" ControlMode="New" FieldName="Dimensiones" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:TextField runat="server" ID="txtDimensiones" ControlMode="New" FieldName="Dimensiones" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="6" />
                    <SharePoint:FieldDescription runat="server" ID="desDimensiones" ControlMode="New" FieldName="Dimensiones" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Cliente--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblCliente" ControlMode="New" FieldName="Cliente" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:LookupField runat="server" ID="lufCliente" ControlMode="New" FieldName="Cliente" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="7" />
                    <SharePoint:FieldDescription runat="server" ID="desCliente" ControlMode="New" FieldName="Cliente" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Cliente asociado--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblClienteAsociado" ControlMode="New" FieldName="Cliente asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:LookupField runat="server" ID="lufClienteAsociado" ControlMode="New" FieldName="Cliente asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="8" />
                    <SharePoint:FieldDescription runat="server" ID="desClienteAsociado" ControlMode="New" FieldName="Cliente asociado" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--Tipo pedido--%>
                <td width="190px" valign="top" class="ms-formlabel">
                    <SharePoint:FieldLabel runat="server" ID="lblTipoPedido" ControlMode="New" FieldName="Tipo pedido" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
                <td width="400px" valign="top" class="ms-formbody">
                    <SharePoint:LookupField runat="server" ID="lufTipoPedido" ControlMode="New" FieldName="Tipo pedido" ListId="267F27A1-690D-4994-91DA-F927F7421756" TabIndex="9" />
                    <SharePoint:FieldDescription runat="server" ID="desTipoPedido" ControlMode="New" FieldName="Tipo pedido" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                </td>
            </tr>
            <tr><%--[Botones]--%>
                <td class="ms-toolbar" nowrap="nowrap" colspan="2">
                    <table>
                        <tr>
                            <td width="99%" class="ms-toolbar" nowrap="nowrap"><IMG SRC="/_layouts/images/blank.gif" width="1" height="18"/></td>
                            <td class="ms-toolbar" nowrap="nowrap">
                                <asp:Button runat="server" ID="btnGuardarItem" CausesValidation="true" Text="Guardar" CssClass="ms-ButtonHeightWidth" onclick="btnGuardarItem_Click" TabIndex="10" />
                            </td>
                            <td class="ms-separator">&nbsp;</td>
                            <td class="ms-toolbar" nowrap="nowrap" align="right">
                                <asp:Button runat="server" ID="btnCancelarItem" CausesValidation="false" Text="Cancelar" CssClass="ms-ButtonHeightWidth" onclick="btnCancelarItem_Click" TabIndex="11" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <%--Formulario Nuevo Item--%>
    </asp:View>
</asp:MultiView>

