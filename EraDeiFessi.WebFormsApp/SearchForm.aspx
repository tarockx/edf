<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchForm.aspx.cs" Inherits="EraDeiFessi.WebFormsApp.SearchForm" MasterPageFile="~/MasterPage.Master" Async="true" %>

<asp:Content ContentPlaceHolderID="mainContentPlaceholder" runat="server">
    <asp:ScriptManager runat="server" ID="searchScriptManager" AsyncPostBackTimeout="60" EnableHistory="true" OnNavigate="searchScriptManager_Navigate" />

    <div class="container">
        <asp:UpdatePanel runat="server" ID="updPanelSearch">

            <ContentTemplate>
                <asp:UpdateProgress runat="server" DynamicLayout="true" ID="updProgSearch" AssociatedUpdatePanelID="updPanelSearch">
                    <ProgressTemplate>
                        <center>
                           <div id="Progress">
                               <img src="Resources/anim_loading.gif" alt="Loading" />                       
                               <br />
                               <h3><span class="label label-primary">Caricamento in corso...</span></h3>
                               
                           </div>
                           <div id="bgDiv" />
                        </center>
                    </ProgressTemplate>
                </asp:UpdateProgress>

                <asp:MultiView ID="mainMultiView" runat="server" ActiveViewIndex="0">
                    <asp:View ID="viewSearch" runat="server">
                        <div class="row">
                            <div class="col-md-9 form-group">
                                <%--<asp:TextBox TextMode="Search" ID="txtSearchBox" placeholder="Cerca..." runat="server" CssClass="form-control"></asp:TextBox>--%>
                                <input type="text" data-provider="typeahead" placeholder="Cerca..." id="txtAutoSearchBox" class="form-control typeahead txtAutoSearchBox" runat="server"/>
                            </div>
                            <div class="col-md-3 form-group">
                                <asp:Button Text="Cerca" ID="btnPerformSearch" runat="server" OnClick="btnPerformSearch_Click" CssClass="btn btn-success btn-block" />
                            </div>
                        </div>

                        <div class="panel-group" id="accordionPlugins" runat="server">
                            <asp:Repeater ID="repeaterPlugins" runat="server" OnItemDataBound="repeaterPlugins_ItemDataBound">
                                <ItemTemplate>
                                    <div class="panel panel-default" runat="server" id="pluginDiv">
                                        <div class="panel-heading">
                                            <a data-toggle="collapse" data-parent="#accordionPlugins" href="#collapseOne<%# (Eval("pluginId") as string).Replace(".", "") %>" class="blackLink">
                                                <span class="glyphicon glyphicon-folder-open margin5"></span>
                                                <asp:Label CssClass="margin5" runat="server"><%# Eval("pluginName") %></asp:Label>
                                            </a>
                                            <span runat="server" id="spanCounterBadge" class="label label-as-badge pull-right"></span>
                                        </div>

                                        <div id="collapseOne<%# (Eval("pluginId") as string).Replace(".", "") %>" class="panel-collapse collapse">
                                            <asp:Label runat="server" Visible="false" ID="labelNoResults">Nessun risultato trovato</asp:Label>
                                            <asp:Repeater runat="server" Visible="false">
                                                <HeaderTemplate>
                                                    <ul class="list-group">
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <li runat="server" class="list-group-item"><span class="glyphicon glyphicon-film"></span>
                                                        <asp:LinkButton runat="server" OnClick="btnBookmark_Click" CssClass="blackLink"><%# Eval("Name") %></asp:LinkButton>
                                                        <span runat="server" visible="false" id="spanBookmarkInfo"><%# Eval("Name") %>;;;<%# Eval("Url") %>;;;<%#Eval("PluginID") %></span>
                                                    </li>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </ul>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>


                                </ItemTemplate>

                            </asp:Repeater>
                        </div>
                    </asp:View>

                    <asp:View ID="viewNestedResult" runat="server">
                        <%--<div class="row">
                            <div class="col-md-3">
                                <asp:Button Text="<< Indietro" ID="Button1" runat="server" OnClick="btnBackToSearchResults_Click" CssClass="btn btn-danger btn-block margin-bottom-2" />
                            </div>
                            <div class="col-md-9">
                                <asp:Label runat="server" CssClass="label-primary label-block margin-bottom-2 col-md-12" ID="lblShowTitle" />
                            </div>
                        </div>--%>
                        <asp:Label runat="server" CssClass="label-primary label-block margin-bottom-2" ID="lblShowTitle" />

                        <br />
                        <div class="panel-group" id="accordionSeasons" runat="server">
                            <asp:Repeater ID="repeaterSeasons" runat="server" OnItemDataBound="repeaterSeasons_ItemDataBound">
                                <ItemTemplate>
                                    <div class="panel panel-default" runat="server" id="seasonDiv">
                                        <div class="panel-heading">
                                            <a data-toggle="collapse" data-parent="#accordionSeasons" href="#collapseSeason<%# Container.ItemIndex + 1 %>" class="blackLink">
                                                <span class="glyphicon glyphicon-folder-open margin5"></span>
                                                <asp:Label CssClass="margin5" runat="server"><%# Eval("Title") %></asp:Label>
                                            </a>
                                            <span runat="server" id="spanCounterBadge" class="label label-as-badge pull-right"></span>
                                        </div>

                                        <div id="collapseSeason<%# Container.ItemIndex + 1 %>" class="panel-collapse collapse">
                                            <asp:Label runat="server" Visible="false" ID="labelNoResults">Nessun link disponibile</asp:Label>
                                            <asp:Repeater runat="server" ID="repeaterEpisodes" Visible="false" OnItemDataBound="repeaterEpisodes_ItemDataBound">
                                                <HeaderTemplate>
                                                    <ul class="list-group">
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <li class="list-group-item"><span class="glyphicon glyphicon-play-circle"></span>
                                                        <%--<a data-toggle="modal" href="#linksOperations" class="blackLink" onclick="moveLinks(this)">--%>
                                                        <a href="javascript:void(0)" class="blackLink" onclick="moveLinks(this)">
                                                            <asp:Label CssClass="margin5" runat="server"><%# Eval("Title") %></asp:Label>
                                                        </a>
                                                        <div class="display-none" id="linksStore">
                                                            <asp:Repeater runat="server" ID="repeaterLinks">
                                                                <HeaderTemplate>
                                                                    <ul class="list-group">
                                                                </HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <li class="list-group-item">
                                                                        <div class="row line-height-2">
                                                                            <span class="glyphicon glyphicon-link margin-horizontal-2"></span>
                                                                            <a href="<%# Eval("Url") %>" target="_blank">
                                                                                <%# libEraDeiFessi.DomainExtractor.GetDomainFromUrl(Eval("Url").ToString()) %>
                                                                            </a>

                                                                            <%--<span><%# libEraDeiFessi.DomainExtractor.GetDomainFromUrl(Eval("Url").ToString()) %></span>--%>

                                                                            <a href="https://real-debrid.com/downloader?links=<%# Uri.EscapeDataString(Eval("Url").ToString()) %>" target="_blank" class="pull-right btn-success btn-sm margin-horizontal-2">Sblocca (RD)</a>
                                                                            <%--<a href="<%# Eval("Url").ToString() %>" target="_blank" class="pull-right btn-info btn-sm margin-horizontal-2 col-xs-2">Visita</a>--%>
                                                                        </div>
                                                                    </li>
                                                                </ItemTemplate>
                                                                <FooterTemplate>
                                                                    </ul>
                                                                </FooterTemplate>
                                                            </asp:Repeater>
                                                        </div>
                                                    </li>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </ul>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>


                                </ItemTemplate>

                            </asp:Repeater>
                        </div>

                        <asp:Button Text="<< Indietro" ID="btnBackToSearchResults" runat="server" OnClick="btnBackToSearchResults_Click" CssClass="btn btn-danger btn-block margin-bottom-2" />

                        <br />
                        <br />
                        <br />
                    </asp:View>

                    <asp:View ID="viewHtmlResult" runat="server">
                        <asp:Label runat="server" CssClass="label-primary label-block margin-bottom-2" ID="lblHtmlContentTitle" />

                        <br />
                        <div class="label-block label-warning square-bottom-corners">Link originali</div>

                        <div runat="server" id="divHtmlContentLinks" class="well square-top-corners"></div>

                        <div class="label-block label-success square-bottom-corners">Sblocca link (redirect su Real-Debrid)</div>

                        <div runat="server" id="divHtmlContentLinksRD" class="well square-top-corners"></div>

                        <asp:Button Text="<< Indietro" runat="server" OnClick="btnBackToSearchResults_Click" CssClass="btn btn-danger btn-block margin-bottom-2" />
                    </asp:View>
                </asp:MultiView>

                <br /><br /><br /><br /><br />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <div id="linksOperations" class="modal fade">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <asp:Label runat="server" class="label-block label-primary">Link disponibili</asp:Label>
                </div>

                <div class="modal-body" id="linksOperationsBody">
                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-warning btn-block" data-dismiss="modal">Chiudi</button>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        $("#<%=updPanelSearch.ClientID%>").addClass("fullHeight");
        $("#<%=updProgSearch.ClientID%>").addClass("fullHeight");
    </script>
</asp:Content>
