using System.Web;
using System.Web.Optimization;

namespace InventoryManagementSystem
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/plugins/css").Include(
                      "~/plugins/fontawesome-free/css/all.min.css",
                      "~/plugins/tempusdominus-bootstrap-4/css/tempusdominus-bootstrap-4.min.css",
                      "~/plugins/icheck-bootstrap/icheck-bootstrap.min.css",
                      "~/plugins/jqvmap/jqvmap.min.css", 
                      "~/plugins/daterangepicker/daterangepicker.css",
                      "~/plugins/summernote/summernote-bs4.min.css",
                      "~/plugins/overlayScrollbars/css/OverlayScrollbars.min.css")); 


            bundles.Add(new StyleBundle("~/dist/css/adminlte").Include(
                      "~/dist/css/adminlte.min.css"));
             
        }
    }
}
