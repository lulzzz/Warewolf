﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces.Dropbox;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Factories;
using Dev2.Util;
using Dropbox.Api;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

namespace Dev2.Activities.DropBox2016.DropboxFileActivity
{
    //To Use this class to get all dropbox files and folders
    [ToolDescriptorInfo("DropBoxLogo", "Dropbox Files", ToolType.Native, "8999E59A-38A3-43BB-A98F-5080D1C8EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropboxFileListActivity : DsfBaseActivity
    {
        public IDropboxFactory DropboxFactory { get; private set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public virtual OauthSource SelectedSource { get; set; }

        public virtual List<string> Files { get; set; }
        public virtual List<string> Folders { get; set; }
        private DropboxClient _dropboxClient;
        public Exception Exception { get; set; }
        [FindMissing]
        public bool IncludeMediaInfo { get; set; }
        [FindMissing]
        public bool IsRecursive { get; set; }
        [FindMissing]
        public bool IncludeDeleted { get; set; }
        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string ToPath { get; set; }
        [FindMissing]
        public bool IsFilesSelected { get; set; }
        [FindMissing]
        public bool IsFoldersSelected { get; set; }
        [FindMissing]
        public bool IsFilesAndFoldersSelected { get; set; }



        public List<string> DeletedFiles { get; set; }
        [Outputs("Files and folders")]
        public List<string> FilesAndFolders { get; set; }
        public override string DisplayName { get; set; }
        private DsfDropboxFileListActivity(IDropboxFactory dropboxFactory)
        {
            DropboxFactory = dropboxFactory;
            // ReSharper disable VirtualMemberCallInContructor
            DisplayName = "List Dropbox files";
            FilesAndFolders = new List<string>();
            Files = new List<string>();
            Folders = new List<string>();
            DeletedFiles = new List<string>();
            IsFilesSelected = true;
            IncludeDeleted = false;
            IsRecursive = false;
            IncludeMediaInfo = false;
        }

        public DsfDropboxFileListActivity()
            : this(new DropboxFactory())
        {

        }

        public virtual DropboxClient GetDropboxClient()
        {
            if (_dropboxClient != null)
            {
                return _dropboxClient;
            }
            _dropboxClient = DropboxFactory.CreateWithSecret(SelectedSource.Secret);
            return _dropboxClient;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (string.IsNullOrEmpty(ToPath))
            {
                dataObject.Environment.AddError("Please confirm that the correct Dropbox file location has been entered");
                return;
            }
            base.ExecuteTool(dataObject, update);
        }

        public virtual IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
        {
            return singleExecutor;
        }
        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            var toPath = evaluatedValues["ToPath"];

            IDropboxSingleExecutor<IDropboxResult> dropboxFileRead = new DropboxFileRead(IsRecursive, toPath, IncludeMediaInfo, IncludeDeleted);
            var dropboxSingleExecutor = GetDropboxSingleExecutor(dropboxFileRead);
            var dropboxExecutionResult = dropboxSingleExecutor.ExecuteTask(GetDropboxClient());
            var dropboxSuccessResult = dropboxExecutionResult as DropboxListFolderSuccesResult;
            if (dropboxSuccessResult != null)
            {
                var listFolderResult = dropboxSuccessResult.GetListFolderResulResult();
                var metadatas = listFolderResult.Entries;
                if (IncludeDeleted)
                {
                    DeletedFiles = listFolderResult.Entries.Where(metadata => metadata.IsDeleted).Select(metadata => metadata.Name).ToList();
                }
                Files = metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.Name).ToList();
                if (IsFoldersSelected)
                    Folders = metadatas.Where(metadata => metadata.IsFolder).Select(metadata => metadata.Name).ToList();
                if (IsFilesSelected)
                    Files = metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.Name).ToList();
                if (IsFilesAndFoldersSelected)
                {
                    FilesAndFolders.AddRange(metadatas.Where(metadata => metadata.IsFolder).Select(metadata => metadata.Name).ToList());
                    FilesAndFolders.AddRange(metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.Name).ToList());
                    if (IncludeDeleted)
                        FilesAndFolders.AddRange(DeletedFiles);

                }
                return GlobalConstants.DropBoxSucces;
            }
            var dropboxFailureResult = dropboxExecutionResult as DropboxFailureResult;
            if (dropboxFailureResult != null)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException == null ? Exception.Message : Exception.InnerException.Message;

            throw new Exception(executionError);
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }
    }
}
