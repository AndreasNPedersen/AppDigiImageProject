namespace Storage
{
    public class StorageService
    {
        public StorageService() { }

        public async Task<Stream> GetFile(string fileName)
        {
            var options = new FileStreamOptions();
            options.Access = FileAccess.Read;
            options.Mode = FileMode.Open;
            string filePath = Directory.GetCurrentDirectory() + "\\" + fileName;
            try
            {
                var file = new FileStream(path: filePath, options);
                return file;

            }
            catch { return null; };

        }
       
        public async Task DeleteTempFileAsync(string fileName) // this should be a job for another app.
        {
            string filePath = Directory.GetCurrentDirectory() + "\\" + fileName;
            try
            {
                Thread.Sleep(5000); // sometimes it can be too quick
                File.Delete(filePath);
            }
            catch { }
        }
    }
}
