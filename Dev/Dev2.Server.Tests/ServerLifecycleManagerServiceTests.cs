﻿using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using WarewolfCOMIPC.Client;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class ServerLifecycleManagerServiceTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_Construct()
        {
            using (var service = new ServerLifecycleManagerService())
            {
                Assert.IsFalse(service.CanPauseAndContinue);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_Construct_Sets_ServerInteractive_False()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();
            mockServerLifeManager.SetupSet(o => o.InteractiveMode = false).Verifiable();
            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {
            }
            mockServerLifeManager.Verify();
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_OnStart_Runs_Server()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();

            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {

                serverLifecycleManagerServiceTest.TestStart();
                Assert.IsTrue(serverLifecycleManagerServiceTest.RunSuccessful);
                mockServerLifeManager.Verify(o => o.Run(It.IsAny<IEnumerable<IServerLifecycleWorker>>()), Times.Once);
            }
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_OnStop_Stops_Server()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();

            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {

                serverLifecycleManagerServiceTest.TestStop();

                mockServerLifeManager.Verify(o => o.Stop(false, 0), Times.Once);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_Dispose_Disposes_IServerLifecycleManager()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();

            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {

                serverLifecycleManagerServiceTest.TestStop();

                mockServerLifeManager.Verify(o => o.Stop(false, 0), Times.Once);
            }
            mockServerLifeManager.Verify(o => o.Dispose(), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_OpenCOMStream_Fails()
        {
            //------------------------Arrange------------------------
            var mockWriter = new Mock<IWriter>();
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };
            //------------------------Act----------------------------
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Throws(new System.Exception("The system cannot find the file specified")).Verifiable();
            using (var serverLifeCylcleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object))
            {
                serverLifeCylcleManager.Run(items);
            }
            //------------------------Assert-------------------------
            mockSerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_IsServerOnline_True()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockDirectoryHelper = new Mock<IDirectoryHelper>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName() { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });
            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                DirectoryHelper = mockDirectoryHelper.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                PauseHelper = mockPauseHelper.Object,
                StartWebServer = mockStartWebServer.Object
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items);
            }
            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Press <ENTER> to terminate service and/or web server if started"), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockSerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_IsServerOnline_False()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockDirectoryHelper = new Mock<IDirectoryHelper>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = false;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName() { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });
            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                DirectoryHelper = mockDirectoryHelper.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                PauseHelper = mockPauseHelper.Object,
                StartWebServer = mockStartWebServer.Object
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items);
            }
            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Press <ENTER> to terminate service and/or web server if started"), Times.Once);
            mockWriter.Verify(o => o.Write("Failed to start Server"), Times.Once);
            mockSerLifeCycleWorker.Verify();
        }


        class ServerLifecycleManagerServiceTest : ServerLifecycleManagerService
        {
            public ServerLifecycleManagerServiceTest(IServerLifecycleManager serverLifecycleManager)
                : base(serverLifecycleManager)
            {
            }

            public void TestStart()
            {
                OnStart(null);
            }

            public void TestStop()
            {
                OnStop();
            }
        }
    }
}