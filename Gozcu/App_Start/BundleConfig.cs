using System;
using System.Collections.Generic;
using System.Web.Optimization;

namespace Gozcu
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/adminjs").Include(
                "~/Content/vendor/jquery/jquery.min.js",
                "~/Content/vendor/popper.js/umd/popper.min.js",
                "~/Content/vendor/bootstrap/js/bootstrap.min.js",
                "~/Content/vendor/jquery.cookie/jquery.cookie.js",
                //"~/Content/vendor/chart.js/Chart.min.js",
                //"~/Content/js/charts-home.js",
                //"~/Content/vendor/jquery-validation/jquery.validate.min.js",  
                "~/Content/js/front.js",
                "~/Content/vendor/highchart/highcharts.js",
                "~/Content/vendor/highchart/jquery.highchartTable.js"
                //"~/Content/vendor/highchart/jquery.highchartTable-min.js"
                ));
            bundles.Add(new StyleBundle("~/bundles/admincss").Include(
                "~/Content/vendor/bootstrap/css/bootstrap.min.css",
                "~/Content/vendor/font-awesome/css/font-awesome.min.css",
                "~/Content/css/fontastic.css",
                "~/Content/css/style.green.css",
                "~/Content/css/custom.css"
            ));

            bundles.Add(new ScriptBundle("~/bundles/validatejs").Include(
                "~/Content/vendor/UnobtrusiveValidator/jquery.validate.js",
                "~/Content/vendor/UnobtrusiveValidator/jquery.validate.unobtrusive.js",
                "~/Content/vendor/UnobtrusiveValidator/jquery.unobtrusive-ajax.js"
            ));


            bundles.Add(new ScriptBundle("~/bundles/datatablesjs").Include(
                "~/Content/vendor/datatables/datatables.min.js",
                "~/Content/vendor/datatables/Responsive-2.2.2/js/dataTables.responsive.min.js",
                "~/Content/vendor/datatables/dataTables.buttons.min.js",
                "~/Content/vendor/datatables/buttons.flash.min.js",
                "~/Content/vendor/datatables/jszip.min.js",
                "~/Content/vendor/datatables/pdfmake.min.js",
                "~/Content/vendor/datatables/vfs_fonts.js",
                "~/Content/vendor/datatables/buttons.html5.min.js",
                "~/Content/vendor/datatables/buttons.print.min.js"
            ));
            bundles.Add(new StyleBundle("~/bundles/datatablescss").Include(
                "~/Content/vendor/datatables/datatables.min.css",
                "~/Content/vendor/datatables/Responsive-2.2.2/css/responsive.dataTables.min.css",
                "~/Content/vendor/datatables/buttons.dataTables.min.css"
            ));
            bundles.Add(new ScriptBundle("~/bundles/datepickerjs").Include(
                "~/Content/vendor/datepicker/js/bootstrap-datepicker.js",
                "~/Content/vendor/datepicker/bootstrap-datepicker.tr.js"
            ));
            bundles.Add(new StyleBundle("~/bundles/datepickercss").Include(
                "~/Content/vendor/datepicker/css/datepicker.css"
            ));
            bundles.Add(new ScriptBundle("~/bundles/daterangepickerjs").Include(
                "~/Content/vendor/daterangepicker/js/moment.min.js",
                "~/Content/vendor/daterangepicker/js/daterangepicker.min.js"
            ));
            bundles.Add(new StyleBundle("~/bundles/daterangepickercss").Include(
                "~/Content/vendor/daterangepicker/css/daterangepicker.css"
            ));


            bundles.Add(new ScriptBundle("~/bundles/geneljs").Include(
                "~/Content/CustomContents/js/Genel.js"
            ));
            bundles.Add(new StyleBundle("~/bundles/genelcss").Include(
                "~/Content/CustomContents/css/Genel.css"
            ));
        }
    }

    public class AsIsBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }


    public class ReplaceQueryStringBundlerResolver : IBundleResolver
    {
        private readonly IBundleResolver _resolver;

        public ReplaceQueryStringBundlerResolver(IBundleResolver resolver)
        {
            _resolver = resolver;
        }

        public IEnumerable<string> GetBundleContents(string virtualPath)
        {
            return _resolver.GetBundleContents(virtualPath);
        }

        //The important part, modifies the generated Url
        public string GetBundleUrl(string virtualPath)
        {
            var bundleUrl = _resolver.GetBundleUrl(virtualPath);
            int index = bundleUrl.IndexOf("?v=", StringComparison.Ordinal);
            if (index != -1)
            {
                bundleUrl = bundleUrl.Substring(0, index);
            }

            //if (bundleUrl.Contains("css"))
            //{
            //    if (Genel.isMobileBrowser())
            //        bundleUrl += "?v=" + Genel.CssVersion + "1";
            //    else
            //        bundleUrl += "?v=" + Genel.CssVersion;

            //}
            //else
            //{
            //    if (Genel.isMobileBrowser())
            //        bundleUrl += "?v=" + Genel.JsVersion + "1";
            //    else
            //        bundleUrl += "?v=" + Genel.JsVersion;
            //}
            return bundleUrl;
        }

        public bool IsBundleVirtualPath(string virtualPath)
        {
            return _resolver.IsBundleVirtualPath(virtualPath);
        }
    }

}
