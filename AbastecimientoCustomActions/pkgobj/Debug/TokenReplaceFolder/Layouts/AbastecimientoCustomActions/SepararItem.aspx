<%@ Assembly Name="AbastecimientoCustomActions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e34d7846c6eccf25" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SepararItem.aspx.cs" Inherits="AbastecimientoCustomActions.Layouts.AbastecimientoCustomActions.SepararItem" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:Literal ID="ltlResultados" runat="server"></asp:Literal>
    <asp:Panel ID="pnlFormulario" runat="server">
        <table border="0" cellspacing="0" width="100%">
            <tr>
                <td width="190px" valign="top" class="ms-formlabel">Cantidad <b style="color:Red">*</b></td>
                <td width="400px" valign="top" class="ms-formbody">
                    <asp:TextBox ID="txbCantidad" runat="server" Width="100" MaxLength="5" />
                    <asp:RequiredFieldValidator ID="rfvCantidad" runat="server" Text="Debe especificar un valor para este campo obligatorio."
                        Display="Dynamic" ControlToValidate="txbCantidad" SetFocusOnError="true" />
                    <asp:RangeValidator ID="rgvCantidad" runat="server" ControlToValidate="txbCantidad" Type="Integer" Display="Dynamic"
                        Text="Debe especificar un número entero válido mayor a 0." MinimumValue="1" MaximumValue="99999" />
                    <asp:CustomValidator ID="csvCantidad" runat="server" ControlToValidate="txbCantidad" Display="Dynamic"
                        Text="Debe especificar un número mayor a 0 y menor a la cantidad original." OnServerValidate="ValidarCantidad" />
                    <br />Ingrese la cantidad de unidades a ser separados para el ítem <asp:Label ID="lblItemAccion" runat="server" />
                </td>
            </tr>
            <tr><%--[Botones]--%>
                <td class="ms-toolbar" nowrap="nowrap" colspan="2">
                    <table>
                        <tr>
                            <td width="99%" class="ms-toolbar" nowrap="nowrap"><IMG SRC="/_layouts/images/blank.gif" width="1" height="18"/></td>
                            <td class="ms-toolbar" nowrap="nowrap">
                                <asp:Button runat="server" ID="btnSepararItem" CausesValidation="true" Text="Aceptar" OnClick="btnSepararItem_Click" CssClass="ms-ButtonHeightWidth" />
                            </td>
                            <td class="ms-separator">&nbsp;</td>
                            <td class="ms-toolbar" nowrap="nowrap" align="right">
                                <SharePoint:GoBackButton runat="server" ID="btnCancelar" ControlMode="New" ListId="267F27A1-690D-4994-91DA-F927F7421756" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </asp:Panel>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Separar &iacute;tem
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Acci&oacute;n: Separar &iacute;tem
</asp:Content>
