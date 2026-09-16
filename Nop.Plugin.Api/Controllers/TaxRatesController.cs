using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTOs.TaxRates;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.TaxRatesParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using System.Net;

namespace Nop.Plugin.Api.Controllers
{
    /// <summary>
    ///     Manages country/state/zip tax rates used by the bundled
    ///     "Manual (Fixed or By Country/State/Zip)" tax provider plugin.
    ///     That plugin must be installed, otherwise the TaxRate table does not exist.
    /// </summary>
    [AuthorizePermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public class TaxRatesController : BaseApiController
    {
        private readonly ITaxRateApiService _taxRateApiService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        // per-request memo of reference lookups so bulk requests do not hit the services once per row
        private readonly Dictionary<int, TaxCategory> _taxCategories = new();
        private readonly Dictionary<int, Country> _countries = new();
        private readonly Dictionary<int, StateProvince> _stateProvinces = new();
        private readonly Dictionary<int, Store> _stores = new();

        public TaxRatesController(
            ITaxRateApiService taxRateApiService,
            ITaxCategoryService taxCategoryService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService)
            : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService,
                discountService, customerActivityService, localizationService, pictureService)
        {
            _taxRateApiService = taxRateApiService;
            _taxCategoryService = taxCategoryService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        /// <summary>
        ///     Receive a list of tax rates
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/tax_rates", Name = "GetTaxRates")]
        [ProducesResponseType(typeof(TaxRatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetTaxRates([FromQuery] TaxRatesParametersModel parameters)
        {
            if (parameters.Limit < Constants.Configurations.MinLimit || parameters.Limit > Constants.Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Constants.Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            var taxRates = _taxRateApiService.GetTaxRates(parameters.Ids, parameters.StoreId, parameters.TaxCategoryId,
                parameters.CountryId, parameters.StateProvinceId, parameters.Zip,
                parameters.Limit, parameters.Page, parameters.SinceId);

            var rootObject = new TaxRatesRootObject
            {
                TaxRates = taxRates.Select(x => x.ToDto()).ToList()
            };

            var json = JsonFieldsSerializer.Serialize(rootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        ///     Receive a count of tax rates
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/tax_rates/count", Name = "GetTaxRatesCount")]
        [ProducesResponseType(typeof(TaxRatesCountRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetTaxRatesCount([FromQuery] TaxRatesCountParametersModel parameters)
        {
            var count = _taxRateApiService.GetTaxRatesCount(parameters.Ids, parameters.StoreId, parameters.TaxCategoryId,
                parameters.CountryId, parameters.StateProvinceId, parameters.Zip);

            return Ok(new TaxRatesCountRootObject { Count = count });
        }

        /// <summary>
        ///     Retrieve a tax rate by id
        /// </summary>
        /// <param name="id">Id of the tax rate</param>
        /// <param name="fields">Fields from the tax rate you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/tax_rates/{id}", Name = "GetTaxRateById")]
        [ProducesResponseType(typeof(TaxRatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetTaxRateById([FromRoute] int id, [FromQuery] string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var taxRate = await _taxRateApiService.GetTaxRateByIdAsync(id);

            if (taxRate == null)
            {
                return Error(HttpStatusCode.NotFound, "tax_rate", "not found");
            }

            var rootObject = new TaxRatesRootObject();
            rootObject.TaxRates.Add(taxRate.ToDto());

            var json = JsonFieldsSerializer.Serialize(rootObject, fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        ///     Create a new tax rate
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="422">Unprocessable Entity</response>
        [HttpPost]
        [Route("/api/tax_rates", Name = "CreateTaxRate")]
        [ProducesResponseType(typeof(TaxRatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<IActionResult> CreateTaxRate(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<TaxRateDto>))]
            Delta<TaxRateDto> taxRateDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var taxRate = BuildNewTaxRate(taxRateDelta.Dto, string.Empty);

            if (!ModelState.IsValid || !await ValidateTaxRateAsync(taxRate, string.Empty))
            {
                return Error();
            }

            await _taxRateApiService.InsertTaxRateAsync(taxRate);

            await CustomerActivityService.InsertActivityAsync("AddNewTaxRate", $"Added a new tax rate (ID = {taxRate.Id})", taxRate);

            var rootObject = new TaxRatesRootObject();
            rootObject.TaxRates.Add(taxRate.ToDto());

            var json = JsonFieldsSerializer.Serialize(rootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        ///     Update an existing tax rate
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        /// <response code="422">Unprocessable Entity</response>
        [HttpPut]
        [Route("/api/tax_rates/{id}", Name = "UpdateTaxRate")]
        [ProducesResponseType(typeof(TaxRatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<IActionResult> UpdateTaxRate(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<TaxRateDto>))]
            Delta<TaxRateDto> taxRateDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var taxRate = await _taxRateApiService.GetTaxRateByIdAsync(taxRateDelta.Dto.Id);

            if (taxRate == null)
            {
                return Error(HttpStatusCode.NotFound, "tax_rate", "not found");
            }

            ApplyDto(taxRateDelta.Dto, taxRate);

            if (!await ValidateTaxRateAsync(taxRate, string.Empty))
            {
                return Error();
            }

            await _taxRateApiService.UpdateTaxRateAsync(taxRate);

            await CustomerActivityService.InsertActivityAsync("UpdateTaxRate", $"Updated a tax rate (ID = {taxRate.Id})", taxRate);

            var rootObject = new TaxRatesRootObject();
            rootObject.TaxRates.Add(taxRate.ToDto());

            var json = JsonFieldsSerializer.Serialize(rootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        ///     Delete a tax rate
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [HttpDelete]
        [Route("/api/tax_rates/{id}", Name = "DeleteTaxRate")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteTaxRate([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var taxRate = await _taxRateApiService.GetTaxRateByIdAsync(id);

            if (taxRate == null)
            {
                return Error(HttpStatusCode.NotFound, "tax_rate", "not found");
            }

            await _taxRateApiService.DeleteTaxRateAsync(taxRate);

            await CustomerActivityService.InsertActivityAsync("DeleteTaxRate", $"Deleted a tax rate (ID = {taxRate.Id})", taxRate);

            return new RawJsonActionResult("{}");
        }

        /// <summary>
        ///     Create or update many tax rates in one request.
        ///     Items with an "id" update that tax rate. Items without an "id" are matched on
        ///     (store_id, tax_category_id, country_id, state_province_id, zip): an existing match is
        ///     updated, otherwise a new tax rate is created. The whole batch is validated before
        ///     anything is written. The response lists the resulting tax rates in request order.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="422">Unprocessable Entity</response>
        [HttpPost]
        [Route("/api/tax_rates/batch", Name = "BatchUpsertTaxRates")]
        [ProducesResponseType(typeof(TaxRatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<IActionResult> BatchUpsertTaxRates([FromBody] TaxRatesRootObject batch)
        {
            if (batch?.TaxRates == null || batch.TaxRates.Count == 0)
            {
                return Error(HttpStatusCode.BadRequest, "tax_rates", "no tax rates provided");
            }

            for (var i = 0; i < batch.TaxRates.Count; i++)
            {
                if (batch.TaxRates[i] == null)
                {
                    ModelState.AddModelError($"tax_rates[{i}]", "item must be an object");
                }
                else if (batch.TaxRates[i].Id < 0)
                {
                    ModelState.AddModelError($"tax_rates[{i}].id", "invalid id");
                }
            }

            if (!ModelState.IsValid)
            {
                return Error();
            }

            var requestedIds = batch.TaxRates.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
            var existingById = (await _taxRateApiService.GetTaxRatesByIdsAsync(requestedIds)).ToDictionary(x => x.Id);

            // only load candidate rows for the countries referenced by id-less items
            var idLessCountryIds = batch.TaxRates.Where(x => x.Id == 0 && x.CountryId.HasValue).Select(x => x.CountryId.Value).Distinct().ToList();
            var existingByKey = idLessCountryIds.Count > 0
                ? await _taxRateApiService.GetTaxRatesByNaturalKeyAsync(idLessCountryIds)
                : new Dictionary<string, TaxRate>();

            // the two lookups are separate queries, so make the index point at the same instances we update by id
            foreach (var key in existingByKey.Keys.ToList())
            {
                if (existingById.TryGetValue(existingByKey[key].Id, out var sameRow))
                {
                    existingByKey[key] = sameRow;
                }
            }

            var toInsert = new List<TaxRate>();
            var toUpdate = new List<TaxRate>();
            var results = new List<TaxRate>();

            for (var i = 0; i < batch.TaxRates.Count; i++)
            {
                var dto = batch.TaxRates[i];
                var prefix = $"tax_rates[{i}].";
                TaxRate taxRate;

                if (dto.Id > 0)
                {
                    if (!existingById.TryGetValue(dto.Id, out taxRate))
                    {
                        ModelState.AddModelError($"{prefix}id", "not found");
                        continue;
                    }

                    var oldKey = NaturalKeyOf(taxRate);
                    ApplyDto(dto, taxRate);
                    var newKey = NaturalKeyOf(taxRate);

                    // keep the natural-key index in sync so later id-less items match this pending update
                    if (oldKey != newKey && existingByKey.TryGetValue(oldKey, out var indexed) && indexed.Id == taxRate.Id)
                    {
                        existingByKey.Remove(oldKey);
                    }
                    existingByKey[newKey] = taxRate;

                    if (!toUpdate.Contains(taxRate))
                    {
                        toUpdate.Add(taxRate);
                    }
                }
                else
                {
                    var candidate = BuildNewTaxRate(dto, prefix);
                    if (candidate == null)
                    {
                        continue;
                    }

                    var key = NaturalKeyOf(candidate);

                    if (existingByKey.TryGetValue(key, out taxRate))
                    {
                        // existing row (either from the database or created earlier in this batch): update the rate
                        taxRate.Percentage = candidate.Percentage;

                        if (taxRate.Id > 0 && !toUpdate.Contains(taxRate))
                        {
                            toUpdate.Add(taxRate);
                        }
                    }
                    else
                    {
                        taxRate = candidate;
                        existingByKey[key] = taxRate;
                        toInsert.Add(taxRate);
                    }
                }

                await ValidateTaxRateAsync(taxRate, prefix);
                results.Add(taxRate);
            }

            if (!ModelState.IsValid)
            {
                return Error();
            }

            await _taxRateApiService.UpdateTaxRatesAsync(toUpdate);
            await _taxRateApiService.InsertTaxRatesAsync(toInsert);

            await CustomerActivityService.InsertActivityAsync("BatchUpsertTaxRates",
                $"Batch upsert of tax rates (created = {toInsert.Count}, updated = {toUpdate.Count})");

            var rootObject = new TaxRatesRootObject
            {
                TaxRates = results.Select(x => x.ToDto()).ToList()
            };

            var json = JsonFieldsSerializer.Serialize(rootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        ///     Delete many tax rates in one request. Ids that do not exist are reported and skipped.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/tax_rates/batch_delete", Name = "BatchDeleteTaxRates")]
        [ProducesResponseType(typeof(TaxRatesBatchDeleteRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> BatchDeleteTaxRates([FromBody] TaxRatesBatchDeleteParametersModel parameters)
        {
            if (parameters?.Ids == null || parameters.Ids.Count == 0)
            {
                return Error(HttpStatusCode.BadRequest, "ids", "no ids provided");
            }

            if (parameters.Ids.Any(id => id <= 0))
            {
                return Error(HttpStatusCode.BadRequest, "ids", "invalid id");
            }

            var ids = parameters.Ids.Distinct().ToList();
            var taxRates = await _taxRateApiService.GetTaxRatesByIdsAsync(ids);

            await _taxRateApiService.DeleteTaxRatesAsync(taxRates);

            var result = new TaxRatesBatchDeleteRootObject
            {
                DeletedIds = taxRates.Select(x => x.Id).ToList()
            };
            result.NotFoundIds = ids.Except(result.DeletedIds).ToList();

            if (result.DeletedIds.Count > 0)
            {
                await CustomerActivityService.InsertActivityAsync("BatchDeleteTaxRates",
                    $"Batch delete of tax rates (deleted = {result.DeletedIds.Count})");
            }

            return Ok(result);
        }

        #region Helpers

        private string NaturalKeyOf(TaxRate taxRate)
        {
            return _taxRateApiService.GetNaturalKey(taxRate.StoreId, taxRate.TaxCategoryId, taxRate.CountryId,
                taxRate.StateProvinceId, taxRate.Zip);
        }

        /// <summary>
        ///     Builds a new entity from a dto. Adds model errors (with the given key prefix) and returns null
        ///     when required fields are missing.
        /// </summary>
        private TaxRate BuildNewTaxRate(TaxRateDto dto, string prefix)
        {
            var valid = true;

            if (dto.TaxCategoryId == null)
            {
                ModelState.AddModelError($"{prefix}tax_category_id", "required");
                valid = false;
            }

            if (dto.CountryId == null)
            {
                ModelState.AddModelError($"{prefix}country_id", "required");
                valid = false;
            }

            if (dto.Percentage == null)
            {
                ModelState.AddModelError($"{prefix}percentage", "required");
                valid = false;
            }

            if (!valid)
            {
                return null;
            }

            return new TaxRate
            {
                StoreId = dto.StoreId ?? 0,
                TaxCategoryId = dto.TaxCategoryId.Value,
                CountryId = dto.CountryId.Value,
                StateProvinceId = dto.StateProvinceId ?? 0,
                Zip = _taxRateApiService.NormalizeZip(dto.Zip),
                Percentage = dto.Percentage.Value
            };
        }

        /// <summary>
        ///     Applies the provided (non-null) dto fields onto an existing entity.
        /// </summary>
        private void ApplyDto(TaxRateDto dto, TaxRate taxRate)
        {
            if (dto.StoreId.HasValue)
            {
                taxRate.StoreId = dto.StoreId.Value;
            }

            if (dto.TaxCategoryId.HasValue)
            {
                taxRate.TaxCategoryId = dto.TaxCategoryId.Value;
            }

            if (dto.CountryId.HasValue)
            {
                taxRate.CountryId = dto.CountryId.Value;
            }

            if (dto.StateProvinceId.HasValue)
            {
                taxRate.StateProvinceId = dto.StateProvinceId.Value;
            }

            if (dto.Zip != null)
            {
                taxRate.Zip = _taxRateApiService.NormalizeZip(dto.Zip);
            }

            if (dto.Percentage.HasValue)
            {
                taxRate.Percentage = dto.Percentage.Value;
            }
        }

        /// <summary>
        ///     Validates the values and referenced entities of a tax rate. Adds model errors with the given key prefix.
        /// </summary>
        private async Task<bool> ValidateTaxRateAsync(TaxRate taxRate, string prefix)
        {
            var valid = true;

            if (taxRate.TaxCategoryId <= 0 || await GetTaxCategoryAsync(taxRate.TaxCategoryId) == null)
            {
                ModelState.AddModelError($"{prefix}tax_category_id", "tax category not found");
                valid = false;
            }

            Country country = null;
            if (taxRate.CountryId <= 0 || (country = await GetCountryAsync(taxRate.CountryId)) == null)
            {
                ModelState.AddModelError($"{prefix}country_id", "country not found");
                valid = false;
            }

            if (taxRate.StateProvinceId < 0)
            {
                ModelState.AddModelError($"{prefix}state_province_id", "invalid state_province_id");
                valid = false;
            }
            else if (taxRate.StateProvinceId > 0)
            {
                var stateProvince = await GetStateProvinceAsync(taxRate.StateProvinceId);
                if (stateProvince == null)
                {
                    ModelState.AddModelError($"{prefix}state_province_id", "state/province not found");
                    valid = false;
                }
                else if (country != null && stateProvince.CountryId != country.Id)
                {
                    ModelState.AddModelError($"{prefix}state_province_id", "state/province does not belong to the country");
                    valid = false;
                }
            }

            if (taxRate.StoreId < 0)
            {
                ModelState.AddModelError($"{prefix}store_id", "invalid store_id");
                valid = false;
            }
            else if (taxRate.StoreId > 0 && await GetStoreAsync(taxRate.StoreId) == null)
            {
                ModelState.AddModelError($"{prefix}store_id", "store not found");
                valid = false;
            }

            if (taxRate.Percentage < 0)
            {
                ModelState.AddModelError($"{prefix}percentage", "percentage must not be negative");
                valid = false;
            }

            return valid;
        }

        private async Task<TaxCategory> GetTaxCategoryAsync(int id)
        {
            if (!_taxCategories.TryGetValue(id, out var taxCategory))
            {
                taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(id);
                _taxCategories[id] = taxCategory;
            }

            return taxCategory;
        }

        private async Task<Country> GetCountryAsync(int id)
        {
            if (!_countries.TryGetValue(id, out var country))
            {
                country = await _countryService.GetCountryByIdAsync(id);
                _countries[id] = country;
            }

            return country;
        }

        private async Task<StateProvince> GetStateProvinceAsync(int id)
        {
            if (!_stateProvinces.TryGetValue(id, out var stateProvince))
            {
                stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(id);
                _stateProvinces[id] = stateProvince;
            }

            return stateProvince;
        }

        private async Task<Store> GetStoreAsync(int id)
        {
            if (!_stores.TryGetValue(id, out var store))
            {
                store = await StoreService.GetStoreByIdAsync(id);
                _stores[id] = store;
            }

            return store;
        }

        #endregion
    }
}
