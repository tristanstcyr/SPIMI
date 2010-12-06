<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Clusteroo
</asp:Content>



<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div>
        <div id="indexStats">
                
        </div>
        <div id="inputs">
            <br />
            <input id="indexSite" type="text" value="www.encs.concordia.ca" />
            <input id="indexBtn" type="submit" value="Run index" />
            &nbsp;
            then
            &nbsp;
            <input id="query" type="text" />
            <input id="queryBtn" type="submit" value="Query" />
            &nbsp;
            or
            &nbsp;
            <input id="cluster" type="text" value="10"/>
            <input id="clusterBtn" type="submit" value="Cluster" />
            <br />
            <br />
        </div>
        <div id="resultsTitle"></div>
        <table id="queryResults">
        </table>

    </div>

    <div id="okToQuery" style="display:none;"> <%: ViewData["OkToQuery"] %> </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="BottomScripts" ID="scripts" runat="server">
<script type="text/javascript">
    function clearQueryResults() {
        $("#resultsTitle").text("");
        $("#queryResults").children().remove();
    }

    function enableFields(enable) {
        if (enable) {
            $("input").removeAttr("disabled");
        }
        else {
            $("input").attr("disabled", "disabled");
        }
    }

    function addQueryResult(rank, title, uri, score) {
        $("#queryResults").append("<tr><td>" + rank + ".</td><td>" + title + "</td><td>" + score.toString().substring(0,4) + "</td><td><a href=\"" + uri + "\">" + uri + "<a></td></tr>");
    }

    function launchIndex() {
        enableFields(false);
        $("#indexStats").children().remove();
        clearQueryResults();
        var siteParam = $("input#indexSite").attr("value");
        $.getJSON("/Home/Reindex", { site: siteParam }, function (data) {
            $("#indexStats").append("<div>" + data.CollectionSize + " documents indexed in " + (data.IndexingTime / 1000).toString().substring(0, 4) + " seconds</div>");
            enableFields(true);
        });
    }

    function getQueryResults() {
        enableFields(false);
        clearQueryResults();
        var queryParam = $("input#query").attr("value");
        var start = new Date().getTime();
        $.getJSON("/Home/Query", { query: queryParam }, function (data) {
            if (data.length == 0) {
                $("#resultsTitle").text("No result");
            }
            $.each(data, function (index, res) {
                var elapsed = new Date().getTime() - start;
                $("#resultsTitle").text("Query results (top 500 or less, " + (elapsed/1000).toString().substring(0,4) + " seconds)");
                addQueryResult(index + 1, res.Title, res.Uri, res.Score);
            });
            enableFields(true);
        });
    }


    function getClusterResults() {
        enableFields(false);
        clearQueryResults();
        var clusterParam = $("input#cluster").attr("value");
        var start = new Date().getTime();
        $.getJSON("/Home/Cluster", { k: clusterParam }, function (data) {
            $.each(data, function (index, res) {
                var elapsed = new Date().getTime() - start;
                $("#resultsTitle").text("Cluster results (top terms and documents, " + (elapsed/1000).toString().substring(0,4) + " seconds)");
                var rowText = "<tr>";
                rowText = rowText + "<td>" + (index + 1) + ".</td>";
                rowText = rowText + "<td>";
                $.each(res.TopTerms, function (topTermIndex, topTerm) {
                    rowText = rowText + "&nbsp;&nbsp;" + topTerm + "&nbsp;&nbsp;";
                });
                rowText = rowText + "</td>"
                rowText = rowText + "</tr>";
                rowText = rowText + "<tr><td></td><td>";
                $.each(res.DocUris, function (docUriIndex, docUri) {
                    rowText = rowText + "<div>" + (docUriIndex+1) + ".&nbsp;<a href=\"" + docUri + "\" >"+ docUri +"</a></div>";
                });    
                rowText = rowText + "</td>"
                rowText = rowText + "</tr>";
                $("#queryResults").append(rowText);
                
            });
            enableFields(true);
        });
    }

    $("#queryBtn").click(getQueryResults);
    $("#clusterBtn").click(getClusterResults);
    $("#indexBtn").click(launchIndex);
    if ($("#okToQuery").text() == " True ") {
        enableFields(true);
    }
    else {
        $("#cluster, #clusterBtn, #query, #queryBtn").attr("disabled", "disabled");
    }
    </script>
 </asp:Content>
