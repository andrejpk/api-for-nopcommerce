# API plugin for nopCommerce 4.90

<!-- Replace YOUR_USERNAME with your GitHub username/organization -->
[![CI Build](https://github.com/YOUR_USERNAME/api-for-nopcommerce/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USERNAME/api-for-nopcommerce/actions/workflows/ci.yml)

This plugin provides a RESTful API for managing resources in nopCommerce 4.9.
For the other versions of nopCommerce, please refer to the other releases and branches.

## Continuous Integration

This project uses GitHub Actions for CI/CD. The build process:

1. Checks out this repository and nopCommerce at the compatible version (currently `release-4.90.3`)
2. Builds nopCommerce core and framework
3. Builds the API plugin
4. Verifies the plugin output is in the correct location

The nopCommerce version is automatically pulled based on the `NOPCOMMERCE_VERSION` environment variable in the workflow file (`.github/workflows/ci.yml`). This ensures the build always uses a compatible version.

**Note:** Tests are currently disabled in CI because the test project uses .NET Framework 4.6.1, which is not supported on Linux runners. To enable tests, the test project needs to be upgraded to .NET 6+.

### Updating nopCommerce Version

To update the nopCommerce version used in CI and development:

1. Update `NOPCOMMERCE_VERSION` in `.github/workflows/ci.yml`
2. Update the version in both `setup-dev.sh` and `setup-dev.ps1`
3. Update `SupportedVersions` in `Nop.Plugin.Api/plugin.json`
4. Test the build locally before pushing changes

## Installation

### Quick Setup (Recommended)

Use the provided setup script to automatically clone and build nopCommerce at the correct version:

```bash
# Linux/macOS
./setup-dev.sh

# Windows (PowerShell)
.\setup-dev.ps1
```

The setup script will:
- Clone nopCommerce at the compatible version (release-4.90.3) if not already present
- Check for version mismatches in existing installations
- Build both nopCommerce and the API plugin
- Display next steps

### Manual Installation

1. clone the [NopCommerce](https://github.com/nopSolutions/nopCommerce) repository (tag `release-4.90.3` or compatible) into folder called `nopCommerce`
1. clone this repository into the same folder where the `nopCommerce` folder is located
1. build the nopCommerce solution
1. build the api-for-nopcommerce solution (the output will be placed inside the nopCommerce directory)
1. run the Nop.Web project in the nopCommerce solution
1. install the nopCommerce database, create the admin user (skip this step if already done)
1. go to the administration page, Api plugin should be listed in local plugins configuration section.
1. in the customers section assign the  role `Api Users` to the user that will be used to access the api.
1. set fake appId and app secret in facebook authentication plugin as a workaround for [issue #17](https://github.com/stepanbenes/api-for-nopcommerce/issues/17#issuecomment-840502748).
1. go to `/api/swagger` page and experiment with the api (use the Authorize button to authenticate requests) or run .NET 5 client application (`ClientApp` project) that contains auto-generated Api client class generated from the swagger json file using C# 9 source generator.
