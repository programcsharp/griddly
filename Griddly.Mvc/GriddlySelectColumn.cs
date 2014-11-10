using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Griddly.Mvc
{
    public class GriddlySelectColumn : GriddlyColumn
    {
        public GriddlySelectColumn()
        {
            ClassName = "griddly-select";

            AdditionalIds = new List<Expression<Func<object, object>>>();
        }

        public Func<object, object> Id { get; set; }
        public List<Expression<Func<object, object>>> AdditionalIds { get; set; }

        public override HtmlString RenderCell(object row, bool encode = true)
        {
            TagBuilder input = new TagBuilder("input");

            input.Attributes["name"] = "_rowselect";
            input.Attributes["value"] = Id(row).ToString();
            input.Attributes["type"] = "checkbox";

            foreach (var x in this.AdditionalIds)
            {
                input.Attributes[GetPath(x)] = x.Compile()(row).ToString();
            }
            
            return new HtmlString(input.ToString(TagRenderMode.SelfClosing));
        }

        public override object RenderCellValue(object row, bool stripHtml = false)
        {
            return null;
        }

        public override string RenderClassName(object row, GriddlyResultPage page)
        {
            return null;
        }

        string GetPath(Expression<Func<object, object>> expr)
        {
            var stack = new Stack<string>();

            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            while (me != null)
            {
                stack.Push(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            // result: data-testBlah-dotBlah
            return "data-" + string.Join("-", stack.Select(x => x.Substring(0, 1).ToLower() + x.Substring(1)).ToArray());
        }
    }
}
