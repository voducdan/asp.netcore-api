using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkgenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkgenerator)
        {
            this._campRepository = campRepository;
            this._mapper = mapper;
            this._linkgenerator = linkgenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var result = await this._campRepository.GetAllCampsAsync(includeTalks);
                CampModel[] campModel = this._mapper.Map<CampModel[]>(result);
                return Ok(campModel);
            }
            catch (System.Exception)
            {
                return BadRequest("Database failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<IActionResult> Get(string moniker)
        {
            try
            {
                var result = await this._campRepository.GetCampAsync(moniker);
                if (result == null)
                {
                    return NotFound();
                }
                CampModel campModel = this._mapper.Map<CampModel>(result);
                return Ok(campModel);
            }
            catch (System.Exception)
            {
                return BadRequest("$moniter not found");
            }

        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var result = await this._campRepository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!result.Any())
                {
                    return NotFound();
                }
                return Ok(this._mapper.Map<CampModel[]>(result));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public  async Task<IActionResult> Post(CampModel model)
        {
            try
            {
                var existingCamp = await this._campRepository.GetCampAsync(model.Moniker);
                if(existingCamp != null)
                {
                    return BadRequest("Moniker is in use");
                }
                var location = this._linkgenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = this._mapper.Map<Camp>(model);
                this._campRepository.Add(camp);
                if (await this._campRepository.SaveChangesAsync())
                {
                    return Created(location, this._mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await this._campRepository.GetCampAsync(moniker);
                if(camp == null)
                {
                    return NotFound();
                }
                this._campRepository.Delete(camp);
                if (await this._campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            return BadRequest("Fail to delete the camp");
        }
    }
}