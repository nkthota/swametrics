using System.Web;
using System.Web.Optimization;

namespace SWAMetrics
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {   

            // Homer style
            bundles.Add(new StyleBundle("~/bundles/homer/css").Include(
                      "~/Content/style.css", new CssRewriteUrlTransform()));

            // Homer script
            bundles.Add(new ScriptBundle("~/bundles/homer/js").Include(
                      "~/Vendor/metisMenu/dist/metisMenu.min.js",
                      "~/Scripts/homer.js"));

            // Animate.css
            bundles.Add(new StyleBundle("~/bundles/animate/css").Include(
                      "~/Vendor/animate.css/animate.min.css"));

            // Pe-icon-7-stroke
            bundles.Add(new StyleBundle("~/bundles/peicon7stroke/css").Include(
                      "~/Icons/pe-icon-7-stroke/css/pe-icon-7-stroke.css", new CssRewriteUrlTransform()));

            // Font Awesome icons style
            bundles.Add(new StyleBundle("~/bundles/font-awesome/css").Include(
                      "~/Vendor/fontawesome/css/font-awesome.min.css", new CssRewriteUrlTransform()));

            // Bootstrap style
            bundles.Add(new StyleBundle("~/bundles/bootstrap/css").Include(
                      "~/Vendor/bootstrap/dist/css/bootstrap.min.css", new CssRewriteUrlTransform()));

            // Bootstrap
            bundles.Add(new ScriptBundle("~/bundles/bootstrap/js").Include(
                      "~/Vendor/bootstrap/dist/js/bootstrap.min.js"));

            // jQuery
            bundles.Add(new ScriptBundle("~/bundles/jquery/js").Include(
                      "~/Vendor/jquery/dist/jquery.min.js"));

            // jQuery UI
            bundles.Add(new ScriptBundle("~/bundles/jqueryui/js").Include(
                      "~/Vendor/jquery-ui/jquery-ui.min.js"));

            // jQuery Validation
            bundles.Add(new ScriptBundle("~/bundles/validation/js").Include(
                      "~/Vendor/jquery-validation/jquery.validate.min.js"));

            // Datatables
            bundles.Add(new ScriptBundle("~/bundles/datatables/js").Include(
                "~/Vendor/datatables/media/js/jquery.dataTables.min.js"));

            // Datatables bootstrap
            bundles.Add(new ScriptBundle("~/bundles/datatablesBootstrap/js").Include(
                "~/Vendor/datatables.net-bs/js/dataTables.bootstrap.min.js"));

            // Datatables plugins
            bundles.Add(new ScriptBundle("~/bundles/datatablesPlugins/js").Include(
                "~/Vendor/pdfmake/build/pdfmake.min.js",
                "~/Vendor/pdfmake/build/vfs_fonts.js",
                "~/Vendor/datatables.net-buttons/js/buttons.html5.min.js",
                "~/Vendor/datatables.net-buttons/js/buttons.print.min.js",
                "~/Vendor/datatables.net-buttons/js/dataTables.buttons.min.js",
                "~/Vendor/datatables.net-buttons-bs/js/buttons.bootstrap.min.js"));

            // Datatables style
            bundles.Add(new StyleBundle("~/bundles/datatables/css").Include(
                "~/Vendor/datatables.net-bs/css/dataTables.bootstrap.min.css"));

        }

    }
}
