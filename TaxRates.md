# What can you do with Tax Rates?

Tax rates are the country / state-province / zip rates used by nopCommerce's bundled
**Manual (Fixed or By Country/State/Zip)** tax provider (`Nop.Plugin.Tax.FixedOrByCountryStateZip`).
That plugin must be installed; these endpoints read and write its `TaxRate` table directly and
clear its rate cache after every change.

All endpoints require the `ManageTaxSettings` permission.

+ [GET /api/tax_rates
Receive a list of tax rates](#get-apitax_rates)

+ [GET /api/tax_rates/count
Receive a count of tax rates](#get-apitax_ratescount)

+ [GET /api/tax_rates/{id}
Receive a single tax rate](#get-apitax_ratesid)

+ [POST /api/tax_rates
Create a tax rate](#post-apitax_rates)

+ [PUT /api/tax_rates/{id}
Modify a tax rate](#put-apitax_ratesid)

+ [DELETE /api/tax_rates/{id}
Remove a tax rate](#delete-apitax_ratesid)

+ [POST /api/tax_rates/batch
Create or update many tax rates in one request](#post-apitax_ratesbatch)

+ [POST /api/tax_rates/batch_delete
Remove many tax rates in one request](#post-apitax_ratesbatch_delete)

## The tax rate object

| Field | Description |
|:---|:---|
| id | Read-only identifier |
| store_id | Store the rate applies to. `0` = all stores |
| tax_category_id | Tax category (required on create) |
| country_id | Country (required on create) |
| state_province_id | State/province, must belong to the country. `0` = all states |
| zip | Zip / postal code, trimmed. Empty = all zips |
| percentage | Tax percentage (required on create), must not be negative |

# Tax Rate Endpoints

## GET /api/tax_rates
Retrieve tax rates, ordered by id.

| GET | /api/tax_rates |
|:---|:---|
| ids | Restrict results to these ids (`?ids=1&ids=2`) |
| store_id | Only rates for this store (`0` = rates for all stores) |
| tax_category_id | Only rates for this tax category |
| country_id | Only rates for this country |
| state_province_id | Only rates for this state/province (`0` = rates for all states) |
| zip | Only rates with this exact zip |
| since_id | Restrict results to after the specified ID |
| limit | Amount of results (default: 50) (maximum: 250) |
| page | Page to show (default: 1) |
| fields | Comma-separated list of fields to include in the response |

<details><summary>Response</summary><p>
```json
HTTP/1.1 200 OK

{
  "tax_rates": [
    {
      "id": 1,
      "store_id": 0,
      "tax_category_id": 1,
      "country_id": 1,
      "state_province_id": 0,
      "zip": "",
      "percentage": 7.25
    }
  ]
}
```
</p></details>

## GET /api/tax_rates/count
Count tax rates. Accepts the same filters as the list endpoint (except paging).

```json
{ "count": 1234 }
```

## GET /api/tax_rates/{id}
Retrieve a single tax rate.

## POST /api/tax_rates
Create a tax rate. `tax_category_id`, `country_id` and `percentage` are required.

```json
{
  "tax_rate": {
    "store_id": 0,
    "tax_category_id": 1,
    "country_id": 1,
    "state_province_id": 0,
    "zip": "",
    "percentage": 7.25
  }
}
```

## PUT /api/tax_rates/{id}
Modify a tax rate. Only the fields present in the request are changed.

```json
{
  "tax_rate": {
    "percentage": 8.0
  }
}
```

## DELETE /api/tax_rates/{id}
Remove a tax rate.

## POST /api/tax_rates/batch
Create or update many tax rates in one request. Intended for loading hundreds or thousands of rates.

* Items with an `id` update that tax rate (only the fields provided are changed).
* Items without an `id` are matched on `store_id` + `tax_category_id` + `country_id` + `state_province_id` + `zip`
  (zip compared trimmed and case-insensitively). A match is updated, otherwise a new tax rate is created.
* The whole batch is validated first. If any item is invalid a `422` is returned with errors keyed by
  position, e.g. `tax_rates[3].country_id`, and nothing is written.
* New rows are written with a bulk insert. The response contains the resulting tax rates, with ids,
  in the same order as the request.

```json
{
  "tax_rates": [
    { "tax_category_id": 1, "country_id": 1, "state_province_id": 0, "zip": "", "percentage": 6.0 },
    { "tax_category_id": 1, "country_id": 1, "state_province_id": 40, "zip": "90210", "percentage": 9.5 },
    { "id": 17, "percentage": 7.5 }
  ]
}
```

<details><summary>Response</summary><p>
```json
HTTP/1.1 200 OK

{
  "tax_rates": [
    { "id": 1,  "store_id": 0, "tax_category_id": 1, "country_id": 1, "state_province_id": 0,  "zip": "",      "percentage": 6.0 },
    { "id": 42, "store_id": 0, "tax_category_id": 1, "country_id": 1, "state_province_id": 40, "zip": "90210", "percentage": 9.5 },
    { "id": 17, "store_id": 0, "tax_category_id": 1, "country_id": 1, "state_province_id": 0,  "zip": "",      "percentage": 7.5 }
  ]
}
```
</p></details>

## POST /api/tax_rates/batch_delete
Remove many tax rates in one request. Ids that do not exist are reported and skipped.

```json
{ "ids": [1, 2, 3] }
```

<details><summary>Response</summary><p>
```json
HTTP/1.1 200 OK

{
  "deleted_ids": [1, 2],
  "not_found_ids": [3]
}
```
</p></details>
