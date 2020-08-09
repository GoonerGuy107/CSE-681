using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DocSharp {
    /// Documentation exporter
    public static partial class Design {
        /// Export the attributes for functions which have attributes
        public static bool ExportAttributes;

        /// File extensions
        public static string Extension = "html";

        /// Create documentation for a single tree node
        /// node = the position in the program tree
        /// path = the path to the current tree node
        /// depth = the depth in the tree
        static void GenerateDocumentation(TreeNode node, string path, int depth) {
            Directory.CreateDirectory(path);
            GeneratePage(path + "\\index." + Extension, node, depth);
            foreach (TreeNode child in node.Nodes) {
                if (child.Tag == null || ((ElementInfo)child.Tag).Exportable) {
                    if (((ElementInfo)child.Tag).Kind >= Element.Functions)
                        GeneratePage(path + '\\' + child.Name + '.' + Extension, child, depth);
                    else
                        GenerateDocumentation(child, path + '\\' + child.Name, depth + 1);
                }
            }
        }

        /// Start the documentation generation
        /// node = the program tree
        /// path = the path to the documentation
        public static void GenerateDocumentation(TreeNode node, string path) {
            File.WriteAllText(path + '\\' + stylesheet, style);
            GenerateDocumentation(node, path, 0);
        }

        /// Create the menu on the side of a documentation page
        /// output = the generated page content
        /// node = The node of the currently exported element
        /// child = Call with null, internally used when the recursion generates higher layers of the menu
        /// locally = The current file is to be placed in the same folder as the index
        /// path = Relative file system path to the checked node from the starting node
        static void BuildMenu(ref string output, TreeNode node, TreeNode child, bool locally, string path = "") {
            StringBuilder front = new StringBuilder();
            bool appendToFront = true;

            int indentLength = 0;
            TreeNode testNode = node;
            while ((testNode = testNode.Parent) != null)
                ++indentLength;

            foreach (TreeNode entry in node.Nodes) {
                if (((ElementInfo)entry.Tag).Exportable) {
                    string entryText = !path.Equals(string.Empty) && entry.Nodes.Count != 0 ? menuElement : menuSubelement;
                    entryText = entryText.Replace(elementMarker,
                        Utils.RemoveParamNames(((ElementInfo)entry.Tag).Name)).Replace(linkMarker, path + Utils.LocalLink(entry))
                        .Replace(indentMarker, indentLength != 0 ? "&nbsp;" + new string(' ', indentLength - 1) : string.Empty);
                    if (appendToFront) {
                        if (entry == child)
                            appendToFront = false;
                        front.Append(entryText);
                    } else
                        output += entryText;
                }
            }

            output = front.ToString() + output;
            if (node.Parent != null)
                BuildMenu(ref output, node.Parent, node, false, !locally ? "..\\" + path : path);
        }

        /// Generate the bofy of an exported page
        /// from = the target code element
        /// locally = the current file is to be placed in the same folder as the index
        static string Base(TreeNode from, bool locally) {
            string menuBuild = string.Empty;
            BuildMenu(ref menuBuild, from, null, locally);
            return baseBuild.Replace(titleMarker, from.Name)
                .Replace(menuMarker, Utils.Indent(menuBuild.Trim(), Utils.SpacesBefore(baseBuild, baseBuild.IndexOf(menuMarker))));
        }

        static bool firstEntry, evenRow;

        /// Start the generation of a content block
        static void BlockStart() {
            firstEntry = true;
            evenRow = false;
        }

        /// Add a row to a content block
        /// entry = the property or reference name
        /// description = the value or description of entry
        static void BlockAppend(StringBuilder builder, string entry, string description) {
            builder.Append(contentEntry.Replace(elementMarker, entry).Replace(contentMarker, description)
                    .Replace(subelementMarker, evenRow ? " class=\"" + evenRowClass + "\"" : string.Empty))
                    .Replace(cssMarker, firstEntry ? " class=\"" + firstColumnClass + "\"" : string.Empty);
            firstEntry = false;
            evenRow = !evenRow;
        }

        /// Generate a content block for all entries in a list
        /// nodes = the code elements
        /// title = the title of the content block
        static string ContentBlock(List<TreeNode> nodes, string title) {
            if (nodes.Count == 0)
                return string.Empty;
            nodes.Sort((TreeNode a, TreeNode b) => { return a.Name.CompareTo(b.Name); });
            StringBuilder block = new StringBuilder("<h1>").Append(title).Append(@"</h1>
<table>");
            IEnumerator enumer = nodes.GetEnumerator();
            BlockStart();
            while (enumer.MoveNext()) {
                TreeNode node = (TreeNode)enumer.Current;
                StringBuilder link = new StringBuilder(((ElementInfo)node.Tag).Type).Append(" <a href=\"").Append(Utils.LocalLink(node))
                    .Append("\">").Append(((ElementInfo)node.Tag).Name).Append("</a>");
                BlockAppend(block, link.ToString(), Utils.QuickSummary(((ElementInfo)node.Tag).Summary, node));
            }
            return block.Append(@"
</table>").ToString();
        }

        /// Generate a content block for all entries in a list, grouped by visibility
        /// nodes = the code elements
        /// titlePostfix = the postfix after visibility (e.g. functions)
        static string VisibilityContentBlock(List<TreeNode> nodes, string titlePostfix) {
            List<TreeNode>[] outs = new List<TreeNode>[(int)Visibility.Public * 2];
            for (int i = 0; i < outs.Length; ++i)
                outs[i] = new List<TreeNode>();

            IEnumerator enumer = nodes.GetEnumerator();
            while (enumer.MoveNext()) {
                TreeNode current = (TreeNode)enumer.Current;
                bool isStatic = ((ElementInfo)current.Tag).Modifiers.Contains("static");
                outs[(int)((ElementInfo)current.Tag).Vis - 1 + (isStatic ? (int)Visibility.Public : 0)].Add(current);
            }

            titlePostfix = ' ' + titlePostfix;
            StringBuilder output = new StringBuilder();
            for (int i = (int)Visibility.Public - 1; i > 0; --i) {
                output.Append(ContentBlock(outs[i], ((Visibility)(i + 1)).ToString() + titlePostfix));
                output.Append(ContentBlock(outs[i + (int)Visibility.Public], ((Visibility)(i + 1)).ToString() + " static" + titlePostfix));
            }

            return output.ToString();
        }

        /// Generate the documentation page's relevant information of the code element
        static string Content(TreeNode node) {
            List<TreeNode>[] types = new List<TreeNode>[(int)Element.Variables + 1];
            for (int i = 0; i <= (int)Element.Variables; ++i)
                types[i] = new List<TreeNode>();

            IEnumerator enumer = node.Nodes.GetEnumerator();
            while (enumer.MoveNext()) {
                TreeNode current = (TreeNode)enumer.Current;
                if (((ElementInfo)current.Tag).Exportable)
                    types[(int)((ElementInfo)current.Tag).Kind].Add(current);
            }

            StringBuilder output = new StringBuilder("<h1>");
            if (node.Tag != null)
                output.Append(((ElementInfo)node.Tag).Type).Append(' ').Append(((ElementInfo)node.Tag).Name);
            else
                output.Append(node.Name);
            output.AppendLine("</h1>");

            if (node.Tag != null) {
                ElementInfo tag = (ElementInfo)node.Tag;
                string summary = tag.Summary;
                BlockStart();
                output.AppendLine(Utils.RemoveTag(ref summary, "summary", node.Nodes.Count != 0 ? node.Nodes[0] : node)).Append("<table>");
                if (ExportAttributes && !tag.Attributes.Equals(string.Empty)) BlockAppend(output, "Attributes", tag.Attributes);
                if (tag.Vis != Visibility.Default) BlockAppend(output, "Visibility", tag.Vis.ToString());
                if (!tag.Modifiers.Equals(string.Empty)) BlockAppend(output, "Modifiers", tag.Modifiers);
                if (!tag.Extends.Equals(string.Empty)) BlockAppend(output, "Extends", tag.Extends);
                if (!tag.DefaultValue.Equals(string.Empty)) BlockAppend(output, "Default value", tag.DefaultValue);
                string returns = Utils.RemoveTag(ref summary, "returns", node);
                if (!returns.Equals(string.Empty)) BlockAppend(output, "Returns", returns);
                output.AppendLine("</table>");

                if (summary.Contains("</param>")) {
                    output.AppendLine("<h1>Parameters</h1>").AppendLine("<table>");
                    BlockStart();
                    string[] definedParams = tag.Name.Substring(tag.Name.IndexOf('(') + 1).Split(',', ')');
                    string[] parameters;
                    while ((parameters = Utils.RemoveParam(ref summary)) != null) {
                        string paramType = string.Empty;
                        foreach (string definedParam in definedParams) {
                            if (definedParam.Contains(parameters[0])) {
                                string cut = definedParam;
                                int eqPos;
                                if ((eqPos = cut.IndexOf('=')) != -1) // remove default value
                                    cut = cut.Substring(0, eqPos).Trim();
                                if (cut.EndsWith(parameters[0])) {
                                    cut = cut.Substring(0, cut.LastIndexOf(parameters[0]));
                                    string trim = cut.Trim();
                                    if (cut.Length != trim.Length) { // if it doesn't end with a whitespace, it's another param that ends the same way
                                        paramType = cut.Trim();
                                        break;
                                    }
                                }
                            }
                        }
                        BlockAppend(output, paramType + " <b>" + parameters[0] + "</b>", parameters[1]);
                    }
                    output.AppendLine("</table>");
                }
            }

            for (int i = 0; i <= (int)Element.Variables; ++i) {
                output.Append(i != (int)Element.Namespaces ? VisibilityContentBlock(types[i], ((Element)i).ToString().ToLower()) :
                    ContentBlock(types[i], "Namespaces"));
            }
            return output.ToString();
        }

        /// Export a documentation page
        /// path = the target file name
        /// site the node of the element to export
        /// depth = the depth in the file system structure relative to the export root
        static void GeneratePage(string path, TreeNode site, int depth) {
            string baseBuild = Base(site, !path.EndsWith("index." + Extension))
                .Replace(cssMarker, string.Concat(Enumerable.Repeat("..\\", depth)) + stylesheet);
            File.WriteAllText(path, baseBuild.Replace(contentMarker, Content(site)));
        }
    }
}