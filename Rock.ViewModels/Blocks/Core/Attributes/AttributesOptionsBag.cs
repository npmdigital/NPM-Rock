﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.Attributes
{
    /// <summary>
    /// The additional configuration options for the Content Channel Item List block.
    /// </summary>
    public class AttributesOptionsBag
    {
        public List<string> HideColumns { get; set; }

        public Guid? EntityTypeGuid { get; set; }

        public int AttributeEntityTypeId { get; set; }

        public bool EnableShowInGrid { get; set; }

        public bool AllowSettingOfValues { get; set; }

        public List<ListItemBag> EntityTypes { get; set; }
    }
}