<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginForm.aspx.cs" Inherits="EraDeiFessi.WebFormsApp.LoginForm" MasterPageFile="~/MasterPage.Master" %>

<asp:Content ContentPlaceHolderID="mainContentPlaceholder" runat="server" ID="mainContainer">
    <div class="container">
        <asp:Label runat="server" CssClass="label-primary label-block margin-bottom-2" ID="lblHtmlContentTitle">
        Autenticazione richiesta
        </asp:Label>

        <asp:Label runat="server" CssClass="label-danger label-block margin-bottom-2" ID="lblWrongCredentials" Visible="false">
        Nome utente o password errati. Riprovare
        </asp:Label>

        <br />

        <div class="input-group input-group-width-fix">
            <span class="input-group-addon input-addon-size-fix">Nome utente</span>
            <input id="txtUser" type="text" class="form-control" required="required" runat="server"/>
        </div>

        <div class="input-group input-group-width-fix">
            <span class="input-group-addon input-addon-size-fix">Password</span>
            <input id="txtPassword" type="password" class="form-control" required="required" runat="server"/>
        </div>

        <br />
        <asp:CheckBox runat="server" Text="Ricorda le mie credenziali" CssClass="form-control" ID="chkRemember" Checked="true"/>

        <br />

        <asp:Button Text="Login" runat="server" OnClick="Logon_Click" CssClass="btn btn-success btn-block" />

    </div>


</asp:Content>
