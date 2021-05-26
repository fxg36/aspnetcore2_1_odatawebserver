using System;
using System.Linq;
using ODataWebserver.Global;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataWebserver.Models;
using Microsoft.AspNetCore.Http;
using ODataWebserver.EfContext;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ODataWebserver.Webserver.Controllers
{
    /* INSTRUCTIONS TO USE THE API
     * ---------------------------
     * - For an overview, call <address>/odata/<controllername>
     * - For a detail endpoint specification, call <adr>/odata/$metadata
     * - To GET specific objects, call <adr>/odata/<ObjectSet>
     *    - You can filter them by id <adr>/odata/<ObjectSet>(<Id>)
     *    - You can expand them toward parent or children objects, e.g.:
     *      <adr>/odata/<ObjectSet>?$expand=<ChildOrParentAccessProperty>[,<ChildOrParentAccessProperty>,...]
     *    - You can filter or sort them, e.g.:
     *      <adr>/odata/<ObjectSet>?$filter=<Property> eq 22
     *    - For more info: see OData specification
     *  - For insert, use POST (c.f. comments below)
     *  - For update, use PATCH (c.f. comments below)
     *  - For delete, use DELETE (c.f. comments below)
     */
    public class ODataControllerBase<T> : ControllerBase where T : class, IModel, new()
    {
        #region Vars / Ctor / Init

        private static bool ConsumersInitialized = false;
        protected readonly DbContext context;
        private const string AdminApiKey = "df4c9af2-24d6-4058-bf6b-cca851b473a6";

        protected ODataControllerBase(DbContext context)
        {
            this.context = context;

            CheckConsumersWithApiKeys();
        }

        private void CheckConsumersWithApiKeys()
        {
            if (ConsumersInitialized) return;
            var allKeysRequired = new Dictionary<string, string>();
            foreach (var r in ConfigHelper.Instance.ApiConsumers)
                allKeysRequired.Add(r.Key, r.Value);
            allKeysRequired.Add("admin", AdminApiKey);

            var keysExisting = context.Set<ApiConsumer>().Select(x => new { Key = x.ApiKey, Name = x.Name }).ToList();

            foreach (var key in allKeysRequired)
            {
                if (!keysExisting.Where(x => x.Key == key.Value).Any())
                {
                    context.Set<ApiConsumer>().Add(new ApiConsumer
                    {
                        Name = key.Key,
                        ApiKey = key.Value
                    });
                    context.SaveChanges();
                }
            }

            ConsumersInitialized = true;
        }

        #endregion

        #region CRUD

        [EnableQuery]
        public async Task<ActionResult<IList<T>>> Get()
        {
            return await DoWithRequest(ApiAccess.Read, false, false, async (urlId, consumer) =>
            {
                if (urlId.Equals(-1))
                    return Ok(context.Set<T>().AsQueryable());

                return Ok(context.Set<T>().Where(o => o.Id == urlId).AsQueryable());
            });
        }


        /// <summary>
        /// For creating a new object
        /// </summary>
        public async Task<ActionResult<IList<T>>> Post([FromBody] Newtonsoft.Json.Linq.JObject[] data)
        {
            // POST => <Address>/odata/<ObjectName>
            // With body [{...}]
            // POST bodys must be JSON arrays !!!

            var objs = (data.Select(d => d.ToObject<T>())).ToList();

            return await DoWithRequest(ApiAccess.Insert, true, false, async (urlId, consumer) =>
            {
                foreach (var obj in objs)
                {
                    if (obj.Id != 0)
                        return BadRequest("Use <POST> for creating objects only. Do not set 'Id' in this case. Otherwise use <PATCH> for updating objects.");
                }

                try
                {
                    foreach (var obj in objs)
                    {
                        obj.CreatedBy = consumer;
                        obj.LastChangeBy = consumer;
                        context.PersistNoSave(obj);
                    }
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
                var uri = new Uri($"{ Request.Scheme }://{Request.Host}{Request.Path}/{objs.Last().Id}", UriKind.RelativeOrAbsolute);
                return Created(uri, objs);
            });
        }

        /// <summary>
        /// For updating an existing object
        /// </summary>
        public async Task<ActionResult<IList<T>>> Patch([FromBody] T obj)
        {
            // PATCH => <Address>/odata/<ObjectName>(<Id>)
            // With body {...}

            return await DoWithRequest(ApiAccess.Update, true, true, async (urlId, consumer) =>
            {
                var existing = context.Set<T>().FirstOrDefault(o => o.Id == urlId);

                if (existing == null)
                    return NotFound("No object found for this Id.");

                obj.Id = urlId;
                obj.LastChangeUtc = DateTime.UtcNow;
                obj.LastChangeBy = consumer;
                obj.CreatedUtc = existing.CreatedUtc;
                obj.CreatedBy = existing.CreatedBy;

                try
                {
                    context.Entry(existing).State = EntityState.Detached;
                    context.Entry(obj).State = EntityState.Modified;
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }
                return Ok(obj);
            });
        }

        public async Task<ActionResult<IList<T>>> Delete()
        {
            // DELETE => <Address>/odata/<ObjectName>(<Id>)
            // no body

            return await DoWithRequest(ApiAccess.Delete, true, true, async (urlId, consumer) =>
            {
                var obj = context.Set<T>().FirstOrDefault(o => o.Id == urlId);

                if (obj == null)
                    return NotFound("No object found for this Id.");

                try
                {
                    context.RemoveAndSave(obj);
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }

                return NoContent();
            });
        }

        #endregion

        private async Task<ActionResult<IList<T>>> DoWithRequest(ApiAccess accessToCheck,
                bool checkModelState, bool checkUrlId,
                Func<int, string, Task<ActionResult<IList<T>>>> caller)
        {
            if (!ApiKeyValid(out var validConsumer))
                return BadRequest($"Auth failed. Invalid API key. Insert your API key into the HTTP-Header 'Api-Key'!");

            var id = GetIdFromUrl(Request);

            try
            {
                if (checkUrlId && id.Equals(-1))
                    return NotFound($"The URL has a wrong format. For <{Request.Method}> the URL must have the following format:" +
                        "<address>/odata/<object>(<id>)");

                if (!IsAccessAllowed(validConsumer))
                    return BadRequest($"It is not allowed to access <{typeof(T).Name}> via <{Request.Method}>.");

                if (checkModelState && !ModelState.IsValid)
                    return BadRequest(ModelState);

                return await caller(id, validConsumer.Name);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occured: {ex.Message}\n\n{ex.StackTrace}");
            }
            finally
            {
                //LogRequest();
            }

            #region Nested Helper

            bool ApiKeyValid(out ApiConsumer consumer)
            {
                consumer = context.Set<ApiConsumer>().FirstOrDefault(x => x.ApiKey == Request.Headers["Api-Key"]);
                return consumer != null;
            }

            bool IsAccessAllowed(ApiConsumer consumer)
            {
                if (consumer.ApiKey.Equals(AdminApiKey)) return true;

                var rights = ODataControllerRegisterer.AccessRights[typeof(T)].ToList()
                        .Single(x => x.Key == consumer.ApiKey).Value;

                return EnumHelper.IsOneFlagSet(rights, accessToCheck);
            }

            int GetIdFromUrl(HttpRequest req)
            {
                var path = req.Path.Value;
                if (!path.Contains("(")) return -1;
                var res = int.TryParse(path.Split("(")[1].Split(")").First(), out var resVal);
                return res ? resVal : -1;
            }

            #endregion
        }

        //private async Task LogRequest(T obj, int id)
        //{
        //    if (!ConfigHelper.Instance.ApiLogging) return;

        //    var operationToLog = $"{Request.Method}=>{typeof(T).Name}";

        //    if (Request.Method.Equals("POST"))
        //    {
        //        if (obj.Id < 0) return;
        //        operationToLog += $"({obj.Id})";
        //    }
        //    else if (Request.Method.Equals("GET"))
        //        operationToLog += $"({(id.Equals(-1) ? Request.QueryString.Value : id.ToString())})";
        //    else
        //        operationToLog += $"({id})";

        //    context.PersistAndSaveAsync(new ApiConsumerLog
        //    {
        //        ApiConsumerId = validConsumer.Id,
        //        CreatedUtc = DateTime.UtcNow,
        //        Operation = operationToLog
        //    });
        //}
    }
}