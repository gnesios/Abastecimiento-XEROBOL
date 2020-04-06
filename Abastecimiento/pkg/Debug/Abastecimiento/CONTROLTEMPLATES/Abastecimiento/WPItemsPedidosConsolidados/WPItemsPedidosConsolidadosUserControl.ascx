<%@ Assembly Name="Abastecimiento, Version=1.0.0.0, Culture=neutral, PublicKeyToken=8edd06d77339fe05" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WPItemsPedidosConsolidadosUserControl.ascx.cs" Inherits="Abastecimiento.WPItemsPedidosConsolidados.WPItemsPedidosConsolidadosUserControl" %>

<SharePoint:SPDataSource runat="server" ID="dtsItemsPedidos" DataSourceMode="List" UseInternalName="true"
    SelectCommand="<Query><OrderBy><FieldRef Name='ID' Ascending='false' /></OrderBy></Query>">
    <SelectParameters>
        <asp:Parameter Name="WebUrl" DefaultValue="/" />
        <asp:Parameter Name="ListID" DefaultValue="267F27A1-690D-4994-91DA-F927F7421756" />
    </SelectParameters>
</SharePoint:SPDataSource>
<SharePoint:SPGridView runat="server" ID="grvItemsPedidosConsolidados" AutoGenerateColumns="false" 
    DataKeyNames="ID" ShowHeader="true" ShowFooter="true" DataSourceID="dtsItemsPedidos"
    RowStyle-BackColor="#DDDDDD" AlternatingRowStyle-BackColor="#EEEEEE">
    <Columns>
        <asp:BoundField DataField="ID" HeaderText="ID" />
        <asp:BoundField DataField="Title" HeaderText="Título" />
        <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
        <asp:BoundField DataField="Precio_x0020_unitario" HeaderText="p/u" />
        <asp:BoundField DataField="Precio_x0020_extendido" HeaderText="Total" />
    </Columns>
</SharePoint:SPGridView>
