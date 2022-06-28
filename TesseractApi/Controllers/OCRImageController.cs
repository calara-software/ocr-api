using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tesseract;
using TesseractApi.Services;

namespace TesseractApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OCRImageController : ControllerBase
    {

        private readonly ILogger<OCRImageController> _logger;

        public static IWebHostEnvironment _environment;

        private readonly TesseractService tesseractService;

        public const string trainedDataFolderName = "tessdata";

        public OCRImageController(ILogger<OCRImageController> logger, IWebHostEnvironment environment, TesseractService tesseractService)
        {
            _logger = logger;
            _environment = environment;
            this.tesseractService = tesseractService;

        }

        [HttpPost]
        public ActionResult Post([FromForm] string arquivo)
        {


            Stopwatch sw = new Stopwatch();
            sw.Start();

            string tessPath = Path.Combine(trainedDataFolderName,"");

            // string testImagePath = @"C:\Users\Thiago Caldas\Desktop\IMG-20220622-WA0010_2.jpg";
            List<string> textResults = new List<string>();

            try
            {
                using (var engine = new TesseractEngine("", "eng", EngineMode.Default))
                {
                    
                    using (var img = Pix.LoadFromMemory(Convert.FromBase64String(arquivo)))
                    {
                        using (var page = engine.Process(img))
                        {
                            // var text = page.GetText();
                            if (page.GetMeanConfidence()<.3){
                                return StatusCode(StatusCodes.Status422UnprocessableEntity, "Imagem inválida");
                            }
                            else {
                                var text = page.GetText();
                                textResults = GetSerialNumber(text.Split(' '));
                                sw.Stop();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status422UnprocessableEntity, $"Erro inesperado:{e.Message}");
            }

            return Ok(new {Text= textResults, TempoGasto = sw.Elapsed.TotalSeconds});
                // return Ok(new {Arquivo = Pix.LoadFromMemory(Convert.FromBase64String(arquivo))});
        }

           private List<string> GetSerialNumber(string[] text){

            const string EXP = @"[LVEMDPC]{1}[A-Z]{2}[0-9]+";

            var result = new List<string>();
            Regex regex = new Regex(EXP, RegexOptions.Multiline);

            text.ToList().ForEach(
                x => {
                    
                    if( regex.IsMatch(x)){
                        var t = regex.Match(x);
                        result.Add(t.Value);
                    } 
                }
            );

            return result;
        }

        [HttpPost("ocr-by-upload")]
        public async Task<List<string>> OcrByUpload(IFormFile file)
        {
            List<string> returnValue = null;

            await file.SaveFileOnTempDirectoryAndRun(filePath =>
            {
                var text = tesseractService.DecodeFile(filePath);
                returnValue = GetSerialNumber(text.Split(' '));
            });

            return returnValue;
        }
    }
}
