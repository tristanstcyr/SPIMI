<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Clusteroo
</asp:Content>



<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="width: 100%;">
        <br />
        <input id="query" type="text" />
        <input id="queryBtn" type="submit" value="Query" />
        &nbsp;
        or
        &nbsp;
        <input id="cluster" type="text" value="10"/>
        <input id="clusterBtn" type="submit" value="Cluster" />
        <br />
        <br />
        <div id="resultsTitle"></div>
        <table id="queryResults">
        </table>
    </div>

    
</asp:Content>

<asp:Content ContentPlaceHolderID="BottomScripts" ID="scripts" runat="server">
<script type="text/javascript">
    function clearQueryResults() {
        $("#resultsTitle").text("");
        $("#queryResults").children().remove();
    }

    function addQueryResult(rank, title, uri, score) {
        $("#queryResults").append("<tr><td>" + rank + ".</td><td>" + title + "</td><td>" + score.toString().substring(0,4) + "</td><td><a href=\"" + uri + "\">" + uri + "<a></td></tr>");
    }

    function getQueryResults() {
        clearQueryResults();
        var queryParam = $("input#query").attr("value");
        $.getJSON("/Home/Query", { query: queryParam }, function (data) {
            if (data.length == 0) {
                $("#resultsTitle").text("No result");
            }
            $.each(data, function (index, res) {
                $("#resultsTitle").text("Query results (top 25)");
                addQueryResult(index + 1, res.Title, res.Uri, res.Score);
            });
        });
    }

    function getClusterResults() {
        clearQueryResults();
        var clusterParam = $("input#cluster").attr("value");
        $.getJSON("/Home/Cluster", { k: clusterParam }, function (data) {
            $.each(data, function (index, res) {
                $("#resultsTitle").text("Cluster results (top terms and documents)");
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
        });
    }

    $("#queryBtn").click(getQueryResults);
    $("#clusterBtn").click(getClusterResults);
    </script>
 </asp:Content>
