// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace Arctium.SimpleRegistration.Models.Soap;

public class SoapBody
{
    [XmlElement("executeCommand", Namespace = "urn:TC")]
    public SoapCommand Command { get; set; }
}
