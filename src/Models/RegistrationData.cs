// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Arctium.SimpleRegistration.Models;

class RegistrationData
{
    [Required(ErrorMessage = "User name is required.")]
    //[RegularExpression("^[^@]+@dev$", ErrorMessage = "User name is required.")]
    public string UserName { get; set; }

    [Required, MinLength(12, ErrorMessage = "The password must be at least 12 characters long.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Passwords do not match."), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string PasswordConfirmation { get; set; }

    [Compare(nameof(PrivacyPolicyExpected), ErrorMessage = "You must accept the Privacy Policy to register.")]
    public bool PrivacyPolicyAccepted { get; set; }

    public static bool PrivacyPolicyExpected => true;
}
