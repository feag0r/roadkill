using Roadkill.Core.Text.Menu;
using Roadkill.Core.Text.Parsers;

namespace Roadkill.Core.Text.TextMiddleware
{
    public class MarkupParserMiddleware : Middleware
    {
        private IMarkupConverter _converter;

        public MarkupParserMiddleware(IMarkupConverter converter)
        {
            _converter = converter;
        }

        public override PageHtml Invoke(PageHtml pageHtml)
        {
            pageHtml.Html = _converter.ToHtml(pageHtml.Html);
            return pageHtml;
        }
    }
}