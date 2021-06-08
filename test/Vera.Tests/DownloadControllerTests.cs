using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vera.Host.Controllers;
using Vera.Host.Security;
using Vera.Models;
using Vera.Stores;
using Xunit;

namespace Vera.Tests
{
    public class DownloadControllerTests
    {
        [Fact]
        public async Task Should_return_not_found()
        {
            const string blobName = "hello";
            var accountId = Guid.NewGuid().ToString();

            var store = new Mock<IBlobStore>();
            store.Setup(x => x.Read(It.IsAny<Guid>(), blobName))
                .Returns(Task.FromResult<Blob>(null));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(MetadataKeys.AccountId, accountId);

            var controller = new DownloadController(store.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await controller.Download(blobName);

            Assert.True(result is NotFoundObjectResult);
        }

        [Fact]
        public async Task Should_return_blob()
        {
            const string blobName = "hello";
            var accountId = Guid.NewGuid().ToString();
            var expectedContent = Encoding.ASCII.GetBytes("HELLO");

            var expected = new Blob
            {
                MimeType = "text/xml",
                Content = new MemoryStream(expectedContent)
            };

            var store = new Mock<IBlobStore>();
            store.Setup(x => x.Read(It.IsAny<Guid>(), blobName))
                .Returns(Task.FromResult(expected));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add(MetadataKeys.AccountId, accountId);

            var controller = new DownloadController(store.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await controller.Download(blobName);
            var response = result as FileStreamResult;

            Assert.NotNull(response);
            Assert.Equal(expected.MimeType, response.ContentType);

            var gotContent = new byte[expectedContent.Length];
            response.FileStream.Read(gotContent);

            Assert.Equal(expectedContent, gotContent);
        }
    }
}
