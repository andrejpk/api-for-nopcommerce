using Nop.Core;

namespace Nop.Plugin.Api.Domain
{
    /// <summary>
    /// Represents a country/state/zip tax rate.
    /// This maps to the "TaxRate" table created by the bundled
    /// Nop.Plugin.Tax.FixedOrByCountryStateZip plugin. The class name must stay
    /// "TaxRate" so nopCommerce's convention-based mapping resolves the same table.
    /// The Tax.FixedOrByCountryStateZip plugin must be installed for the table to exist.
    /// </summary>
    public class TaxRate : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier (0 = all stores)
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the tax category identifier
        /// </summary>
        public int TaxCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the state/province identifier (0 = all states)
        /// </summary>
        public int StateProvinceId { get; set; }

        /// <summary>
        /// Gets or sets the zip (empty = all zips)
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the percentage
        /// </summary>
        public decimal Percentage { get; set; }
    }
}
