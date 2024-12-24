// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Arctium.SimpleRegistration.Models;

namespace Arctium.SimpleRegistration.Services;

interface IRegistrationService
{
    Task<(bool Success, string ErrorMessage)> RegisterAsync(RegistrationData registrationData, string userNameSuffix);
}
