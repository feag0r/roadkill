using System;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using Roadkill.Core.Text.CustomTokens;
using Roadkill.Core.Text.Parsers;
using Roadkill.Core.Text.Parsers.Images;
using Roadkill.Core.Text.Parsers.Links;
using Roadkill.Core.Text.Parsers.Markdig;
using Roadkill.Core.Text.Plugins;
using Roadkill.Core.Text.Sanitizer;
using Roadkill.Core.Text.TextMiddleware;
using StructureMap;
using StructureMap.Graph;

namespace Roadkill.Core.DependencyResolution.StructureMap.Registries
{
    public class TextRegistry : Registry
    {
        public TextRegistry()
        {
            Scan(ScanTypes);
            ConfigureInstances();
        }

        private void ScanTypes(IAssemblyScanner scanner)
        {
            scanner.TheCallingAssembly();
            scanner.SingleImplementationsOfInterface();
            scanner.WithDefaultConventions();

            scanner.AddAllTypesOf<CustomTokenParser>();
        }

        private void ConfigureInstances()
        {
            For<IPluginFactory>().Use<PluginFactory>();
            WireupMarkdigParser();
            For<IHtmlSanitizerFactory>().Use<HtmlSanitizerFactory>();

            For<TextMiddlewareBuilder>()
                .AlwaysUnique()
                .Use("TextMiddlewareBuilder", ctx =>
                {
                    var builder = new TextMiddlewareBuilder();

                    var textPluginRunner = ctx.GetInstance<TextPluginRunner>();
                    var markupParser = ctx.GetInstance<IMarkupParser>();
                    var htmlSanitizerFactory = ctx.GetInstance<IHtmlSanitizerFactory>();
                    var customTokenParser = ctx.GetInstance<CustomTokenParser>();

                    builder.Use(new TextPluginBeforeParseMiddleware(textPluginRunner));
                    builder.Use(new MarkupParserMiddleware(markupParser));
                    builder.Use(new HarmfulTagMiddleware(htmlSanitizerFactory));
                    builder.Use(new CustomTokenMiddleware(customTokenParser));
                    builder.Use(new TextPluginAfterParseMiddleware(textPluginRunner));

                    return builder;

                }).Singleton();
        }

        private void WireupMarkdigParser()
        {
			For<IMarkupParser>().Use("MarkdigParser", ctx =>
			{
				Func<HtmlImageTag, HtmlImageTag> imageTagParsed = OnImageParsed(ctx);
				Func<HtmlLinkTag, HtmlLinkTag> linkParsed = OnLinkParsed(ctx);

				var parser = new MarkdigParser();
				parser.ImageParsed = imageTagParsed;
				parser.LinkParsed = linkParsed;

				return parser;
			});
        }

		private Func<HtmlLinkTag, HtmlLinkTag> OnLinkParsed(IContext ctx)
		{
			// Link parsing callback
			return (htmlImageTag) =>
			{
				var pageRepository = ctx.GetInstance<IPageRepository>();
				var applicationSettings = ctx.GetInstance<ApplicationSettings>();
				var urlHelper = ctx.GetInstance<UrlHelper>();

				var tokenParser = new LinkHrefParser(pageRepository, applicationSettings, urlHelper);
				htmlImageTag = tokenParser.Parse(htmlImageTag);

				return htmlImageTag;
			};
		}

		private Func<HtmlImageTag, HtmlImageTag> OnImageParsed(IContext ctx)
		{
			// Image parsing callback
			return (htmlImageTag) =>
			{
				var appSettings = ctx.GetInstance<ApplicationSettings>();
				var urlHelper = ctx.GetInstance<UrlHelper>();

				var provider = new ImageHrefParser(appSettings, urlHelper);
				htmlImageTag = provider.Parse(htmlImageTag);

				return htmlImageTag;
			};
		}
	}
}
