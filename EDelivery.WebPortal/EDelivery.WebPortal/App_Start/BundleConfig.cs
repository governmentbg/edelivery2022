using System.Web.Optimization;

namespace EDelivery.WebPortal
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterScriptBundles(bundles);

            RegisterStyleBundles(bundles);
        }

        private static void RegisterScriptBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/jquery.extensions.js",
                "~/Scripts/jquery.validate*",
                "~/Scripts/jquery.unobtrusive-ajax.js",
                "~/Scripts/custom.validate.js",
                "~/Scripts/bootstrap.bundle.min.js",
                "~/Scripts/select2.js",
                "~/Scripts/i18n/bg.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/bootstrap-datepicker.bg.js",
                "~/Scripts/alertify.js",
                "~/Scripts/jquery.lazy.min.js",
                "~/Scripts/global.functions.js",
                "~/Scripts/site.js"));

            bundles.Add(new ScriptBundle("~/bundles/file-uploader").Include(
                "~/Scripts/SimpleAjaxUploader.js",
                "~/Scripts/file.uploader.js"));

            bundles.Add(new ScriptBundle("~/bundles/uppy").Include(
                "~/Scripts/uppy.1.27.0.js",
                "~/Scripts/uppy.bg_BG.js"));
        }

        private static void RegisterStyleBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css/site").Include(
                "~/Content/css/alertify.css",
                "~/Content/css/bootstrap.css",
                "~/Content/css/datepicker.css",
                "~/Content/css/select2.css",
                "~/Content/css/style.css",
                "~/Content/css/custom.css"));
        }
    }
}
