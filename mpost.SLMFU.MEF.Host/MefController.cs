using System;
using System.ComponentModel.Composition;
using System.Windows;
using mpost.Silverlight.MEF.Interfaces;
using mpost.SilverlightMultiFileUpload;
using mpost.SilverlightMultiFileUpload.Contracts;

namespace mpost.SLMFU.MEF.Host
{
    public class MefController : IPartImportsSatisfiedNotification
    {
        private readonly Page _mainPage;

        public MefController()
        {
            CompositionInitializer.SatisfyImports(this);

            _mainPage = new Page();
            RootVisual = _mainPage;

            CatalogService.AddXap("mpost.SLMFU.MEF.Plugins.Thumbnails.xap");
        }

        [Import(AllowRecomposition = true, AllowDefault = true)]
        public Lazy<IVisualizeFileRow> VisualizeFileRowControl { get; set; }

        [Import]
        public IDeploymentCatalogService CatalogService { get; set; }

        public UIElement RootVisual { get; set; }

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            //Create the template to show individual file rows
            if (VisualizeFileRowControl != null)
            {
                _mainPage.SetRowTemplate(VisualizeFileRowControl.Value.GetType());
            }
        }

        #endregion
    }
}