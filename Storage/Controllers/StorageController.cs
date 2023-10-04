using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace Storage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        StorageService _storageService;
        public StorageController(StorageService storage) {
            _storageService = storage;
        }

        // GET api/<StorageController>/5
        [HttpGet]
        public async Task<Stream> Get([FromQuery]string fileName)
        {
            var file = await _storageService.GetFile(fileName);
            return file;
               
        }

        [HttpDelete]
        public async Task DeleteFile([FromQuery]string fileName)
        {
            _storageService.DeleteTempFileAsync(fileName);
        }

        // POST api/<StorageController>
        [HttpPost]
        public async Task<string> Post([FromForm] IFormFile file)
        {
            if (file.Length > 0)
            {
                var fileExtension = Path.GetExtension(file.FileName);
                string fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Directory.GetCurrentDirectory() + "\\" + fileName;

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                return fileName;
            }
            return null;
        }

      
    }
}
