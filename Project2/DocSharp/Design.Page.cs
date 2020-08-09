﻿namespace DocSharp {
    // HTML design of the exporter
    public static partial class Design {
        // Replace markers for the exporter
        const string
            titleMarker = "<!--title-->",
            menuMarker = "<!--menu-->",
            cssMarker = "<!--css-->",
            elementMarker = "<!--elem-->",
            linkMarker = "<!--link-->",
            indentMarker = "<!--indent-->",
            subelementMarker = "<!--sub-->",
            contentMarker = "<!--content-->";

        /// Path to the stylesheet
        const string stylesheet = "style.css";

        // Class names in the stylesheet
        const string
            mainTableClass = "mt",
            menuTdClass = "t1",
            firstColumnClass = "t2",
            evenRowClass = "sr";

        /// Parent menu element
        const string menuElement = @"
<h2>" + indentMarker + "<a href=\"" + linkMarker + "\">" + elementMarker + "</a></h2>";
        /// Menu element on the same level as the opened module
        const string menuSubelement = @"
<h3>" + indentMarker + "<a href=\"" + linkMarker + "\">" + elementMarker + "</a></h3>";

        /// Boilerplate of a page
        const string baseBuild = @"<html>
    <head>
        <title>" + titleMarker + @"</title>
        <link rel=""stylesheet"" href=""" + cssMarker + @""" type=""text/css"">
      </head>
    <body>
        <table class=""" + mainTableClass + @""">
            <tr>
                <td class=""" + menuTdClass + @""">
                    " + menuMarker + @"
                </td>
                <td>
                    " + contentMarker + @"
                </td>
            </tr>
        </table>
    </body>
</html>";

        /// List row
        const string contentEntry = @"
    <tr" + subelementMarker + @">
        <td" + cssMarker + ">" + elementMarker + @"</td>
        <td>" + contentMarker + @"</td>
    </tr>";

        /// Stylesheet of the documentation
        const string style =
@"." + mainTableClass + @" { height: 100%; }
." + evenRowClass + @" { background-color: #EEEEEE; }
." + menuTdClass + @" { width: 250px; }
." + firstColumnClass + @" { width: 350px; }
a:link { color: red; text-decoration: none; }
a:visited { color: red; text-decoration: none; }
a:active { color: red; text-decoration: underline; }
a:hover { color: red; text-decoration: underline; }
h1 {
  font-size: 24px;
  margin: 0;
  margin-bottom: 6px;
}
h2 {
  font-size: 16px;
  margin: 0;
}
h3 {
  font-size: 14px;
  margin: 0;
}
html, body {
  font-family: Verdana;
  height: 100%;
  margin: 0;
}
table {
  border: none;
  padding-bottom: 8px;
  width: 100%;
}            
table tr td {
  text-align: left;
  vertical-align: top;
}";
    }
}
