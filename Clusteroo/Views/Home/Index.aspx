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
            <input id="indexBtn" type="submit" value="Index" />
            <br />
            &nbsp;
            &nbsp;
            &nbsp;
            <br />
            then
            <br />
            &nbsp;
            &nbsp;
            &nbsp;
            <br />
            <input id="query" type="text" />
            <select id="ranking">
              <option value="tf-idf">tf-idf</option>
              <option value="BM25">BM25</option>
            </select>
            <input id="queryBtn" type="submit" value="Query" />
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            or
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            &nbsp;
            <input id="cluster" type="text" value="10" style="width: 100px"/>
            <input id="clusterBtn" type="submit" value="Cluster" />
            <br />
            <br />
        </div>
        <br />
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
        $("#queryResults").hide();
    }

    function showQueryResults() {
        $("#queryResults").show();
    }

    function addQueryResultsHeader() {
        $("#queryResults").append("<tr><th>Rank</th><th>Title</th><th>Score</th><th>Link</th></tr>");
    }

    function enableFields(enable) {
        if (enable) {
            $("input, select").removeAttr("disabled");
        }
        else {
            $("input, select").attr("disabled", "disabled");
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
            showQueryResults();
        });
    }

    function getQueryResults() {
        enableFields(false);
        clearQueryResults();
        var queryParam = $("input#query").attr("value");
        var ranking = $("select option:selected").attr("value");
        addQueryResultsHeader(ranking);
        var start = new Date().getTime();
        $.getJSON("/Home/Query", { query: queryParam, rankingMode: ranking }, function (data) {

            $.each(data, function (index, res) {
                var elapsed = new Date().getTime() - start;
                var hits = data.length;
                var hitsString = hits.toString();
                if (hits == 500) {
                    hitsString = "top 500";
                }
                $("#resultsTitle").text("Query results (" + hitsString + " hits, "
                                                + (elapsed / 1000).toString().substring(0, 4) + " seconds, " 
                                                + $("select option:selected").attr("value") + ")");

                addQueryResult(index + 1, res.Title, res.Uri, res.Score);
            });
            enableFields(true);
            if (data.length == 0) {
                $("#resultsTitle").text("No hit");
            } else {
                showQueryResults();
            }
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
            showQueryResults();
        });
    }

    $("#queryBtn").click(getQueryResults);
    $("#clusterBtn").click(getClusterResults);
    $("#indexBtn").click(launchIndex);
    if ($("#okToQuery").text() == " True ") {
        enableFields(true);
    }
    else {
        $("#cluster, #clusterBtn, #query, #queryBtn, #ranking").attr("disabled", "disabled");
    }
    </script>
 </asp:Content>
