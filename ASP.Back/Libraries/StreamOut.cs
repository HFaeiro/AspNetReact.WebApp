using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using TeamManiacs.Core.Models;

namespace ASP.Back.Libraries
{
    public class StreamOut
    {
        private readonly ControllerBase _controller;
        List<string> _streams;
        public enum StatusCodes
        {
            Success = 200,
            Blob = 201,
            Text = 202,
        }
        public int StatusCode
        {
            get
            {
                return _controller.Response.StatusCode;
            }
        }

        public StreamOut(ControllerBase controller)
        {
            _controller = controller;
        }

        public void Add(string charStream)
        {
            _streams.Add(charStream);
        }
        public async Task Write(string charStream)
        {
            await _controller.Response.Body.WriteAsync(charStream.Select(c => (byte)c).ToArray(), 0, charStream.Length);
        }

        public async Task Write(Stream? stream, string contentType, StatusCodes code = StatusCodes.Success)
        {
            try
            {
                
                using (stream)
                {
                    if (stream != null && stream.Length > 0)
                    {

                        _controller.Response.StatusCode = (int)code;
                        _controller.Response.ContentType = contentType;
                        byte[] buffer = new byte[1024 * 10];
                        int bytesRead = 0;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length - 1)) > 0)
                        {
                            //string base64Video = Convert.ToBase64String(buffer, 0, bytesRead, Base64FormattingOptions.None);
                            await _controller.Response.Body.WriteAsync(buffer, 0, bytesRead);

                        }

                        await _controller.Response.Body.FlushAsync();
                        stream.Close();
                        return;
                    }
                    else
                    {

                        _controller.Response.StatusCode = 400;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message + ex.StackTrace);
                _controller.Response.StatusCode = 400;
            }
        }
        

        public async Task Write()
        {
            try
            {
                foreach (var stream in _streams)
                {
                    await _controller.Response.Body.WriteAsync(stream.Select(c => (byte)c).ToArray(), 0, stream.Length);
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }



    }
}
