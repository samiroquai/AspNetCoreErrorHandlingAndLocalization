using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WebApplication1.Business;
using WebApplication1.DTO;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Get_FirstErrorType()
        {
            //levée d'une exception technique, erreur inattendue;
            SqlConnection conn = new SqlConnection("Data Source=localhost\\FAKEINSTANCE;Integrated Security=true;Initial Catalog=FakeDB");
            conn.Open();
            return Ok();
        }

        /// <summary>
        /// Création d'une nouvelle ville
        /// </summary>
        /// <remarks>Utilisez le nom de ville "namur" pour générer une erreur métier. Spécifiez des longueurs supérieures à 255 ou inférieures à 2 pour le nom de la ville afin de générer des erreurs de validation.</remarks>
        /// <param name="newCity"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        public ActionResult<City> Post_SecondErrorType([FromBody] City newCity)
        {
            //le type d'exception est ici incohérent avec l'action de suppression. Ce n'est rien, c'est juste pour l'exemple.
            if (newCity.Name.ToLower() == "namur")
                throw new BusinessException(BusinessExceptionCode.DuplicateCity);
            return Created("", newCity);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Put_ThirdErrorType()
        {
            return NotFound();
        }

        /// <summary>
        /// Suppression d'une ville existante
        /// </summary>
        /// <remarks>Utilisez le nom de ville "namur" pour générer une erreur métier.</remarks>
        /// <param name="cityToDelete"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public ActionResult Delete_FourthErrorType([FromBody] City cityToDelete)
        {
            if (cityToDelete.Name.ToLower() == "namur")

                //le type d'exception est ici incohérent avec l'action de suppression. Ce n'est rien, c'est juste pour l'exemple.
                throw new BusinessException(BusinessExceptionCode.PersistentCity);
            return Accepted();
        }
    }
}
