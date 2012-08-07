namespace Ninject.Extensions.WebApi.UsageLogger.Handlers
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using Ninject.Extensions.Logging;

    /// <summary>
    /// Usage logger handler
    /// </summary>
    public class UsageHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsageHandler" /> class.
        /// </summary>
        public UsageHandler()
        {
            var kernel = new StandardKernel();

            var logfactory = kernel.Get<ILoggerFactory>();

            this.Log = logfactory.GetCurrentClassLogger();
        }

        /// <summary>
        /// Gets or sets the max HTTP content length.
        /// </summary>
        /// <value>
        /// The max HTTP content length.
        /// </value>
        public int MaxContentLength { get; set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Log { get; private set; }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>. The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request"/> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            // Log request
            await request.Content.ReadAsStringAsync().ContinueWith(c =>
                {
                    this.Log.Info("{0}: {1} called from {2}", request.Method, HttpUtility.UrlDecode(request.RequestUri.AbsoluteUri), ((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress);
                    this.Log.Info("Content-Type: {0}, Content-Length: {1}", request.Content.Headers.ContentType != null ? request.Content.Headers.ContentType.MediaType : string.Empty, request.Content.Headers.ContentLength);
                    this.Log.Info("Accept-Encoding: {0}, Accept-Charset: {1}, Accept-Language: {2}", request.Headers.AcceptEncoding, request.Headers.AcceptCharset, request.Headers.AcceptLanguage);

                    if (!string.IsNullOrEmpty(c.Result))
                    {
                        if (this.MaxContentLength > 0 && c.Result.Length > this.MaxContentLength)
                        {
                            this.Log.Info("Data: {0}", HttpUtility.UrlDecode(c.Result).Substring(0, this.MaxContentLength - 1));
                        }
                        else 
                        {
                            this.Log.Info("Data: {0}", HttpUtility.UrlDecode(c.Result));
                        }
                    }
                });

            var response = await base.SendAsync(request, cancellationToken);

            // Log the error if it returned an error
            if (!response.IsSuccessStatusCode)
            {
                this.Log.Error(response.Content.ReadAsStringAsync().Result);
            }

            // Log performance
            this.Log.Info("Request processing time: " + DateTime.Now.Subtract(startTime).TotalSeconds + "s");

            return response;
        }
    }
}
