﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Update_List_Item
    {
        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void Sharepoint_Update_List_Item_UITest()
        {
            UIMap.Open_Sharepoint_Update_List_Item_Tool_Large_View();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.DataGridTableVariables.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.DataGridTableVariables.ItemRow1.MatchTypeCell.MatchTypeComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.DataGridTableVariables.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.DataGridTableValues.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.DataGridTableValues.ItemRow1.MatchTypeCell.MatchTypeComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.DataGridTableValues.ItemRow1.ValueCell.ValueComboBox.Exists);
        }

        [TestMethod]
        [TestCategory("Sharepoint Tools")]
        public void NewSource_From_SharepointUpdateListItemToolUITest()
        {
            UIMap.Click_NewSource_From_SharepointUpdateListItemTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Enabled, "Server Name Textbox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Enabled, "Windows Radio button is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Enabled, "User Radio button is not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton.Enabled, "Test Connection button is enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.CancelTestButton.Enabled, "Cancel Test button is  enabled.");
            UIMap.Click_Close_SharepointSource_Tab_Button();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
