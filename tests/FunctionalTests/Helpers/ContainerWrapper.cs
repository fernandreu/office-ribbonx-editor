using Autofac;
using Moq;
using NUnit.Framework;
using OfficeRibbonXEditor.Helpers;
using OfficeRibbonXEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OfficeRibbonXEditor.FunctionalTests.Helpers
{
    public class ContainerWrapper : IDisposable
    {
        public RedistributableDetails RedistributableDetails { get; set; } = new("v1.2.3", "x86", new Uri("http://localhost"))
        {
            NeedsDownload = false,
        };

        public string? FileToBeOpened { get; set; }

        public IEnumerable<string>? FilesToBeOpened { get; set; }

        public string? FileToBeSaved { get; set; }

        public Func<string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult>? ShowMessageFunc { get; set; }

        public Action<Uri>? OpenExternalAction { get; set; }

        private IContainer? _container;
        public IContainer Container => _container ??= CreateContainer();

        public static DragData CreateDragData(string dataFormat, object? data)
        {
            var dataMock = new Mock<IDataObject>();
            dataMock.Setup(x => x.GetDataPresent(It.IsAny<string>()))
                .Returns<string>(x => x == dataFormat);
            dataMock.Setup(x => x.GetData(It.IsAny<string>()))
#pragma warning disable CS8603 // Possible null reference return. This is a problem with the IDataObject interface; null is a perfectly valid return value
                .Returns<string>(x => x != dataFormat ? null : data);
#pragma warning restore CS8603 // Possible null reference return.
            return new DragData(dataMock.Object);
        }

        public void AssertMessage(Action action, MessageBoxImage image, MessageBoxResult result = MessageBoxResult.OK, string message = "Message not shown")
        {
            var count = 0;
            ShowMessageFunc = (text, caption, button, funcImage) =>
            {
                if (funcImage == image)
                {
                    count++;
                }

                return result;
            };
            action();
            ShowMessageFunc = null;
            Assert.AreEqual(1, count, message);
        }

        private IContainer CreateContainer()
        {
            var builder = App.CreateContainerBuilder();
            builder.RegisterInstance(CreateMessageBoxService());
            builder.RegisterInstance(CreateFileDialogService());
            builder.RegisterInstance(CreateVersionChecker());
            builder.RegisterInstance(CreateUrlHelper());
            return builder.Build();
        }

        private IMessageBoxService CreateMessageBoxService()
        {
            var mock = new Mock<IMessageBoxService>();
            mock.Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                .Returns<string, string, MessageBoxButton, MessageBoxImage>((text, caption, button, image) => ShowMessageFunc?.Invoke(text, caption, button, image) ?? MessageBoxResult.OK);
            return mock.Object;
        }

        private IFileDialogService CreateFileDialogService()
        {
            var mock = new Mock<IFileDialogService>();
            mock.Setup(x => x.OpenFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) =>
                {
                    if (FileToBeOpened != null)
                    {
                        action?.Invoke(FileToBeOpened);
                    }
                });
            mock.Setup(x => x.OpenFilesDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<IEnumerable<string>>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<IEnumerable<string>>, string, int>((title, filter, action, fileName, filterIndex) =>
                {
                    if (FilesToBeOpened != null)
                    {
                        action?.Invoke(FilesToBeOpened);
                    }
                });
            mock.Setup(x => x.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true)
                .Callback<string, string, Action<string>, string, int>((title, filter, action, fileName, filterIndex) =>
                {
                    if (FileToBeSaved != null)
                    {
                        action?.Invoke(FileToBeSaved);
                    }
                });
            return mock.Object;
        }

        private IVersionChecker CreateVersionChecker()
        {
            var mock = new Mock<IVersionChecker>();
            mock.Setup(x => x.CheckRedistributableVersion())
                .Returns(RedistributableDetails);
            return mock.Object;
        }

        private IUrlHelper CreateUrlHelper()
        {
            var mock = new Mock<IUrlHelper>();
            mock.Setup(x => x.OpenExternal(It.IsAny<Uri>()))
                .Callback<Uri>(uri => OpenExternalAction?.Invoke(uri));
            return mock.Object;
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _container?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
