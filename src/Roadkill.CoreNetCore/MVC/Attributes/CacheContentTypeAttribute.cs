using System.Web.Mvc;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Over-rides the OutputCache so it doesn't force text/html
	/// </summary>
	public class CacheContentTypeAttribute : OutputCacheAttribute
	{
		public string ContentType { get; set; }

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);

			ContentType = ContentType ?? "text/html";
			filterContext.HttpContext.Response.ContentType = ContentType;
		}
	}
}