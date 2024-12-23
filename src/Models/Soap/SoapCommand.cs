// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace Arctium.SimpleRegistration.Models.Soap;

public class SoapCommand
{
    [XmlElement("command")]
    public string CommandText { get; set; }
}
