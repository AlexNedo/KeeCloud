using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Dropbox.Api.Files;

namespace KeeCloud.Providers.Dropbox
{
    public class DropboxProvider : IProvider
    {
        public Uri Uri { get; set; }

        private const bool activateLog = false;

        public static StreamWriter logger = activateLog ? new StreamWriter("/home/alex/SSD_SHARE/projects/keecloud/output.log") : null;

        public static void Log(string line) {
            if (activateLog){
                logger.WriteLine(line);
                logger.Flush();
            }
        } 

        Stream IProvider.Get(ICredentials credentials)
        {
            try {
                Log("Get 1: " + GetPath(this.Uri));
                var client = Api.AuthenticatedClient(this.GetNetworkCredential(credentials));
                var task1 = client.Files.DownloadAsync(GetPath(this.Uri));
                Log("Get 2");
                task1.GetAwaiter().GetResult();
                var task2 = task1.Result.GetContentAsStreamAsync();
                var task2Res = task2.GetAwaiter().GetResult();
                Log("Get 3");
                return task2Res;
            }
            catch(Exception ex){
                Log("Get Exception details: " + ex);
                throw;
            }
        }

        void IProvider.Put(Stream stream, ICredentials credentials)
        {
            try {
                Log("Put Start: " + GetPath(this.Uri));
                var client = Api.AuthenticatedClient(this.GetNetworkCredential(credentials));

                var path = GetPath(this.Uri);
                Log("PutAsync Start: " + path);
                var commit = new CommitInfo(path, mode: WriteMode.Overwrite.Instance, mute: true);

                Log("Starting Upload");
                var task = client.Files.UploadAsync(commit, stream).ConfigureAwait(false);  
                Log("Starting Upload 2");
                var id = task.GetAwaiter().GetResult().Id;
                Log("Upload finished Id: " + id);
                return;
            }
            catch(Exception ex){
                Log("Put Exception details: " + ex);
                throw;
            }
        }


        void IProvider.Delete(ICredentials credentials)
        {
            try {
                Log("Delete: " + GetPath(this.Uri));
                var client = Api.AuthenticatedClient(this.GetNetworkCredential(credentials));

                var path = GetPath(this.Uri);
                Log("Before Delete: ");
                var task = client.Files.DeleteV2Async(path);
                var name = task.GetAwaiter().GetResult().Metadata.Name;
                Log("After Delete " + name);
             }
            catch(Exception ex){
                Log("Delete Exception details: " + ex);
                throw;
            }
        }


        void IProvider.Move(Uri destination, ICredentials credentials)
        {
            Log("Move: ");
            var client = Api.AuthenticatedClient(this.GetNetworkCredential(credentials));

            var sourcePath = GetPath(this.Uri);
            var destinationPath = GetPath(destination);

            Log("Move: " + sourcePath + " " + destinationPath);
            var task = client.Files.MoveV2Async(new RelocationArg(sourcePath, destinationPath));
            task.Wait();
        }

        bool IProvider.CanConfigureCredentials { get { return true; } }

        ICredentialConfigurationProvider IProvider.CredentialConfigurationProvider
        {
            get { return new DropboxCredentialConfigurationProvider(); }
        }

        string IProvider.FriendlyName { get { return "Dropbox"; } }

        private NetworkCredential GetNetworkCredential(ICredentials credentials)
        {
            return credentials.GetCredential(this.Uri, "basic");
        }

        private static string GetPath(Uri uri)
        {
            var path = "/" + uri.OriginalString.Substring(uri.Scheme.Length + 3);
            if (path.EndsWith("/"))
                return path.TrimEnd('/');
            else
                return path;
        }

        private byte[] ReadAll(Stream stream)
        {
            List<byte> bytes = new List<byte>();
            int read = 0;
            byte[] buffer = new byte[32000];
            do
            {
                read = stream.Read(buffer, 0, buffer.Length);
                bytes.AddRange(buffer.Take(read));
            } while (read > 0);

            return bytes.ToArray();
        }
    }
}
